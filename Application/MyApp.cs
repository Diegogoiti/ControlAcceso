using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ControlAcceso.DTOs;
using ControlAcceso.Services;
using Microsoft.AspNetCore.Identity;

public record ResultadoMarcaje(bool Exito, string Nombre, DateTime? Hora, string Mensaje);

namespace ControlAcceso.Application
{
    public class MyApp
    {
        // Hasher estándar de ASP.NET Core Identity: sal aleatoria, PBKDF2 con
        // 100k iteraciones y formato versionado, mantenido por Microsoft.
        private static readonly PasswordHasher<object> _hasher = new();

        private DatabaseService DatabaseService { get; }
        private BiometricService BiometricService { get; }
        private CaptahuellasService CaptahuellasService { get; }

        public IReadOnlyList<HuellaEmpleadoDto> HuellasCache { get; private set; } = new List<HuellaEmpleadoDto>();
        public IReadOnlyList<EmpleadoViewDto> EmpleadosViewCache { get; private set; } = new List<EmpleadoViewDto>();

        public DatabaseService Db => DatabaseService;

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
                var empleados = DatabaseService.ObtenerEmpleados(new EmpleadoFilter { });
                // Obtener todos los roles (activos e inactivos) para que el nombre del rol se muestre incluso si el rol fue desactivado después
                var roles = DatabaseService.ObtenerCargos(false);

