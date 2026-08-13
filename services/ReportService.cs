using System;
using System.Collections.Generic;
using System.Linq;
using ControlAcceso.DTOs;

namespace ControlAcceso.Services
{
    public class ReportService
    {
        private readonly DatabaseService _databaseService;

        public ReportService(DatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }

        public List<ReporteEmpleadoDto> GenerarDatosReporteSemanal(DateTime fechaInicioSemana)
        {
            // Ajustar al lunes de la semana seleccionada
            while (fechaInicioSemana.DayOfWeek != DayOfWeek.Monday)
            {
                fechaInicioSemana = fechaInicioSemana.AddDays(-1);
            }

            DateTime fechaFinSemana = fechaInicioSemana.AddDays(5); // Hasta el sábado

            var filtroEmpleados = new EmpleadoFilter { SoloActivos = true };
            var empleados = _databaseService.ObtenerEmpleados(filtroEmpleados);

            // Obtener todos los cargos (roles) para mapear el nombre real
            var cargos = _databaseService.ObtenerCargos(false); // incluye inactivos por si algún empleado tiene uno desactivado

            var config = _databaseService.ObtenerConfiguracion();
            TimeSpan horaLimite = config.HasValue ? config.Value.HoraLimite : new TimeSpan(9, 0, 0); // Default 9am

            var asistenciasFiltro = new AsistenciaFilter
            {
                FechaInicio = fechaInicioSemana,
                FechaFin = fechaFinSemana
            };

            var todasAsistencias = _databaseService.ObtenerAsistencias(asistenciasFiltro);
            var reportes = new List<ReporteEmpleadoDto>();

            foreach (var emp in empleados)
            {
                // Obtener el nombre del cargo desde la lista de cargos
                string nombreCargo = cargos.FirstOrDefault(c => c.Id == emp.RolId)?.Nombre ?? "Sin cargo";

                var reporte = new ReporteEmpleadoDto
                {
                    Cedula = emp.Cedula.ToString(),
                    Nombre = emp.NombreCompleto,
                    Posicion = nombreCargo, // ← Ahora usa el nombre real de la tabla roles
                    DiasAsistidos = 0,
                    DiasFaltados = 0,
                    Tardanzas = 0,
                    PorAdministrador = 0
                };

                // Asistencias de este empleado específico
                var asistenciasEmpleado = todasAsistencias.Where(a => a.EmpleadoID == emp.Id).ToList();
                int diasEvaluados = 0;

                for (int i = 0; i < 6; i++) // Lunes a Sábado
                {
                    DateTime diaActual = fechaInicioSemana.AddDays(i);
                    int diaDeLaSemana = (int)diaActual.DayOfWeek; // 1 = Lunes ... 6 = Sábado

                    // Si el día es futuro, lo dejamos en blanco
                    if (diaActual.Date > DateTime.Today)
                    {
                        reporte.DiasAsistencia[diaDeLaSemana] = "";
                        continue;
                    }

                    diasEvaluados++;

                    // Buscar si hay una entrada (Tipo == 1) o si es por administrador
                    var asistenciaDia = asistenciasEmpleado.FirstOrDefault(a => a.Timestamp.Date == diaActual.Date && (a.Tipo == 1 || a.PorAdministrador));

                    if (asistenciaDia != null)
                    {
                        if (asistenciaDia.PorAdministrador)
                        {
                            // Regla de negocio: toda asistencia registrada por un
                            // administrador se computa como RETARDO JUSTIFICADO
                            // (el motivo va en la observación del registro).
                            reporte.PorAdministrador++;
                            reporte.DiasAsistencia[diaDeLaSemana] = "RJ"; // Retardo justificado
                            reporte.DiasAsistidos++; // Aun así, el día cuenta como trabajado
                        }
                        else if (asistenciaDia.Timestamp.TimeOfDay > horaLimite)
                        {
                            reporte.DiasAsistencia[diaDeLaSemana] = "T"; // Tardanza
                            reporte.Tardanzas++;
                            reporte.DiasAsistidos++; // Se cuenta como asistido pero tarde
                        }
                        else
                        {
                            reporte.DiasAsistencia[diaDeLaSemana] = "A"; // Asistencia a tiempo
                            reporte.DiasAsistidos++;
                        }
                    }
                    else
                    {
                        // No vino
                        reporte.DiasAsistencia[diaDeLaSemana] = "F"; // Falta
                        reporte.DiasFaltados++;
                    }
                }

                // Cálculo del porcentaje (en base a los días reales transcurridos de esa semana)
                if (diasEvaluados > 0)
                {
                    reporte.PorcentajeAsistencia = Math.Round((double)reporte.DiasAsistidos / (double)diasEvaluados * 100.0, 0);
                }
                else
                {
                    reporte.PorcentajeAsistencia = 0;
                }

                reportes.Add(reporte);
            }

            return reportes;
        }

