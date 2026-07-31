using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ControlAcceso.Application;
using ControlAcceso.DTOs;

namespace ControlAcceso.UI.controladores
{
    public class AdminController
    {
        private readonly MyApp _app;
        private AdminWindow? _adminWindow;
        private CancellationTokenSource? _ctsCaptura;

        private readonly Dictionary<int, byte[]> _huellasCapturadas = new();

        public AdminController(MyApp app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        public void MostrarVentanaAdmin()
        {
            if (_adminWindow == null || !_adminWindow.IsLoaded)
            {
                _adminWindow = new AdminWindow(this);
                _app.CargarEmpleadosViewCache();
                CargarListaEmpleados();
                _adminWindow.ShowDialog();
            }
            else
            {
                _adminWindow.Activate();
            }
        }

        #region Limpieza y Gestión de Memoria
        public void LimpiarHuellasEnMemoria()
        {
            CancelarCaptura();
            _huellasCapturadas.Clear();
        }
        #endregion

        #region Métodos de Empleados

        public async Task<bool> RegistrarEmpleadoAsync(string cedula, string nombre, string apellido, string cargo)
        {
            if (string.IsNullOrWhiteSpace(cedula) || string.IsNullOrWhiteSpace(nombre))
            {
                throw new ArgumentException("La cédula y el nombre son campos obligatorios.");
            }

            await Task.Delay(100);
            return true;
        }

        public async Task<List<object>> ObtenerEmpleadosAsync(string filtroNombre = "", string estado = "Todos")
        {
            await Task.Delay(100);
            return new List<object>();
        }

        public async Task<bool> CambiarEstadoEmpleadoAsync(int empleadoId, bool activo)
        {
            await Task.Delay(100);
            return true;
        }

        #endregion

        #region Métodos de Configuración

        public async Task<bool> GuardarConfiguracionAsync(string parametro, string valor)
        {
            await Task.Delay(100);
            return true;
        }

        #endregion

        public void ProcesarGuardadoEmpleado()
        {
            if (_adminWindow == null) return;

            EmpleadoSaveDto emp = _adminWindow.ObtenerDatosFormulario();

            if (!EsFormularioValido(emp, requiereHuellas: true))
            {
                return;
            }

            int index = 0;
            foreach (var kvp in _huellasCapturadas)
            {
                emp.Huellas[index] = new HuellaEmpleadoDto
                {
                    Dedo = kvp.Key,
                    TemplateHuella = kvp.Value
                };
                index++;
            }

            try
            {
                _app.GuardarEmpleado(emp);
                _adminWindow.MostrarMensaje("Empleado guardado exitosamente.");
                _app.CargarEmpleadosViewCache();
                CargarListaEmpleados();
            }
            catch (Exception ex)
            {
                _adminWindow.MostrarError($"Error al guardar el empleado: {ex.Message}");
            }
        }

        public void ProcesarEdicionEmpleado()
        {
            if (_adminWindow == null || !_adminWindow.EmpleadoEditandoId.HasValue) return;

            EmpleadoSaveDto emp = _adminWindow.ObtenerDatosEdicionFormulario();

            if (!EsFormularioValido(emp, requiereHuellas: false))
            {
                return;
            }

            try
            {
                // Guarda la actualización a través de la aplicación
                _app.GuardarEmpleado(emp);
                _adminWindow.MostrarMensaje("Empleado actualizado correctamente.");
                _adminWindow.OcultarModalEdicion();
                _app.CargarEmpleadosViewCache();
                CargarListaEmpleados();
            }
            catch (Exception ex)
            {
                _adminWindow.MostrarError($"Error al actualizar el empleado: {ex.Message}");
            }
        }

        private bool EsFormularioValido(EmpleadoSaveDto emp, bool requiereHuellas = true)
        {
            emp.Cedula = emp.Cedula.Trim();
            emp.Telefono = emp.Telefono.Trim();
            emp.TelefonoEmergencia = emp.TelefonoEmergencia.Trim();

            if (string.IsNullOrWhiteSpace(emp.Cedula) || !emp.Cedula.All(char.IsDigit))
            {
                _adminWindow?.MostrarError("La cédula es obligatoria y debe contener solo números.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(emp.NombreCompleto))
            {
                _adminWindow?.MostrarError("El nombre completo es obligatorio.");
                return false;
            }

            if (emp.FechaNacimiento == default(DateOnly))
            {
                _adminWindow?.MostrarError("Debe seleccionar una fecha de nacimiento.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(emp.Telefono) || !emp.Telefono.All(char.IsDigit))
            {
                _adminWindow?.MostrarError("El teléfono principal es obligatorio y debe tener formato numérico.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(emp.TelefonoEmergencia) || !emp.TelefonoEmergencia.All(char.IsDigit))
            {
                _adminWindow?.MostrarError("El teléfono de emergencia es obligatorio y debe tener formato numérico.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(emp.Direccion))
            {
                _adminWindow?.MostrarError("La dirección de habitación es obligatoria.");
                return false;
            }

            if (requiereHuellas && _huellasCapturadas.Count < 3)
            {
                _adminWindow?.MostrarError("Debe registrar al menos tres huellas dactilares para el empleado.");
                return false;
            }

            return true;
        }

        public async Task CapturarHuellaDedoAsync(int numeroDedo)
        {
            _ctsCaptura?.Cancel();
            _ctsCaptura?.Dispose();

            _ctsCaptura = new CancellationTokenSource();
            var token = _ctsCaptura.Token;

            bool exitoCaptura = false;
            RestaurarEstadoTodosLosBotones(esperandoDedo: numeroDedo);

            try
            {
                if (_adminWindow?.EmpleadoEditandoId.HasValue == true)
                {
                    _adminWindow.EstablecerEstadoEsperandoHuellaEdicion(numeroDedo);
                }
                else
                {
                    _adminWindow?.EstablecerEstadoEsperandoHuella(numeroDedo);
                }

                byte[]? rawImage = await _app.IniciarCapturaAsync(token);

                if (rawImage == null || rawImage.Length == 0)
                {
                    if (token.IsCancellationRequested) return;
                    _adminWindow?.MostrarError("No se logró capturar la imagen del sensor o la operación fue cancelada.");
                    return;
                }

                if (token.IsCancellationRequested) return;

                if (!_app.ProcesarHuellaBruta(rawImage, out byte[]? templateCapturado, out string msgError))
                {
                    if (!token.IsCancellationRequested)
                    {
                        _adminWindow?.MostrarError(msgError);
                    }
                    return;
                }

                if (templateCapturado == null)
                {
                    if (!token.IsCancellationRequested)
                    {
                        _adminWindow?.MostrarError("Ocurrió un error inesperado al procesar el template biométrico.");
                    }
                    return;
                }

                if (!token.IsCancellationRequested)
                {
                    _huellasCapturadas[numeroDedo] = templateCapturado;
                    exitoCaptura = true;
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    _adminWindow?.MostrarError($"Error en el proceso de captura: {ex.Message}");
                }
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    bool estaRegistradoActualmente = exitoCaptura || _huellasCapturadas.ContainsKey(numeroDedo);
                    if (_adminWindow?.EmpleadoEditandoId.HasValue == true)
                    {
                        _adminWindow.ActualizarEstadoHuellaEdicion(numeroDedo, registrada: estaRegistradoActualmente);
                    }
                    else
                    {
                        _adminWindow?.ActualizarEstadoHuella(numeroDedo, registrada: estaRegistradoActualmente);
                    }
                }
            }
        }

        private void RestaurarEstadoTodosLosBotones(int esperandoDedo)
        {
            for (int i = 1; i <= 3; i++)
            {
                if (i != esperandoDedo)
                {
                    bool registrada = _huellasCapturadas.ContainsKey(i);
                    if (_adminWindow?.EmpleadoEditandoId.HasValue == true)
                    {
                        _adminWindow.ActualizarEstadoHuellaEdicion(i, registrada);
                    }
                    else
                    {
                        _adminWindow?.ActualizarEstadoHuella(i, registrada);
                    }
                }
            }
        }

        public void CancelarCaptura()
        {
            if (_ctsCaptura != null && !_ctsCaptura.IsCancellationRequested)
            {
                _ctsCaptura.Cancel();
                _ctsCaptura.Dispose();
                _ctsCaptura = null;
            }
        }

        public void CargarListaEmpleados(string filtroNombre = "", string estado = "Todos")
        {
            if (_adminWindow == null) return;

            var empleados = _app.EmpleadosViewCache.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filtroNombre))
            {
                empleados = empleados.Where(e =>
                    e.Nombre.Contains(filtroNombre, StringComparison.OrdinalIgnoreCase) ||
                    e.Cedula.Contains(filtroNombre));
            }

            if (estado != "Todos" && !string.IsNullOrWhiteSpace(estado))
            {
                empleados = empleados.Where(e => e.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase));
            }

            _adminWindow.MostrarListaEmpleados(empleados.ToList());
        }

        public void AbrirEdicionEmpleado(int idEmpleado)
        {
            if (_adminWindow == null) return;

            Console.WriteLine($"AbrirEdicionEmpleado: ID del empleado a editar: {idEmpleado}");

            // Muestra la UI inmediatamente pasando el ID
            _adminWindow.MostrarModalEdicion(idEmpleado);
        }
    }
}