using System.Windows;

namespace ControlAcceso
{
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
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

        // Eventos temporales vacíos para evitar errores de compilación al hacer clic en buscar
        private void txtBuscarNombre_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) { }
        private void cmbFiltroEstado_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) { }
    }
}
