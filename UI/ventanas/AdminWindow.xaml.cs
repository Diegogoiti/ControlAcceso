using System;
using System.Windows;
using System.Windows.Controls;
using ControlAcceso.UI.controladores;
using ControlAcceso.DTOs;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace ControlAcceso
{
    public partial class AdminWindow : Window
    {
        private readonly AdminController? _controller;

        // Constructor para diseñador de WPF o fallback
        public AdminWindow()
        {
            InitializeComponent();
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

        private void txtBuscarNombre_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) { }
        private void cmbFiltroEstado_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) { }

        public (string Cedula, string NombreCompleto, DateTime? FechaNacimiento, string Telefono, string TelefonoEmergencia, string Direccion, string? RolTexto) ObtenerDatosFormulario()
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

            return datosCrudos;
        }

        private void btnGuardarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            // Llama al controlador para que tome los datos y los valide
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
        
        // Llamamos al controlador pasándole el número de dedo seleccionado
        if (_controller != null)
        {
            await _controller.CapturarHuellaDedoAsync(numeroDedo);
        }
    }
}

// Método para actualizar visualmente la etiqueta de los botones según el estado
public void ActualizarEstadoHuella(int numeroDedo, bool registrada)
{
    string estado = registrada ? "✅ Registrado" : "No registrado";
    string texto = $"👍 Dedo {numeroDedo} ({estado})";

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
    }
}
