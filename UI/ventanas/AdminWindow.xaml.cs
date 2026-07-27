using System;
using System.Windows;
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
    }
}
