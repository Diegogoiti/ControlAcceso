using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ControlAcceso.DTOs;
using ControlAcceso.Services;

public record ResultadoMarcaje(bool Exito, string Nombre, DateTime? Hora, string Mensaje);

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
            try
            {
                HuellasCache = DatabaseService.ObtenerHuellasActivas();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error de conexión a la Base de Datos:\n{ex.Message}\n\nVerifica que el servicio MySQL esté activo.",
                    "Error de Conexión", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                HuellasCache = new List<HuellaEmpleadoDto>();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Ocurrió un error inesperado al cargar huellas:\n{ex.Message}",
                    "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                HuellasCache = new List<HuellaEmpleadoDto>();
            }
        }

        public void CargarEmpleadosViewCache()
        {
            try
            {
                var hoy = DateTime.Today;

                var empleadosActivos = DatabaseService.ObtenerEmpleados(new EmpleadoFilter { SoloActivos = true });
                var asistenciasHoy = DatabaseService.ObtenerAsistencias(new AsistenciaFilter
                {
                    FechaInicio = hoy,
                    FechaFin = hoy
                });

                var ultimasMarcasHoy = asistenciasHoy
                    .GroupBy(a => a.EmpleadoID)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderByDescending(a => a.Timestamp).FirstOrDefault()
                    );

                EmpleadosViewCache = empleadosActivos.Select(emp =>
                {
                    ultimasMarcasHoy.TryGetValue(emp.Id, out var ultimaMarca);

                    string estadoCalculado = ultimaMarca switch
                    {
                        { Tipo: 1 } => "Presente",
                        { Tipo: 2 } => "Retirado",
                        _ => "Inasistente"
                    };

                    return new EmpleadoViewDto
                    {
                        Id = emp.Id,
                        Nombre = emp.NombreCompleto,
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
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error de conexión a la Base de Datos:\n{ex.Message}\n\nVerifica que MySQL esté corriendo.",
                    "Error de Conexión", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                EmpleadosViewCache = new List<EmpleadoViewDto>();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Ocurrió un error inesperado al cargar la vista de empleados:\n{ex.Message}",
                    "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                EmpleadosViewCache = new List<EmpleadoViewDto>();
            }
        }

        #endregion

        #region --- Casos de Uso del Sistema ---

        public async Task<(bool Exito, EmpleadoDto? EmpleadoEncontrado, string Mensaje)> IdentificarEmpleadoPorHuellaAsync(CancellationToken cancellationToken = default)
        {
            byte[]? rawImage = await CaptahuellasService.IniciarCapturaAsync(cancellationToken);
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


            int? idEmpleado = BiometricService.IdentificarEmpleado(templateCapturado, this.HuellasCache);
            if (!idEmpleado.HasValue)
            {
                return (false, null, "Huella no reconocida. Acceso denegado.");
            }

            var empleado = empleadosActivos.Find(e => e.Id == idEmpleado.Value);
            return (true, empleado, "Empleado identificado con éxito.");
        }

        public async Task<(bool Exito, string Mensaje, string NombreEmpleado, DateTime Hora)> MarcarAsistenciaAsync(int tipoAsistencia, CancellationToken cancellationToken = default)
        {
            var resultado = await IdentificarEmpleadoPorHuellaAsync(cancellationToken);

            // 1. Fallo en la lectura/identificación biométrica
            if (!resultado.Exito || resultado.EmpleadoEncontrado == null)
            {
                return (false, "Intenta de nuevo o llama a tu supervisor", string.Empty, DateTime.Now);
            }

            // 2. Intento de registro en la base de datos
            bool guardado = DatabaseService.RegistrarAsistencia(resultado.EmpleadoEncontrado.Id, tipoAsistencia);
            if (!guardado)
            {
                return (false, "Error al registrar el marcado de asistencia en la base de datos.", string.Empty, DateTime.Now);
            }

            CargarEmpleadosViewCache();

            // 3. Éxito: retornamos la hora exacta y el nombre limpio para la vista
            DateTime horaActual = DateTime.Now;
            string tipoTexto = tipoAsistencia == 1 ? "Entrada" : "Salida";
            string mensajeExito = $"¡Marcado de {tipoTexto} exitoso!";

            return (true, mensajeExito, resultado.EmpleadoEncontrado.NombreCompleto, horaActual);
        }

        #endregion
    }
}
