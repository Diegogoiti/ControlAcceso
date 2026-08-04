using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace ControlAcceso
{
    public partial class AuthWindow : Window
    {
        private readonly UI.controladores.AuthController _controller;

        public AuthWindow(UI.controladores.AuthController controller)
        {
            InitializeComponent();
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        private async void btnEntrar_Click(object sender, RoutedEventArgs e)
        {
            btnEntrar.IsEnabled = false;
            btnUsarHuella.IsEnabled = false;

            try
            {
                bool exito = await _controller.AutenticarConPasswordAsync(pwdPassword.Password);
                if (!exito)
                {
                    MostrarMensaje("Contraseña incorrecta. Intenta nuevamente.", false);
                }
            }
            finally
            {
                btnEntrar.IsEnabled = true;
                btnUsarHuella.IsEnabled = true;
            }
        }

        private async void btnUsarHuella_Click(object sender, RoutedEventArgs e)
        {
            btnEntrar.IsEnabled = false;
            btnUsarHuella.IsEnabled = false;

            try
            {
                bool exito = await _controller.AutenticarConHuellaAsync();
                if (!exito)
                {
                    MostrarMensaje("No se reconoció la huella de administrador.", false);
                }
            }
            finally
            {
                btnEntrar.IsEnabled = true;
                btnUsarHuella.IsEnabled = true;
            }
        }

        private void AuthWindow_Closing(object sender, CancelEventArgs e)
        {
            // Si el sensor está escaneando cuando se cierra la ventana, cancela
            // la captura para liberar el dispositivo (ftrScanCloseDevice se
            // dispara en el finally del adaptador al cancelarse el token).
            _controller.CancelarCaptura();
        }

        public void MostrarMensaje(string mensaje, bool esExito)
        {
            txtEstado.Text = mensaje;
            txtEstado.Foreground = esExito ? Brushes.Green : Brushes.Firebrick;
        }
    }
}
