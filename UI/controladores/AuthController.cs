using System;
using System.Threading.Tasks;
using ControlAcceso.Application;

namespace ControlAcceso.UI.controladores
{
    public class AuthController
    {
        private readonly MyApp _app;
        private AuthWindow? _authWindow;
        private readonly AdminController _adminController;

        public AuthController(MyApp app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _adminController = new AdminController(app);
        }

        public void MostrarVentanaAutenticacion()
        {
            if (_authWindow == null || !_authWindow.IsLoaded)
            {
                _authWindow = new AuthWindow(this);
                _authWindow.ShowDialog();
            }
            else
            {
                _authWindow.Activate();
            }
        }

        public async Task<bool> AutenticarConPasswordAsync(string password)
        {
            bool valido = _app.ValidarPasswordAdmin(password);
            if (!valido)
            {
                _authWindow?.MostrarMensaje("Contraseña incorrecta.", false);
                return false;
            }

            _authWindow?.MostrarMensaje("Acceso correcto.", true);
            await Task.Delay(250);
            _authWindow?.Close();
            _adminController.MostrarVentanaAdmin();
            return true;
        }

        public async Task<bool> AutenticarConHuellaAsync()
        {
            var (exito, mensaje) = await _app.AutenticarAdministradorPorHuellaAsync();
            if (!exito)
            {
                _authWindow?.MostrarMensaje(mensaje, false);
                return false;
            }

            _authWindow?.MostrarMensaje("Acceso correcto.", true);
            await Task.Delay(250);
            _authWindow?.Close();
            _adminController.MostrarVentanaAdmin();
            return true;
        }
    }
}
