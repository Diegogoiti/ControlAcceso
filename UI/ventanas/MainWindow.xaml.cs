using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using ControlAcceso.DTOs;
using ControlAcceso.UI.controladores;

namespace ControlAcceso
{
    public partial class MainWindow : Window
    {
        private readonly MainController? _controller;
        private readonly DispatcherTimer _relojTimer;

        public MainWindow()
        {
            InitializeComponent();

            // Sincronización continua a 200 ms para acoplarse al instante al cambio de segundo de Windows
            _relojTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _relojTimer.Tick += RelojTimer_Tick;
            _relojTimer.Start();

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

        private void ActualizarHoraUI()
        {
            var ahora = DateTime.Now;
            lblHora.Text = ahora.ToString("HH:mm:ss");
            lblFecha.Text = ahora.ToString("dddd, d 'de' MMMM 'de' yyyy");
        }

        public void MostrarEmpleados(IEnumerable<EmpleadoViewDto> empleados)
        {
            // Método mantenido para evitar romper la interfaz con MainController
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
                // Envía tipoAsistencia = 1 (Entrada)
                await _controller.ProcesarMarcajeAsistenciaAsync(tipoAsistencia: 1);
            }
        }

        private void btnAdministrar_Click(object sender, RoutedEventArgs e)
        {
            _controller?.AbrirRegistroEmpleado();
        }
    }
}
