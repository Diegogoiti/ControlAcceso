using System.Windows;
using System.Threading; // Necesario para CancellationTokenSource
using ControlAcceso.Services;

namespace ControlAcceso
{
    public partial class VentanaHuella : Window
    {
        // 1. Declarar el campo de clase aquí
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public VentanaHuella()
        {
            InitializeComponent();
        }

        private async void BtnCapturar_Click(object sender, RoutedEventArgs e)
        {
            _cts = new CancellationTokenSource(); // Reinicia el token

            lblMensaje.Text = "Coloque su dedo en el lector...";

            // 2. Pasamos el token al servicio
            byte[]? rawData = await HardwareService.CapturarHuellaAsync(_cts.Token);

            if (rawData != null) {
                Scanner.GuardarComoImagen(rawData, 320, 480);
                lblMensaje.Text = "Huella capturada.";
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            _cts.Cancel(); // Esto detiene el bucle en HardwareService
            this.Close();
        }
    }
}
