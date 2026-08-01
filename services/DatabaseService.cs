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
            try
            {
                return _dbAdapter.ObtenerEmpleados(filtro ?? new EmpleadoFilter());
            }
            catch (Exception)
            {
                // Aquí puedes registrar el error en log
                return new List<EmpleadoDto>();
            }
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

            try
            {
                return _dbAdapter.AgregarEmpleado(empleado);
            }
            catch (Exception ex)
            {
                mensajeError = $"Error al guardar en la base de datos: {ex.Message}";
                return false;
            }
        }

        public bool ActualizarEmpleado(EmpleadoSaveDto empleado, out string mensajeError)
        {
            mensajeError = string.Empty;

            if (empleado.Id <= 0)
            {
                mensajeError = "Identificador de empleado no válido.";
                return false;
            }

            try
            {
                return _dbAdapter.ActualizarEmpleado(empleado);
            }
            catch (Exception ex)
            {
                mensajeError = $"Error al actualizar empleado: {ex.Message}";
                return false;
            }
        }

        public bool CambiarEstado(int id, bool activo)
        {
            try
            {
                _dbAdapter.CambiarEstadoEmpleado(id, activo);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public EmpleadoDto? ObtenerEmpleadoPorId(int id)
{
    return _dbAdapter.ObtenerEmpleadoPorId(id);
}

        #endregion

        #region --- Métodos de Asistencia y Configuración ---

        public bool RegistrarAsistencia(int empleadoId, int tipoAsistencia)
        {
            try
            {
                _dbAdapter.RegistrarAsistencia(new AsistenciaDto
                {
                    EmpleadoID = empleadoId,
                    Tipo = tipoAsistencia
                });
                return true;
            }
            catch
            {
                return false;
            }
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

        public (string password, TimeSpan entrada, TimeSpan salida)? ObtenerConfiguracion()
        {
            return _dbAdapter.ObtenerConfiguracion();
        }

        public bool GuardarConfiguracion(TimeSpan entrada, TimeSpan salida, string password)
        {
            try
            {
                _dbAdapter.GuardarConfiguracion(password, entrada, salida);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<HuellaEmpleadoDto> ObtenerHuellasActivas()
        {
            try
            {
                return _dbAdapter.ObtenerHuellasActivas();
            }
            catch (Exception)
            {
                return new List<HuellaEmpleadoDto>();
            }
        }

        #endregion
    }
}
