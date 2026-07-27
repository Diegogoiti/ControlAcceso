using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ControlAcceso.Application;

namespace ControlAcceso.UI.controladores
{
    public class AdminController
    {
        private readonly MyApp _app;
        private AdminWindow? _adminWindow;

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
    }
}
