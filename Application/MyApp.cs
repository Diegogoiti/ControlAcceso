using System;
using System.Collections.Generic;
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

        public IReadOnlyList<HuellaEmpleadoDto> HuellasCache { get; private set; }

        public MyApp(
            DatabaseService databaseService,
            BiometricService biometricService,
            CaptahuellasService captahuellasService)
        {
            DatabaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            BiometricService = biometricService ?? throw new ArgumentNullException(nameof(biometricService));
            CaptahuellasService = captahuellasService ?? throw new ArgumentNullException(nameof(captahuellasService));

            CargarHuellasActivas();
        }



        /// <summary>
        /// Orquesta la captura biométrica desde el hardware, procesa los bytes crudos y busca coincidencias en 1:N.
        /// </summary>
        public async Task<(bool Exito, EmpleadoDto? EmpleadoEncontrado, string Mensaje)> IdentificarEmpleadoPorHuellaAsync()
        {
            // 1. Escuchar la huella desde el sensor
            byte[]? rawImage = await CaptahuellasService.IniciarCapturaAsync();
            if (rawImage == null || rawImage.Length == 0)
            {
                return (false, null, "No se logró capturar la imagen del sensor o la operación fue cancelada.");
            }

            // 2. Transformar la imagen bruta a un template biométrico
            if (!BiometricService.ProcesarHuellaBruta(rawImage, out byte[]? templateCapturado, out string msgError))
            {
                return (false, null, msgError);
            }

            if (templateCapturado == null)
            {
                return (false, null, "Ocurrió un error inesperado al procesar el template biométrico.");
            }

            // 3. Obtener los empleados activos de la base de datos
            var empleadosActivos = DatabaseService.ObtenerEmpleados(new EmpleadoFilter { SoloActivos = true });
            if (empleadosActivos == null || empleadosActivos.Count == 0)
            {
                return (false, null, "No hay empleados activos registrados en la base de datos.");
            }

            // 4. Comparar el template obtenido contra la base de datos (1:N)
            int? idEmpleado = BiometricService.IdentificarEmpleado(templateCapturado, empleadosActivos);
            if (!idEmpleado.HasValue)
            {
                return (false, null, "Huella no reconocida. Acceso denegado.");
            }

            var empleado = empleadosActivos.Find(e => e.Id == idEmpleado.Value);
            return (true, empleado, "Empleado identificado con éxito.");
        }

        /// <summary>
        /// Flujo completo para marcar entrada o salida mediante el sensor.
        /// </summary>
        public async Task<(bool Exito, string Mensaje)> MarcarAsistenciaAsync(int tipoAsistencia)
        {
            var resultado = await IdentificarEmpleadoPorHuellaAsync();

            if (!resultado.Exito || resultado.EmpleadoEncontrado == null)
            {
                return (false, resultado.Mensaje);
            }

            // Registrar el evento de asistencia en DB
            bool guardado = DatabaseService.RegistrarAsistencia(resultado.EmpleadoEncontrado.Id, tipoAsistencia);
            if (!guardado)
            {
                return (false, "Error al registrar el marcado de asistencia en la base de datos.");
            }

            string tipoTexto = tipoAsistencia == 1 ? "Entrada" : "Salida";
            return (true, $"¡Marcado de {tipoTexto} exitoso! Bienvenido/a, {resultado.EmpleadoEncontrado.Nombre}.");
        }

        /// <summary>
        /// Orquesta el registro de un nuevo empleado incluyendo la verificación de duplicados de huella.
        /// </summary>
        public async Task<(bool Exito, string Mensaje)> RegistrarNuevoEmpleadoConHuellaAsync(string nombre, int cedula)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "El nombre no puede estar vacío.");

            if (cedula <= 0)
                return (false, "La cédula no es válida.");

            // 1. Escuchar la huella
            byte[]? rawImage = await CaptahuellasService.IniciarCapturaAsync();
            if (rawImage == null)
            {
                return (false, "Lectura de huella cancelada o fallida.");
            }

            // 2. Convertir imagen bruta a template
            if (!BiometricService.ProcesarHuellaBruta(rawImage, out byte[]? templateGenerado, out string msgError))
            {
                return (false, msgError);
            }

            // 3. Validar si la huella ya pertenece a otra persona
            var todosLosEmpleados = DatabaseService.ObtenerEmpleados();
            if (BiometricService.ExisteHuellaDuplicada(templateGenerado!, todosLosEmpleados, out string nombreDuplicado))
            {
                return (false, $"Esta huella ya pertenece al empleado registrado: {nombreDuplicado}.");
            }

            // 4. Guardar en Base de Datos
            var nuevoEmpleado = new EmpleadoDto
            {
                Nombre = nombre,
                Cedula = cedula,
                HuellaBytes = templateGenerado,
                Activo = true
            };

            bool exito = DatabaseService.RegistrarEmpleado(nuevoEmpleado, out string errorDb);
            return exito ? (true, "Empleado registrado correctamente con su biometría.") : (false, errorDb);
        }


        public List<AsistenciaDto> ObtenerAsistenciasDelDia()
        {
            var hoy = DateTime.Today;
            return DatabaseService.ObtenerAsistencias(new AsistenciaFilter
            {
                FechaInicio = hoy,
                FechaFin = hoy
            });
        }

        public List<HuellaEmpleadoDto> ObtenerHuellasActivas()
        {
            return DatabaseService.ObtenerHuellasActivas();
        }

        public void CargarHuellasActivas()
        {
            HuellasCache = DatabaseService.ObtenerHuellasActivas();

        }
    }

}
