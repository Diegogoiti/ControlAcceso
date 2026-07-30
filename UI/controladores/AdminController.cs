using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Método encargado de instanciar y gestionar la apertura del Panel de Administración.
        /// </summary>
        public void MostrarVentanaAdmin()
        {
            // Evitamos abrir múltiples instancias de la ventana si ya está abierta
            if (_adminWindow == null || !_adminWindow.IsLoaded)
            {
                _adminWindow = new AdminWindow(this); // Le pasamos este mismo controlador a la vista
                _adminWindow.ShowDialog(); // O _adminWindow.ShowDialog() si quieres que sea modal
            }
            else
            {
                _adminWindow.Activate(); // Si ya estaba abierta, la traemos al frente
            }
        }

        #region Métodos de Empleados

        public async Task<bool> RegistrarEmpleadoAsync(string cedula, string nombre, string apellido, string cargo)
        {
            Console.WriteLine("llamada a la fncion de registrar");
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
            if (_adminWindow == null)
            {
                Console.WriteLine("Error: La vista no está inicializada.");
                return;
            }
            // 1. La Vista captura y entrega los datos
            EmpleadoSaveDto emp = _adminWindow.ObtenerDatosFormulario();

            // 2. El Controlador valida la información
            if (!EsFormularioValido(emp))
            {
                Console.WriteLine("Validación fallida.");
                return; // Detiene el flujo si la validación falla
            }
            Console.WriteLine("Validación exitosa.");


            int index = 0;
            foreach (var kvp in _huellasCapturadas)
            {
                emp.Huellas[index] = new HuellaEmpleadoDto
                {
                    Dedo = kvp.Key,      // 1, 2 o 3
                    TemplateHuella = kvp.Value   // byte[]
                };
                index++;
            }

            try
            {
                _app.GuardarEmpleado(emp);
                Console.WriteLine("Empleado guardado exitosamente.");
                _adminWindow.MostrarMensaje("Empleado guardado exitosamente.");
            }
            catch (Exception ex)
            {
                _adminWindow.MostrarError($"Error al guardar el empleado: {ex.Message}");
            }

        }

        private bool EsFormularioValido(EmpleadoSaveDto emp)
        {
            emp.Cedula = emp.Cedula.Trim();
            emp.Telefono = emp.Telefono.Trim();
            emp.TelefonoEmergencia = emp.TelefonoEmergencia.Trim();


            // 1. Validar Cédula (no vacía y numérica)
            if (string.IsNullOrWhiteSpace(emp.Cedula))
            {
                _adminWindow?.MostrarError("La cédula es obligatoria.");
                return false;
            }

            if (!emp.Cedula.All(char.IsDigit))
            {
                _adminWindow?.MostrarError("La cédula debe contener solo números.");
                return false;
            }


            // 2. Validar Nombre Completo
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

            // 4. Validar Teléfono Principal (no vacío y formato numérico)
            if (string.IsNullOrWhiteSpace(emp.Telefono))
            {
                _adminWindow?.MostrarError("El teléfono principal es obligatorio.");
                return false;
            }

            if (!emp.Telefono.All(char.IsDigit))
            {
                _adminWindow?.MostrarError("El teléfono principal debe contener solo números.");
                return false;
            }

            // 5. Validar Teléfono de Emergencia (no vacío y formato numérico)
            if (string.IsNullOrWhiteSpace(emp.TelefonoEmergencia))
            {
                _adminWindow?.MostrarError("El teléfono de emergencia es obligatorio.");
                return false;
            }

            if (!emp.TelefonoEmergencia.All(char.IsDigit))
            {
                _adminWindow?.MostrarError("El teléfono de emergencia debe contener solo números.");
                return false;
            }

            // 6. Validar Dirección
            if (string.IsNullOrWhiteSpace(emp.Direccion))
            {
                _adminWindow?.MostrarError("La dirección de habitación es obligatoria.");
                return false;
            }

            if (_huellasCapturadas.Count < 3)
            {
                _adminWindow?.MostrarError("Debe registrar al menos tres huellas dactilares para el empleado.");
                return false;
            }





            return true;
        }
        public async Task CapturarHuellaDedoAsync(int numeroDedo)
        {
            // 1. Cancelar cualquier proceso de captura en curso
            _ctsCaptura?.Cancel();
            _ctsCaptura?.Dispose();

            // 2. Crear un nuevo CTS para la solicitud actual
            _ctsCaptura = new CancellationTokenSource();
            var token = _ctsCaptura.Token;

            bool exitoCaptura = false;

            // 3. Restaurar los otros botones a su estado normal (efecto selector de radio)
            RestaurarEstadoTodosLosBotones(esperandoDedo: numeroDedo);

            try
            {
                // 4. Feedback inmediato al botón seleccionado
                _adminWindow?.EstablecerEstadoEsperandoHuella(numeroDedo);

                // 5. Capturar pasando el token de esta ejecución activa
                byte[]? rawImage = await _app.IniciarCapturaAsync(token);

                if (rawImage == null || rawImage.Length == 0)
                {
                    if (token.IsCancellationRequested) return;

                    _adminWindow?.MostrarError("No se logró capturar la imagen del sensor o la operación fue cancelada.");
                    return;
                }

                if (token.IsCancellationRequested) return;

                // 6. Procesar el template
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

                // 7. Guardar template si esta tarea sigue siendo la activa
                if (!token.IsCancellationRequested)
                {
                    _huellasCapturadas[numeroDedo] = templateCapturado;
                    exitoCaptura = true;
                }
            }
            catch (OperationCanceledException)
            {
                // Cancelación controlada al cambiar de botón
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    _adminWindow?.MostrarError($"Error en el proceso de captura: {ex.Message}");
                }
            }
            finally
            {
                // 8. Restablecer el estado del botón solo si la llamada no fue sobreescrita por otra nueva
                if (!token.IsCancellationRequested)
                {
                    bool estaRegistradoActualmente = exitoCaptura || _huellasCapturadas.ContainsKey(numeroDedo);
                    _adminWindow?.ActualizarEstadoHuella(numeroDedo, registrada: estaRegistradoActualmente);
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
                    _adminWindow?.ActualizarEstadoHuella(i, registrada);
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
    }

}