                EmpleadosViewCache = empleados.Select(emp => new EmpleadoViewDto
                {
                    Id = emp.Id,
                    Nombre = emp.NombreCompleto,
                    Cedula = emp.Cedula.ToString(),
                    Estado = emp.Activo ? "Activo" : "Inactivo",
                    FechaNacimiento = emp.FechaNacimiento,
                    Direccion = emp.Direccion,
                    Telefono = emp.Telefono,
                    TelefonoEmergencia = emp.TelefonoEmergencia,
                    NombreRol = roles.FirstOrDefault(r => r.Id == emp.RolId)?.Nombre ?? "Sin rol",
                    FechaIngreso = emp.FechaIngreso
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

        public async Task<(bool Exito, bool Denegado, string Mensaje, string NombreEmpleado, DateTime Hora)> MarcarAsistenciaAsync(int tipoAsistencia, CancellationToken cancellationToken = default)
        {
            var resultado = await IdentificarEmpleadoPorHuellaAsync(cancellationToken);

            // Si el token fue cancelado antes o durante la lectura, lanzamos la excepción
            // para que el controlador lo atrape en el catch (OperationCanceledException)
            cancellationToken.ThrowIfCancellationRequested();

            // 1. Fallo en la lectura/identificación biométrica
            if (!resultado.Exito || resultado.EmpleadoEncontrado == null)
            {
                return (false, false, "Intenta de nuevo o llama a tu supervisor", string.Empty, DateTime.Now);
            }

            DateTime horaActual = DateTime.Now;

            // 2. Validación de hora límite (solo para entradas)
            if (tipoAsistencia == 1)
            {
                var config = DatabaseService.ObtenerConfiguracion();
                if (config.HasValue)
                {
                    /*TimeSpan horaLimite = config.Value.HoraLimite;
                    if (horaActual.TimeOfDay > horaLimite)
                    {
                        return (false, true, "Acceso denegado: Se ha excedido la hora límite de entrada configurada. El registro se encuentra bloqueado. Contacte a su supervisor para autorizar una excepción.", string.Empty, horaActual);
                    }*/
                }
            }

            // 3. Intento de registro en la base de datos
            bool guardado = DatabaseService.RegistrarAsistencia(resultado.EmpleadoEncontrado.Id, tipoAsistencia);
            if (!guardado)
            {
                return (false, false, "Error al registrar el marcado de asistencia en la base de datos.", string.Empty, horaActual);
            }

            CargarEmpleadosViewCache();

            // 4. Éxito: retornamos la hora exacta y el nombre limpio para la vista
            string tipoTexto = tipoAsistencia == 1 ? "Entrada" : "Salida";
            string mensajeExito = $"¡Marcado de {tipoTexto} exitoso!";

            return (true, false, mensajeExito, resultado.EmpleadoEncontrado.NombreCompleto, horaActual);
        }

        #endregion

        public async Task<byte[]?> IniciarCapturaAsync(CancellationToken cancellationToken = default)
        {
            return await CaptahuellasService.IniciarCapturaAsync(cancellationToken);
        }

        public bool ProcesarHuellaBruta(byte[] rawImage, out byte[]? templateCapturado, out string mensajeError)
        {
            return BiometricService.ProcesarHuellaBruta(rawImage, out templateCapturado, out mensajeError);
        }

        /// <summary>
        /// Verificación 1:1 entre dos templates de la misma huella. Se usa para
        /// validar que una lectura recién capturada es consistente (doble lectura)
        /// antes de guardarla definitivamente.
        /// </summary>
        public bool VerificarCoincidenciaHuella(byte[] templateCapturado, byte[] templateVerificacion)
        {
            return BiometricService.VerificarCoincidencia(templateCapturado, templateVerificacion, 50.0);
        }

        public List<HuellaEmpleadoDto> ObtenerHuellasDeEmpleado(int empleadoId)
        {
            return DatabaseService.ObtenerHuellasDeEmpleado(empleadoId);
        }

        public (bool, string) GuardarEmpleado(EmpleadoSaveDto emp)
        {
            var mensajeError = string.Empty;
            var exito = DatabaseService.RegistrarEmpleado(emp, out mensajeError);

            if (exito)
            {
                CargarEmpleadosViewCache();
                CargarHuellasActivas();
            }

            return (exito, mensajeError);
        }

        public async Task<bool> CambiarEstadoEmpleadoAsync(int idEmpleado, bool activo)
        {
            bool exito = await Task.Run(() => DatabaseService.CambiarEstado(idEmpleado, activo));
            if (exito)
            {
                // Actualiza la lista en memoria (EmpleadosViewCache)
                CargarEmpleadosViewCache();
            }
            return exito;
        }

        public EmpleadoDto? ObtenerEmpleadoPorId(int id)
        {
            return DatabaseService.ObtenerEmpleadoPorId(id);
        }

        public (bool Exito, string Mensaje) EditarEmpleado(EmpleadoSaveDto emp)
        {
            var exito = DatabaseService.ActualizarEmpleado(emp, out string mensajeError);
            if (exito)
            {
                // Refrescar el caché en memoria tras editar[cite: 9]
                CargarEmpleadosViewCache();
            }
            return (exito, mensajeError);
        }

        public bool GuardarConfiguracion(string password, TimeSpan horaEntrada, TimeSpan horaLimite, IReadOnlyList<HuellaEmpleadoDto> huellasAdmin)
        {
            return DatabaseService.GuardarConfiguracion(horaEntrada, horaLimite, password, huellasAdmin);
        }

        public List<HuellaEmpleadoDto> ObtenerHuellasAdmin()
        {
            return DatabaseService.ObtenerHuellasAdmin();
        }

        public bool RegistrarAsistenciaAdministrador(int empleadoId, int tipoAsistencia, string observacion)
        {
            var asistencia = new AsistenciaDto
            {
                EmpleadoID = empleadoId,
                Tipo = tipoAsistencia,
                PorAdministrador = true,
                Observacion = observacion?.Trim()
            };

            bool exito = DatabaseService.RegistrarAsistencia(asistencia);
            if (exito)
            {
                CargarEmpleadosViewCache();
            }

            return exito;
        }

        public bool ValidarPasswordAdmin(string password)
        {
            var config = ObtenerConfiguracion();
            if (config == null) return false;

            string? almacenada = config.Value.Password;
            if (string.IsNullOrEmpty(almacenada)) return false;

            // 1) Formato estándar de Identity: verificación directa. Si el hash
            //    es válido pero usa parámetros antiguos, Identity avisa con
            //    SuccessRehashNeeded y aprovechamos para actualizarlo en caliente.
            var resultado = _hasher.VerifyHashedPassword(null!, almacenada, password);
            if (resultado == PasswordVerificationResult.Success)
                return true;

            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                DatabaseService.ActualizarPasswordAdmin(_hasher.HashPassword(null!, password));
                return true;
            }

            // 2) Formato legacy (Base64 de texto plano de instalaciones viejas):
            //    comparar en claro y migrar al hash de Identity si coincide.
            if (VerificarPasswordLegacy(almacenada, password, out bool coincide))
            {
                if (coincide)
                {
                    DatabaseService.ActualizarPasswordAdmin(_hasher.HashPassword(null!, password));
                }
                return coincide;
            }

            return false;
        }

        /// <summary>
        /// Intenta interpretar un valor almacenado como el formato viejo (Base64
        /// de texto plano). Devuelve true si pudo decodificarlo y, en ese caso,
        /// indica en <paramref name="coincide"/> si la contraseña ingresada es la
        /// misma. Devuelve false si ni siquiera es Base64 válido (no es legacy).
        /// </summary>
        private static bool VerificarPasswordLegacy(string almacenada, string password, out bool coincide)
        {
            coincide = false;
            try
            {
                string textoPlano = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(almacenada));
                coincide = string.Equals(password ?? string.Empty, textoPlano, StringComparison.Ordinal);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Cambia la contraseña de administrador. Quien llega a la ventana de
        /// configuración ya pasó la autenticación, por lo que no se exige la
        /// contraseña actual para autorizar el cambio.
        /// </summary>
        public (bool Exito, string Mensaje) CambiarPasswordAdmin(string nuevaPassword)
        {
            if (string.IsNullOrWhiteSpace(nuevaPassword))
            {
                return (false, "La nueva contraseña no puede estar vacía.");
            }

            bool exito = DatabaseService.ActualizarPasswordAdmin(_hasher.HashPassword(null!, nuevaPassword));
            return exito
                ? (true, "Contraseña actualizada correctamente.")
                : (false, "No se pudo actualizar la contraseña en la base de datos.");
        }

        public async Task<(bool Exito, string Mensaje)> AutenticarAdministradorPorHuellaAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            byte[]? rawImage = await CaptahuellasService.IniciarCapturaAsync(cancellationToken);
            if (rawImage == null || rawImage.Length == 0)
            {
                return (false, "No se pudo capturar la huella del administrador.");
            }

            if (!BiometricService.ProcesarHuellaBruta(rawImage, out byte[]? templateCapturado, out string mensajeError))
            {
                return (false, mensajeError);
            }

            var huellasAdmin = DatabaseService.ObtenerHuellasAdmin();
            foreach (var huella in huellasAdmin)
            {
                if (huella.TemplateHuella == null || huella.TemplateHuella.Length == 0)
                    continue;

                if (BiometricService.VerificarCoincidencia(templateCapturado!, huella.TemplateHuella, 50.0))
                {
                    return (true, "Huella de administrador reconocida.");
                }
            }

            return (false, "La huella no coincide con la registrada para el administrador.");
        }

        public (string? Password, TimeSpan HoraEntrada, TimeSpan HoraLimite)? ObtenerConfiguracion()
        {
            return DatabaseService.ObtenerConfiguracion();
        }

        // Dentro de la clase MyApp:
        public List<CargoDto> ObtenerTodosLosCargos() => DatabaseService.ObtenerCargos(false);
        public bool CrearCargo(string nombre) => DatabaseService.CrearCargo(nombre);
        public bool ActualizarCargo(int id, string nombre) => DatabaseService.ActualizarCargo(id, nombre);
        public bool CambiarEstadoCargo(int id, bool activo) => DatabaseService.CambiarEstadoCargo(id, activo);

        public List<EmpleadoDto> ObtenerEmpleadosPorRol(int rolId)
        {
            return DatabaseService.ObtenerEmpleados(new EmpleadoFilter { RolId = rolId, SoloActivos = true });
        }

    }
}
