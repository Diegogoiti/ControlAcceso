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
            // Ajustar al lunes de la semana pasada por la fecha seleccionada
            while (fechaInicioSemana.DayOfWeek != DayOfWeek.Monday)
            {
                fechaInicioSemana = fechaInicioSemana.AddDays(-1);
            }

            DateTime fechaFinSemana = fechaInicioSemana.AddDays(5); // Hasta el sábado

            var filtroEmpleados = new EmpleadoFilter { SoloActivos = true };
            var empleados = _databaseService.ObtenerEmpleados(filtroEmpleados);
            var config = _databaseService.ObtenerConfiguracion();
            TimeSpan horaLimite = config.HasValue ? config.Value.HoraLimite : new TimeSpan(9, 0, 0); // Default 9am si no hay

            var asistenciasFiltro = new AsistenciaFilter
            {
                FechaInicio = fechaInicioSemana,
                FechaFin = fechaFinSemana
            };

            var todasAsistencias = _databaseService.ObtenerAsistencias(asistenciasFiltro);
            var reportes = new List<ReporteEmpleadoDto>();

            foreach (var emp in empleados)
            {
                var reporte = new ReporteEmpleadoDto
                {
                    Cedula = emp.Cedula.ToString(),
                    Nombre = emp.NombreCompleto,
                    Posicion = emp.RolId == 1 ? "EMPLEADO" : "ADMINISTRADOR",
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
                    int diaDeLaSemana = (int)diaActual.DayOfWeek; // 1 = Lunes ... 6 = Sabado

                    // Si el día actual que estamos evaluando es mayor al día de hoy (futuro),
                    // lo dejamos en blanco y no evaluamos faltas.
                    if (diaActual.Date > DateTime.Today)
                    {
                        reporte.DiasAsistencia[diaDeLaSemana] = "";
                        continue;
                    }

                    diasEvaluados++;

                    // Buscamos si hay alguna entrada (Tipo == 1) o si es por administrador
                    var asistenciaDia = asistenciasEmpleado.FirstOrDefault(a => a.Timestamp.Date == diaActual.Date && (a.Tipo == 1 || a.PorAdministrador));

                    if (asistenciaDia != null)
                    {
                        if (asistenciaDia.PorAdministrador)
                        {
                            reporte.PorAdministrador++;
                        }

                        // Vino
                        if (!asistenciaDia.PorAdministrador && asistenciaDia.Timestamp.TimeOfDay > horaLimite)
                        {
                            reporte.DiasAsistencia[diaDeLaSemana] = "T"; // Tardanza
                            reporte.Tardanzas++;
                            reporte.DiasAsistidos++; // Se cuenta como asistido pero tarde
                        }
                        else
                        {
                            reporte.DiasAsistencia[diaDeLaSemana] = "A"; // Asistencia a tiempo (o justificada por admin)
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
    }
}
