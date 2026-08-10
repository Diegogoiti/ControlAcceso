using System;
using System.Windows;
using System.Windows.Media;

namespace ControlAcceso
{
    public partial class CambiarPasswordWindow : Window
    {
        public string NuevaPassword => pwdNueva.Password;

        public CambiarPasswordWindow()
        {
            InitializeComponent();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(pwdNueva.Password))
            {
                MostrarError("La nueva contraseña no puede estar vacía.");
                return;
            }

            if (pwdNueva.Password != pwdConfirmar.Password)
            {
                MostrarError("La confirmación no coincide con la nueva contraseña.");
                return;
            }

            DialogResult = true;
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void MostrarError(string mensaje)
        {
            txtEstado.Text = mensaje;
            txtEstado.Foreground = Brushes.Firebrick;
        }
    }
}
