using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using ControlAcceso.DTOs;
using ControlAcceso.UI.controladores;

namespace ControlAcceso
{
    public partial class MainWindow : Window
    {
        private readonly MainController? _controller;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MainController controller) : this()
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public void MostrarEmpleados(IEnumerable<EmpleadoViewDto> empleados)
        {
            dgvEmpleados.ItemsSource = empleados;
        }

        public void SetEstadoCargando(bool cargando)
        {
            btnMarcarAsistencia.IsEnabled = !cargando;
            btnAdministrar.IsEnabled = !cargando;
        }

        public void MostrarMensaje(string mensaje, bool esExito)
        {
            MessageBoxImage icono = esExito ? MessageBoxImage.Information : MessageBoxImage.Error;
            string titulo = esExito ? "Éxito" : "Atención";
            MessageBox.Show(mensaje, titulo, MessageBoxButton.OK, icono);
        }

        private async void btnMarcarAsistencia_Click(object sender, RoutedEventArgs e)
        {
            if (_controller != null)
            {
                await _controller.ProcesarMarcajeAsistenciaAsync(tipoAsistencia: 1);
            }
        }

        private void btnAdministrar_Click(object sender, RoutedEventArgs e)
        {
            _controller?.AbrirRegistroEmpleado();
        }

        private void dgvEmpleados_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Evento para acciones al hacer doble clic sobre un registro
        }
    }
}
