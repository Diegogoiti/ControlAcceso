using System.Windows;
using System.Windows.Controls;

namespace ControlAcceso
{
    public class VentanaHuella : Window
    {
        public VentanaHuella(int idEmpleado)
        {
            Title = "Captura de Huella";
            Width = 300;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            StackPanel panel = new StackPanel { Margin = new Thickness(20) };

            TextBlock lblMensaje = new TextBlock
            {
                Text = $"Capturando huella para ID: {idEmpleado}",
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Button btnCerrar = new Button { Content = "Cerrar", Margin = new Thickness(0, 20, 0, 0) };
            btnCerrar.Click += (s, e) => this.Close();

            panel.Children.Add(lblMensaje);
            panel.Children.Add(btnCerrar);
            this.Content = panel;
        }
    }
}