        /// <summary>
        /// Datos del dashboard de un día: estadísticas, los marcajes en orden de
        /// hora (más reciente primero) y los empleados activos que aún no marcan.
        /// El estado de cada marcaje usa la misma regla que el reporte semanal:
        /// después de la hora límite configurada es "Tarde"; los registros hechos
        /// por el administrador (con observación) son "Retardo justificado".
        /// </summary>
        public DashboardDiaDto GenerarDatosDashboardDia(DateTime fecha)
        {
            var config = _databaseService.ObtenerConfiguracion();
            TimeSpan horaLimite = config.HasValue ? config.Value.HoraLimite : new TimeSpan(9, 0, 0);

            var empleados = _databaseService.ObtenerEmpleados(new EmpleadoFilter { SoloActivos = true });
            var cargos = _databaseService.ObtenerCargos(false);
            var asistencias = _databaseService.ObtenerAsistenciasDelDia(fecha);

            var marcajes = asistencias
                .Select(a =>
                {
                    var empleado = empleados.FirstOrDefault(e => e.Id == a.EmpleadoID);
                    return new AsistenciaDiaDto
                    {
                        Id = a.Id,
                        EmpleadoId = a.EmpleadoID,
                        Hora = a.Timestamp,
                        NombreEmpleado = empleado?.NombreCompleto ?? "Empleado desconocido",
                        Cargo = empleado != null
                            ? (cargos.FirstOrDefault(c => c.Id == empleado.RolId)?.Nombre ?? "Sin cargo")
                            : string.Empty,
                        // Regla de negocio unificada: los registros manuales del
                        // administrador son RETARDOS JUSTIFICADOS.
                        Estado = a.PorAdministrador
                            ? "Retardo justificado"
                            : (a.Timestamp.TimeOfDay > horaLimite ? "Tarde" : "A tiempo"),
                        Observacion = a.Observacion
                    };
                })
                .OrderByDescending(a => a.Hora)
                .ToList();

            var idsMarcados = asistencias.Select(a => a.EmpleadoID).Distinct().ToHashSet();
            var sinMarcar = empleados
                .Where(e => !idsMarcados.Contains(e.Id))
                .Select(e => new EmpleadoPendienteDto
                {
                    Id = e.Id,
                    Nombre = e.NombreCompleto,
                    Cargo = cargos.FirstOrDefault(c => c.Id == e.RolId)?.Nombre ?? "Sin cargo"
                })
                .OrderBy(e => e.Nombre)
                .ToList();

            return new DashboardDiaDto
            {
                Fecha = fecha,
                EmpleadosActivos = empleados.Count,
                MarcajesHoy = asistencias.Count,
                TardanzasHoy = marcajes.Count(m => m.Estado == "Tarde"),
                PorAdminHoy = marcajes.Count(m => m.Estado == "Retardo justificado"),
                Marcajes = marcajes,
                SinMarcar = sinMarcar
            };
        }

