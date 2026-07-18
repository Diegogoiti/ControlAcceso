using System;
using System.Windows;

namespace ControlAcceso
{
    public partial class VentanaContrasena : Window
    {
        private readonly string _contrasenaCorrecta;

        public VentanaContrasena(string contrasenaCorrecta)
        {
            InitializeComponent();
            _contrasenaCorrecta = contrasenaCorrecta;
        }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Password == _contrasenaCorrecta)
            {
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Contraseña incorrecta", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
