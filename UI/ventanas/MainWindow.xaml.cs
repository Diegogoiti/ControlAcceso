using System;
using System.Windows;
using System.Windows.Threading;
using ControlAcceso.UI.controladores;

namespace ControlAcceso
{
    public partial class MainWindow : Window
    {
        private readonly MainController? _controller;
        private readonly DispatcherTimer _relojTimer;
        private readonly DispatcherTimer _animacionTimer;
        private int _contadorPuntos = 0;

        public MainWindow()
        {
            InitializeComponent();

            _relojTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _relojTimer.Tick += RelojTimer_Tick;
            _relojTimer.Start();

            _animacionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
            _animacionTimer.Tick += AnimacionTimer_Tick;

            ActualizarHoraUI();
        }

        public MainWindow(MainController controller) : this()
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        private void RelojTimer_Tick(object? sender, EventArgs e)
        {
            ActualizarHoraUI();
        }

        private void AnimacionTimer_Tick(object? sender, EventArgs e)
        {
            _contadorPuntos = (_contadorPuntos + 1) % 4;
            string puntos = new string('.', _contadorPuntos);
            lblMensajeSensor.Text = $"Coloque su dedo en el sensor\n{puntos}";
        }

        private void ActualizarHoraUI()
        {
            var ahora = DateTime.Now;
            lblHora.Text = ahora.ToString("HH:mm:ss");
            lblFecha.Text = ahora.ToString("dddd, d 'de' MMMM 'de' yyyy");
        }

        public void ModoEsperaHuella(bool esperando)
        {
            if (esperando)
            {
                panelReloj.Visibility = Visibility.Collapsed;
                panelCaptahuellas.Visibility = Visibility.Visible;
                btnMarcarEntrada.IsEnabled = false;
                btnAdministrar.IsEnabled = false;
                _contadorPuntos = 0;
                _animacionTimer.Start();
            }
            else
            {
                _animacionTimer.Stop();
                panelCaptahuellas.Visibility = Visibility.Collapsed;
                panelReloj.Visibility = Visibility.Visible;
                btnMarcarEntrada.IsEnabled = true;
                btnAdministrar.IsEnabled = true;
            }
        }

        public void SetEstadoCargando(bool cargando)
        {
            btnMarcarEntrada.IsEnabled = !cargando;
            btnAdministrar.IsEnabled = !cargando;
        }

        public void MostrarMensaje(string mensaje, bool esExito)
        {
            MessageBoxImage icono = esExito ? MessageBoxImage.Information : MessageBoxImage.Error;
            string titulo = esExito ? "Éxito" : "Atención";
            MessageBox.Show(mensaje, titulo, MessageBoxButton.OK, icono);
        }

        private async void btnMarcarEntrada_Click(object sender, RoutedEventArgs e)
        {
            if (_controller != null)
            {
                await _controller.ProcesarMarcajeAsistenciaAsync(tipoAsistencia: 1);
            }
        }

        private void btnCancelarCaptura_Click(object sender, RoutedEventArgs e)
        {
            _controller?.CancelarCapturaHuella();
        }

        private void btnAdministrar_Click(object sender, RoutedEventArgs e)
        {
            _controller?.AbrirRegistroEmpleado();
        }
    }
}
