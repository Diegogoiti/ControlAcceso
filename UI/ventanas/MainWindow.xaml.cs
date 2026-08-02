using System;
using System.Windows;
using System.Windows.Threading;
using ControlAcceso.UI.controladores;

namespace ControlAcceso
{
    public partial class MainWindow : Window
    {
        public readonly MainController? _controller;
        private readonly DispatcherTimer _relojTimer;
        private readonly DispatcherTimer _animacionTimer;
        private readonly DispatcherTimer _reinicioTimer;
        private int _contadorPuntos = 0;

        public MainWindow()
        {
            InitializeComponent();

            _relojTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _relojTimer.Tick += RelojTimer_Tick;
            _relojTimer.Start();

            _animacionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
            _animacionTimer.Tick += AnimacionTimer_Tick;

            _reinicioTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
            _reinicioTimer.Tick += ReinicioTimer_Tick;

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

        private void ReinicioTimer_Tick(object? sender, EventArgs e)
        {
            _reinicioTimer.Stop();
            PanelExito.Visibility = Visibility.Collapsed;
            PanelFallo.Visibility = Visibility.Collapsed;
            PanelDenegado.Visibility = Visibility.Collapsed;
            panelCaptahuellas.Visibility = Visibility.Collapsed;
            panelReloj.Visibility = Visibility.Visible;
            btnMarcarEntrada.IsEnabled = true;
            btnAdministrar.IsEnabled = true;
        }

        private void ActualizarHoraUI()
        {
            var ahora = DateTime.Now;
            lblHora.Text = ahora.ToString("hh:mm:ss tt");
            lblFecha.Text = ahora.ToString("dddd, d 'de' MMMM 'de' yyyy");
        }

        public void ModoEsperaHuella(bool esperando)
        {
            if (esperando)
            {
                _reinicioTimer.Stop(); // Detener cualquier reinicio pendiente
                panelReloj.Visibility = Visibility.Collapsed;
                PanelFallo.Visibility = Visibility.Collapsed;
                PanelExito.Visibility = Visibility.Collapsed;
                PanelDenegado.Visibility = Visibility.Collapsed;
                panelCaptahuellas.Visibility = Visibility.Visible;
                btnMarcarEntrada.IsEnabled = false;
                btnAdministrar.IsEnabled = false;
                _contadorPuntos = 2;
                _animacionTimer.Start();
            }
            else
            {
                _animacionTimer.Stop();
                panelCaptahuellas.Visibility = Visibility.Collapsed;
                // No mostramos el reloj aquí directamente, se manejará en MostrarResultadoMarcaje o al cancelar
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
                btnMarcarEntrada.IsEnabled = false;
                btnAdministrar.IsEnabled = false;
                await _controller.ProcesarMarcajeAsistenciaAsync(tipoAsistencia: 1);
            }
        }

        private void btnCancelarCaptura_Click(object sender, RoutedEventArgs e)
        {
            _controller?.CancelarCapturaHuella();
            // Restaurar estado visual inmediatamente al cancelar
            ModoEsperaHuella(false);
            panelReloj.Visibility = Visibility.Visible;
            btnMarcarEntrada.IsEnabled = true;
            btnAdministrar.IsEnabled = true;
        }

        private void btnAdministrar_Click(object sender, RoutedEventArgs e)
        {
            _controller?.AbrirRegistroEmpleado();
        }

        private void btnReportes_Click(object sender, RoutedEventArgs e)
        {
            _controller?.AbrirCentroDeReportes();
        }

        public void MostrarResultadoMarcaje(bool exito, bool denegado, string mensaje, string nombreEmpleado, DateTime hora)
        {
            _reinicioTimer.Stop(); // Por si acaso
            panelCaptahuellas.Visibility = Visibility.Collapsed;
            panelReloj.Visibility = Visibility.Collapsed;
            PanelExito.Visibility = Visibility.Collapsed;
            PanelFallo.Visibility = Visibility.Collapsed;
            PanelDenegado.Visibility = Visibility.Collapsed;

            if (exito)
            {
                TxtNombreEmpleado.Text = $"¡Bienvenido/a, {nombreEmpleado}!";
                TxtHoraEntrada.Text = $"Hora: {hora:hh:mm:ss tt}";
                PanelExito.Visibility = Visibility.Visible;
            }
            else if (denegado)
            {
                TxtMensajeDenegado.Text = mensaje;
                PanelDenegado.Visibility = Visibility.Visible;
            }
            else
            {
                TxtMensajeError.Text = mensaje;
                PanelFallo.Visibility = Visibility.Visible;
            }

            ReiniciarVistaTemporal();
        }

        private void ReiniciarVistaTemporal()
        {
            _reinicioTimer.Stop(); // Reinicia el temporizador si ya estaba corriendo
            _reinicioTimer.Start();
        }
    }
}
