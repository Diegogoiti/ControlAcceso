using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControlAcceso.DTOs;
using ControlAcceso.Services;

namespace ControlAcceso.Application
{
    public class MyApp
    {
        private DatabaseService DatabaseService { get; }
        private BiometricService BiometricService { get; }
        private CaptahuellasService CaptahuellasService { get; }

        public IReadOnlyList<HuellaEmpleadoDto> HuellasCache { get; private set; } = new List<HuellaEmpleadoDto>();
        public IReadOnlyList<EmpleadoViewDto> EmpleadosViewCache { get; private set; } = new List<EmpleadoViewDto>();

        public MyApp(
            DatabaseService databaseService,
            BiometricService biometricService,
            CaptahuellasService captahuellasService)
        {
            DatabaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            BiometricService = biometricService ?? throw new ArgumentNullException(nameof(biometricService));
            CaptahuellasService = captahuellasService ?? throw new ArgumentNullException(nameof(captahuellasService));

            // Cargar estado inicial
            CargarHuellasActivas();
            CargarEmpleadosViewCache();
        }

        #region --- Métodos de Cache y Vista ---

        public void CargarHuellasActivas()
        {
            HuellasCache = DatabaseService.ObtenerHuellasActivas();
        }

        /// <summary>
        /// Consulta empleados y marcas del día a la BD para calcular la proyección que consume la interfaz.
        /// </summary>
        public void CargarEmpleadosViewCache()
        {
            var hoy = DateTime.Today;

            // 1. Obtenemos los datos frescos directamente para el cálculo local
            var empleadosActivos = DatabaseService.ObtenerEmpleados(new EmpleadoFilter { SoloActivos = true });
            var asistenciasHoy = DatabaseService.ObtenerAsistencias(new AsistenciaFilter
            {
                FechaInicio = hoy,
                FechaFin = hoy
            });

            // 2. Extraemos la última marca del día por cada empleado
            var ultimasMarcasHoy = asistenciasHoy
                .GroupBy(a => a.EmpleadoID)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(a => a.Timestamp).FirstOrDefault()
                );

            // 3. Mapeamos hacia el DTO de la vista
            EmpleadosViewCache = empleadosActivos.Select(emp =>
            {
                ultimasMarcasHoy.TryGetValue(emp.Id, out var ultimaMarca);

                string estadoCalculado = ultimaMarca switch
                {
                    { Tipo: 1 } => "Presente", // 1 = Entrada
                    { Tipo: 2 } => "Retirado", // 2 = Salida
                    _ => "Inasistente"
                };

                return new EmpleadoViewDto
                {
                    Id = emp.Id,
                    Nombre = emp.Nombre,
                    Cedula = emp.Cedula.ToString(),
                    Estado = estadoCalculado,
                    HoraEntrada = "No calculado",
                    HoraSalida = "No calculado",
                    Retraso = "No calculado",
                    TiempoExtra = "No calculado",
                    TotalLaborado = "Incompleto"
                };
            }).ToList();
        }

        #endregion

        #region --- Casos de Uso del Sistema ---

        public async Task<(bool Exito, EmpleadoDto? EmpleadoEncontrado, string Mensaje)> IdentificarEmpleadoPorHuellaAsync()
        {
            byte[]? rawImage = await CaptahuellasService.IniciarCapturaAsync();
            if (rawImage == null || rawImage.Length == 0)
            {
                return (false, null, "No se logró capturar la imagen del sensor o la operación fue cancelada.");
            }

            if (!BiometricService.ProcesarHuellaBruta(rawImage, out byte[]? templateCapturado, out string msgError))
            {
                return (false, null, msgError);
            }

            if (templateCapturado == null)
            {
                return (false, null, "Ocurrió un error inesperado al procesar el template biométrico.");
            }

            var empleadosActivos = DatabaseService.ObtenerEmpleados(new EmpleadoFilter { SoloActivos = true });
            if (empleadosActivos == null || empleadosActivos.Count == 0)
            {
                return (false, null, "No hay empleados activos registrados en la base de datos.");
            }

            int? idEmpleado = BiometricService.IdentificarEmpleado(templateCapturado, empleadosActivos);
            if (!idEmpleado.HasValue)
            {
                return (false, null, "Huella no reconocida. Acceso denegado.");
            }

            var empleado = empleadosActivos.Find(e => e.Id == idEmpleado.Value);
            return (true, empleado, "Empleado identificado con éxito.");
        }

        public async Task<(bool Exito, string Mensaje)> MarcarAsistenciaAsync(int tipoAsistencia)
        {
            var resultado = await IdentificarEmpleadoPorHuellaAsync();

            if (!resultado.Exito || resultado.EmpleadoEncontrado == null)
            {
                return (false, resultado.Mensaje);
            }

            bool guardado = DatabaseService.RegistrarAsistencia(resultado.EmpleadoEncontrado.Id, tipoAsistencia);
            if (!guardado)
            {
                return (false, "Error al registrar el marcado de asistencia en la base de datos.");
            }

            // Recalculamos la vista tras insertar el nuevo registro en la BD
            CargarEmpleadosViewCache();

            string tipoTexto = tipoAsistencia == 1 ? "Entrada" : "Salida";
            return (true, $"¡Marcado de {tipoTexto} exitoso! Bienvenido/a, {resultado.EmpleadoEncontrado.Nombre}.");
        }

        public async Task<(bool Exito, string Mensaje)> RegistrarNuevoEmpleadoConHuellaAsync(string nombre, int cedula)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "El nombre no puede estar vacío.");

            if (cedula <= 0)
                return (false, "La cédula no es válida.");

            byte[]? rawImage = await CaptahuellasService.IniciarCapturaAsync();
            if (rawImage == null)
            {
                return (false, "Lectura de huella cancelada o fallida.");
            }

            if (!BiometricService.ProcesarHuellaBruta(rawImage, out byte[]? templateGenerado, out string msgError))
            {
                return (false, msgError);
            }

            var todosLosEmpleados = DatabaseService.ObtenerEmpleados();
            if (BiometricService.ExisteHuellaDuplicada(templateGenerado!, todosLosEmpleados, out string nombreDuplicado))
            {
                return (false, $"Esta huella ya pertenece al empleado registrado: {nombreDuplicado}.");
            }

            var nuevoEmpleado = new EmpleadoDto
            {
                Nombre = nombre,
                Cedula = cedula,
                HuellaBytes = templateGenerado,
                Activo = true
            };

            bool exito = DatabaseService.RegistrarEmpleado(nuevoEmpleado, out string errorDb);

            if (exito)
            {
                CargarHuellasActivas();
                CargarEmpleadosViewCache();
            }

            return exito ? (true, "Empleado registrado correctamente con su biometría.") : (false, errorDb);
        }

        #endregion
    }
}
