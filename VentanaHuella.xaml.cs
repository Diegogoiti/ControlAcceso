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

                Empleado nuevo_empleado = new Empleado(0, "Diego Goitia", Random.Shared.Next(10000000, 99999999), template);

                _app.Db.AgregarEmpleado(nuevo_empleado);

                _app.CargarEmpleadosDesdeDb();
                 Console.WriteLine("Empleado agregado: " + nuevo_empleado.Nombre);
                lblMensaje.Text = "Empleado agregado: " + nuevo_empleado.Nombre;
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            _cts.Cancel();
            this.Close();
        }
    }
}