        /// <summary>
        /// Reporte detallado de un empleado entre dos fechas. Genera una fila por
        /// día (lunes a sábado, siguiendo la convención del reporte semanal):
        /// hora de entrada, estado y observación. Reglas:
        ///  - Registro manual del admin   → "Retardo justificado" (con su motivo).
        ///  - Entrada con huella tras la hora límite → "Retardo" (con minutos).
        ///  - Entrada con huella a tiempo → "A tiempo".
        ///  - Sin registro                → "Falta".
        /// </summary>
        public ReporteDetalladoEmpleadoDto GenerarDatosReporteDetallado(int empleadoId, DateTime desde, DateTime hasta)
        {
            var empleado = _databaseService.ObtenerEmpleadoPorId(empleadoId);
            if (empleado == null)
            {
                throw new InvalidOperationException("El empleado seleccionado no existe.");
            }

            var cargos = _databaseService.ObtenerCargos(false);
            string nombreCargo = cargos.FirstOrDefault(c => c.Id == empleado.RolId)?.Nombre ?? "Sin cargo";

            var config = _databaseService.ObtenerConfiguracion();
            TimeSpan horaLimite = config.HasValue ? config.Value.HoraLimite : new TimeSpan(9, 0, 0);

            var asistencias = _databaseService.ObtenerAsistencias(new AsistenciaFilter
            {
                EmpleadoId = empleadoId,
                FechaInicio = desde.Date,
                FechaFin = hasta.Date
            });

            // Los días futuros dentro del rango no se evalúan (igual que el semanal).
            DateTime fechaFin = hasta.Date > DateTime.Today ? DateTime.Today : hasta.Date;

            var dias = new List<ReporteDetalleDiaDto>();
            for (DateTime dia = desde.Date; dia <= fechaFin; dia = dia.AddDays(1))
            {
                if (dia.DayOfWeek == DayOfWeek.Sunday) continue; // Solo lunes a sábado

                var registrosDia = asistencias.Where(a => a.Timestamp.Date == dia.Date).ToList();

                ReporteDetalleDiaDto fila;
                if (registrosDia.Count == 0)
                {
                    fila = new ReporteDetalleDiaDto
                    {
                        Fecha = dia,
                        Dia = NombreDia(dia.DayOfWeek),
                        Estado = "Falta"
                    };
                }
                else
                {
                    var porAdmin = registrosDia.FirstOrDefault(a => a.PorAdministrador);
                    if (porAdmin != null)
                    {
                        // Regla de negocio: asistencia manual del admin = retardo justificado.
                        fila = new ReporteDetalleDiaDto
                        {
                            Fecha = dia,
                            Dia = NombreDia(dia.DayOfWeek),
                            HoraEntrada = porAdmin.Timestamp.TimeOfDay,
                            Estado = "Retardo justificado",
                            MinutosRetraso = 0,
                            Observacion = porAdmin.Observacion ?? string.Empty
                        };
                    }
                    else
                    {
                        var entrada = registrosDia.OrderBy(a => a.Timestamp.TimeOfDay).First();
                        int minutos = (int)(entrada.Timestamp.TimeOfDay - horaLimite).TotalMinutes;

                        fila = new ReporteDetalleDiaDto
                        {
                            Fecha = dia,
                            Dia = NombreDia(dia.DayOfWeek),
                            HoraEntrada = entrada.Timestamp.TimeOfDay,
                            Estado = minutos > 0 ? "Retardo" : "A tiempo",
                            MinutosRetraso = minutos > 0 ? minutos : 0,
                            Observacion = entrada.Observacion ?? string.Empty
                        };
                    }
                }

                dias.Add(fila);
            }

            int trabajados = dias.Count(d => d.Estado != "Falta");
            int faltas = dias.Count(d => d.Estado == "Falta");
            int retardos = dias.Count(d => d.Estado == "Retardo");
            int justificados = dias.Count(d => d.Estado == "Retardo justificado");

            return new ReporteDetalladoEmpleadoDto
            {
                EmpleadoId = empleado.Id,
                Cedula = empleado.Cedula.ToString(),
                Nombre = empleado.NombreCompleto,
                Cargo = nombreCargo,
                Desde = desde.Date,
                Hasta = fechaFin,
                Dias = dias,
                DiasTrabajados = trabajados,
                Faltas = faltas,
                Retardos = retardos,
                RetardosJustificados = justificados,
                PorcentajeAsistencia = dias.Count > 0
                    ? Math.Round((double)trabajados / dias.Count * 100.0, 0)
                    : 0
            };
        }

        private static string NombreDia(DayOfWeek dia)
        {
            return dia switch
            {
                DayOfWeek.Monday => "Lunes",
                DayOfWeek.Tuesday => "Martes",
                DayOfWeek.Wednesday => "Miércoles",
                DayOfWeek.Thursday => "Jueves",
                DayOfWeek.Friday => "Viernes",
                DayOfWeek.Saturday => "Sábado",
                _ => "Domingo"
            };
        }
    }
}
