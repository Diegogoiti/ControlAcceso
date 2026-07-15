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
        private readonly MyApp _app;
        private readonly int _empleadoId;

        public VentanaHuella(MyApp app, int empleadoId)
        {
            InitializeComponent();
            _app = app;
            _empleadoId = empleadoId;
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

                var empleado = _app.ObtenerEmpleadoPorId(_empleadoId);

                if (empleado != null)
                {
                    bool similarity = _fingerprintService.Comparar(template, empleado.Huella);
                    lblMensaje.Text = $"Similitud: {similarity}";
                }

            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            _cts.Cancel();
            this.Close();
        }
    }
}
