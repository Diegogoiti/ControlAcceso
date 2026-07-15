using System.Windows;
using System.Threading;
using ControlAcceso.Services;

namespace ControlAcceso
{
    public partial class VentanaHuella : Window
    {
        // 1. Declarar el campo para el servicio
        private readonly FingerprintService _fingerprintService = new FingerprintService();
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public VentanaHuella()
        {
            InitializeComponent();
        }

        private async void BtnCapturar_Click(object sender, RoutedEventArgs e)
        {
            _cts = new CancellationTokenSource();

            lblMensaje.Text = "Coloque su dedo en el lector...";

            byte[]? rawData = await HardwareService.CapturarHuellaAsync(_cts.Token);

            if (rawData != null)
            {
                // 2. Usar la instancia _fingerprintService en lugar de llamar al servicio de forma estática
                var template = _fingerprintService.CrearTemplate(rawData);
                _fingerprintService.Guardar(template, "./template.bin");

                lblMensaje.Text = "Huella capturada.";
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            _cts.Cancel();
            this.Close();
        }
    }
}
