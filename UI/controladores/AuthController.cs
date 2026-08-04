using System;
using System.Threading;
using System.Threading.Tasks;
using ControlAcceso.Application;

namespace ControlAcceso.UI.controladores
{
    public class AuthController
    {
        private readonly MyApp _app;
        private AuthWindow? _authWindow;
        private readonly Action _onAuthenticated;
        private CancellationTokenSource? _ctsCaptura;

        public AuthController(MyApp app, Action onAuthenticated)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _onAuthenticated = onAuthenticated ?? throw new ArgumentNullException(nameof(onAuthenticated));
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
            _onAuthenticated.Invoke();
            return true;
        }

        public async Task<bool> AutenticarConHuellaAsync()
        {
            _ctsCaptura?.Cancel();
            _ctsCaptura?.Dispose();
            _ctsCaptura = new CancellationTokenSource();
            var token = _ctsCaptura.Token;

            try
            {
                var (exito, mensaje) = await _app.AutenticarAdministradorPorHuellaAsync(token);

                if (token.IsCancellationRequested)
                {
                    // La ventana se cerró a medio escaneo; no toques la UI.
                    return false;
                }

                if (!exito)
                {
                    _authWindow?.MostrarMensaje(mensaje, false);
                    return false;
                }

                //_authWindow?.MostrarMensaje("Acceso correcto.", true);
                //await Task.Delay(250);
                _authWindow?.Close();
                _onAuthenticated.Invoke();
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
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
