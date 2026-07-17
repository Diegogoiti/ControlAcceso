using System.Windows;

namespace ControlAcceso
{
    public partial class VentanaAdministrar : Window
    {
        private readonly MyApp _app;

        // Recibe la instancia global de la app para comunicarse
        public VentanaAdministrar(MyApp app)
        {
            InitializeComponent();
            _app = app;

            // Mostramos un mensaje de prueba usando los datos compartidos de la app
            MostrarInfoDePrueba();
        }

        private void MostrarInfoDePrueba()
        {
            if (_app.Empleados != null)
            {
                lblEstado.Text = $"Conectado. Empleados registrados: {_app.Empleados.Count}";
            }
            else
            {
                lblEstado.Text = "No se pudieron cargar los datos de la aplicación.";
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
