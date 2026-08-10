using System;
using System.Collections.Generic;
using ControlAcceso.Database;
using ControlAcceso.DTOs;

namespace ControlAcceso.Services
{
    public class DatabaseService
    {
        private readonly IDatabase _dbAdapter;

        // Inyectamos el contrato de la base de datos
        public DatabaseService(IDatabase dbAdapter)
        {
            _dbAdapter = dbAdapter;
        }

        #region --- Métodos de Empleado ---

        public List<EmpleadoDto> ObtenerEmpleados(EmpleadoFilter? filtro = null)
        {
            return _dbAdapter.ObtenerEmpleados(filtro ?? new EmpleadoFilter());
        }

        public bool RegistrarEmpleado(EmpleadoSaveDto empleado, out string mensajeError)
        {
            mensajeError = string.Empty;

            // Validaciones previas de la capa de negocio
            if (string.IsNullOrWhiteSpace(empleado.NombreCompleto))
            {
                mensajeError = "El nombre del empleado no puede estar vacío.";
                return false;
            }

            if (empleado.Cedula.Length > 20)
            {
                mensajeError = "La cédula ingresada excede el límite de 20 caracteres.";
                return false;
            }

            return _dbAdapter.AgregarEmpleado(empleado);
        }

        public bool ActualizarEmpleado(EmpleadoSaveDto empleado, out string mensajeError)
        {
            mensajeError = string.Empty;

            if (empleado.Id <= 0)
            {
                mensajeError = "Identificador de empleado no válido.";
                return false;
            }

            return _dbAdapter.ActualizarEmpleado(empleado);
        }

        public bool CambiarEstado(int id, bool activo)
        {
            return _dbAdapter.CambiarEstadoEmpleado(id, activo);
        }

        public EmpleadoDto? ObtenerEmpleadoPorId(int id)
{
    return _dbAdapter.ObtenerEmpleadoPorId(id);
}

        #endregion

        #region --- Métodos de Asistencia y Configuración ---

        public bool RegistrarAsistencia(int empleadoId, int tipoAsistencia)
        {
            return _dbAdapter.RegistrarAsistencia(new AsistenciaDto
            {
                EmpleadoID = empleadoId,
                Tipo = tipoAsistencia,
                PorAdministrador = false,
                Observacion = null
            });
        }

        public bool RegistrarAsistencia(AsistenciaDto asistencia)
        {
            return _dbAdapter.RegistrarAsistencia(asistencia);
        }

        public List<AsistenciaDto> ObtenerAsistenciasDelDia(DateTime fecha)
        {
            var filtro = new AsistenciaFilter
            {
                FechaInicio = fecha,
                FechaFin = fecha
            };

            return _dbAdapter.ObtenerAsistencias(filtro);
        }

        public List<AsistenciaDto> ObtenerAsistencias(AsistenciaFilter filtro)
        {
            return _dbAdapter.ObtenerAsistencias(filtro);
        }

        public bool GuardarConfiguracion(TimeSpan entrada, TimeSpan limite, string password)
        {
            return GuardarConfiguracion(entrada, limite, password, Array.Empty<HuellaEmpleadoDto>());
        }

        public bool GuardarConfiguracion(TimeSpan entrada, TimeSpan limite, string password, IReadOnlyList<HuellaEmpleadoDto> huellasAdmin)
        {
            return _dbAdapter.GuardarConfiguracion(password, entrada, limite, huellasAdmin);
        }

        public (string? Password, TimeSpan HoraEntrada, TimeSpan HoraLimite)? ObtenerConfiguracion()
        {
            var config = _dbAdapter.ObtenerConfiguracion();
            if (config == null) return null;

            var (password, horaEntrada, horaLimite) = config.Value;
            return (password, horaEntrada, horaLimite);
        }

        /// <summary>
        /// Reemplaza la contraseña de administrador por un valor ya hasheado
        /// (el hashing lo hace la capa de aplicación con el hasher de Identity).
        /// </summary>
        public bool ActualizarPasswordAdmin(string hashedPassword)
        {
            return _dbAdapter.ActualizarPasswordAdmin(hashedPassword);
        }

        public List<HuellaEmpleadoDto> ObtenerHuellasActivas()
        {
            return _dbAdapter.ObtenerHuellasActivas();
        }

        public List<HuellaEmpleadoDto> ObtenerHuellasAdmin()
        {
            return _dbAdapter.ObtenerHuellasAdmin();
        }

        #endregion

        public List<CargoDto> ObtenerCargos(bool soloActivos = false)
        {
            return _dbAdapter.ObtenerCargos(soloActivos);
        }

        public bool CrearCargo(string nombre)
        {
            return _dbAdapter.CrearCargo(nombre);
        }

        public bool CambiarEstadoCargo(int id, bool activo)
        {
            return _dbAdapter.CambiarEstadoCargo(id, activo);
        }

        // Dentro de DatabaseService:
        public bool ActualizarCargo(int id, string nombre) => _dbAdapter.ActualizarCargo(id, nombre);

        public List<EmpleadoDto> ObtenerEmpleadosPorRol(int rolId)
        {
            return _dbAdapter.ObtenerEmpleados(new EmpleadoFilter { RolId = rolId, SoloActivos = true });
        }
    }
}
