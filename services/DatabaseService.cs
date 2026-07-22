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
                return _dbAdapter.ObtenerEmpleados(filtro);
            }
            catch (Exception)
            {
                // Aquí puedes registrar el error en log
                return new List<EmpleadoDto>();
            }
        }

        public bool RegistrarEmpleado(EmpleadoDto empleado, out string mensajeError)
        {
            mensajeError = string.Empty;

            // Validaciones previas de la capa de negocio
            if (string.IsNullOrWhiteSpace(empleado.Nombre))
            {
                mensajeError = "El nombre del empleado no puede estar vacío.";
                return false;
            }

            if (empleado.Cedula <= 0)
            {
                mensajeError = "La cédula ingresada no es válida.";
                return false;
            }

            try
            {
                _dbAdapter.AgregarEmpleado(empleado);
                return true;
            }
            catch (Exception ex)
            {
                mensajeError = $"Error al guardar en la base de datos: {ex.Message}";
                return false;
            }
        }

        public bool ActualizarEmpleado(EmpleadoDto empleado, out string mensajeError)
        {
            mensajeError = string.Empty;

            if (empleado.Id <= 0)
            {
                mensajeError = "Identificador de empleado no válido.";
                return false;
            }

            try
            {
                _dbAdapter.ActualizarEmpleado(empleado);
                return true;
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

        public (string password, TimeSpan entrada, TimeSpan salida) ObtenerConfiguracion()
        {
            return _dbAdapter.ObtenerConfiguracion();
        }

        public bool GuardarConfiguracion(TimeSpan entrada, TimeSpan salida, string password)
        {
            try
            {
                _dbAdapter.GuardarConfiguracion(entrada, salida, password);
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
