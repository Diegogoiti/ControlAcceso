using System.Windows;
using System.Windows.Media;

namespace ControlAcceso
{
    public partial class VentanaAdministrar : Window
    {
        private readonly MyApp _app;

        public VentanaAdministrar(MyApp app)
        {
            InitializeComponent();
            _app = app; // Guardamos la referencia compartida[cite: 15]

            CargarDatosReporte();
        }

        private void CargarDatosReporte()
        {
            if (_app.Empleados != null)
            {
                lblTotalEmpleados.Text = $"Total Empleados Registrados: {_app.Empleados.Count}";
            }
            if (_app.HistorialAsistencias != null)
            {
                lblTotalAsistencias.Text = $"Asistencias Registradas: {_app.HistorialAsistencias.Count}";
            }
        }

        // Cambiar a la pestaña de Reportes
        private void BtnNavReporte_Click(object sender, RoutedEventArgs e)
        {
            panelReporte.Visibility = Visibility.Visible;
            panelConfiguracion.Visibility = Visibility.Collapsed;

            // Highlight visual simple para saber qué botón está activo
            btnNavReporte.FontWeight = FontWeights.Bold;
            btnNavConfig.FontWeight = FontWeights.Normal;

            CargarDatosReporte(); // Refresca los datos al volver
        }

        // Cambiar a la pestaña de Configuración
        private void BtnNavConfig_Click(object sender, RoutedEventArgs e)
        {
            panelReporte.Visibility = Visibility.Collapsed;
            panelConfiguracion.Visibility = Visibility.Visible;

            btnNavReporte.FontWeight = FontWeights.Normal;
            btnNavConfig.FontWeight = FontWeights.Bold;
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Cierra la ventana y vuelve a la principal[cite: 15]
        }
    }
}
