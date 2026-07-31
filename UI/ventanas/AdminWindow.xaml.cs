using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ControlAcceso.DTOs;
using ControlAcceso.UI.controladores;

namespace ControlAcceso
{
    public partial class AdminWindow : Window
    {
        private readonly AdminController? _controller;

        // Constructor para diseñador de WPF o fallback
        public AdminWindow()
        {
            InitializeComponent();
            this.Closing += AdminWindow_Closing;
        }

        // Constructor principal con inyección de controlador
        public AdminWindow(AdminController controller) : this()
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        private void CambiarPestaña(UIElement panelActivo)
        {
            PanelRegistrar.Visibility = Visibility.Collapsed;
            PanelEmpleados.Visibility = Visibility.Collapsed;
            PanelConfiguracion.Visibility = Visibility.Collapsed;

            panelActivo.Visibility = Visibility.Visible;
        }

        private void btnTabRegistrar_Click(object sender, RoutedEventArgs e) => CambiarPestaña(PanelRegistrar);
        private void btnTabEmpleados_Click(object sender, RoutedEventArgs e) => CambiarPestaña(PanelEmpleados);
        private void btnTabConfig_Click(object sender, RoutedEventArgs e) => CambiarPestaña(PanelConfiguracion);

        public EmpleadoSaveDto ObtenerDatosFormulario()
        {
            var datosCrudos = (
                Cedula: txtCedula.Text,
                NombreCompleto: txtNombreCompleto.Text,
                FechaNacimiento: dpFechaNacimiento.SelectedDate,
                Telefono: txtTelefono.Text,
                TelefonoEmergencia: txtTelefonoEmergencia.Text,
                Direccion: txtDireccion.Text,
                RolTexto: (cmbRol.SelectedItem as ComboBoxItem)?.Content?.ToString()
            );

            var nuevoEmpleado = new EmpleadoSaveDto
            {
                Cedula = datosCrudos.Cedula,
                NombreCompleto = datosCrudos.NombreCompleto,
                FechaNacimiento = datosCrudos.FechaNacimiento.HasValue ? DateOnly.FromDateTime(datosCrudos.FechaNacimiento.Value) : default,
                Telefono = datosCrudos.Telefono,
                TelefonoEmergencia = datosCrudos.TelefonoEmergencia,
                Direccion = datosCrudos.Direccion,
                RolId = cmbRol.SelectedIndex
            };

            return nuevoEmpleado;
        }

        private void btnGuardarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            _controller?.ProcesarGuardadoEmpleado();
        }

        public void MostrarError(string mensaje)
        {
            MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private async void btnCapturarDedo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int numeroDedo = Convert.ToInt32(btn.Tag);
                if (_controller != null)
                {
                    await _controller.CapturarHuellaDedoAsync(numeroDedo);
                }
            }
        }

        public void ActualizarEstadoHuella(int numeroDedo, bool registrada)
        {
            string estado = registrada ? "✅ Registrado" : "No registrado";
            string texto = $"👍 Dedo {numeroDedo} ({estado})";

            switch (numeroDedo)
            {
                case 1:
                    btnHuella1.Content = texto;
                    btnHuella1.IsEnabled = true;
                    break;
                case 2:
                    btnHuella2.Content = texto;
                    btnHuella2.IsEnabled = true;
                    break;
                case 3:
                    btnHuella3.Content = texto;
                    btnHuella3.IsEnabled = true;
                    break;
            }
        }

        public void EstablecerEstadoEsperandoHuella(int numeroDedo)
        {
            string texto = $"⏳ Coloque el dedo {numeroDedo}...";

            btnHuella1.IsEnabled = true;
            btnHuella2.IsEnabled = true;
            btnHuella3.IsEnabled = true;

            switch (numeroDedo)
            {
                case 1:
                    btnHuella1.Content = texto;
                    break;
                case 2:
                    btnHuella2.Content = texto;
                    break;
                case 3:
                    btnHuella3.Content = texto;
                    break;
            }
        }

        private void AdminWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _controller?.CancelarCaptura();
        }

        public void MostrarMensaje(string mensaje, string titulo = "Información")
        {
            MessageBox.Show(mensaje, titulo, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // --- Gestión de la Lista de Empleados y Filtros ---

        public void MostrarListaEmpleados(IEnumerable<EmpleadoViewDto> lista)
        {
            dgEmpleados.ItemsSource = lista;
        }

        private void txtBuscarNombre_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void cmbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            string busqueda = txtBuscarNombre?.Text ?? "";
            string estado = (cmbFiltroEstado?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos";

            _controller?.CargarListaEmpleados(busqueda, estado);
        }
    }
}