using System.Windows;
using System.Windows.Input;

namespace ControlAcceso
{
    public partial class MainWindow : Window
    {
        private readonly MyApp _app = new MyApp();

        public MainWindow()
        {
            InitializeComponent();

            _app.CargarEmpleadosDesdeDb();
            _app.CargarHistorialAsistencias();
            dgvEmpleados.ItemsSource = _app.Empleados.Select(emp => new
            {
                // Guardamos el empleado completo
                Datos = emp,
                // Calculamos el estado aquí mismo y lo pasamos como un string aparte
                Estado = _app.HistorialAsistencias.Any(a => a.EmpleadoID == emp.id && a.Tipo == 1) ? "Presente" : "Ausente"
            }).ToList();
            btnMarcarAsistencia.Click += btnMarcarAsistencia_click;

            // TEST: Abre la ventana de huella automáticamente al iniciar
            //this.Loaded += MainWindow_Loaded;
        }

        /*private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Usamos el ID del primer empleado cargado, o 1 por defecto para evitar errores
            int testId = _app.Empleados.Count > 0 ? _app.Empleados[0].id : 1;

            VentanaHuella modal = new VentanaHuella(_app, testId)
            {
                Owner = this
            };
            modal.ShowDialog();
        }*/

        private void dgvEmpleados_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvEmpleados.SelectedItem is Empleado emp)
            {
                VentanaHuella modal = new VentanaHuella(_app, emp.id)
                {
                    Owner = this
                };
                modal.ShowDialog();
            }
        }

        private void btnMarcarAsistencia_click(object sender, RoutedEventArgs e)
        {
            VentanaHuella modal = new VentanaHuella(_app, 0)
            {
                Owner = this
            };
            modal.ShowDialog();
            ActualizarTablaEmpleados();
        }

        private void btnAdministrar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Recuperar la contraseña correcta directamente desde la base de datos a través de '_app'
            var (passwordCorrecta, _, _) = _app.Db.ObtenerConfiguracion();
            Console.WriteLine($"Contraseña correcta: {passwordCorrecta}");

            // 2. Instanciar la ventana de validación pasándole la contraseña
            VentanaContrasena loginModal = new VentanaContrasena(passwordCorrecta)
            {
                Owner = this // Centra la ventana sobre la principal y bloquea el fondo
            };

            // 3. Abrir la ventana como diálogo modal y evaluar la respuesta
            if (loginModal.ShowDialog() == true)
            {
                // Si la contraseña fue correcta (DialogResult = true), permitimos el paso:
                VentanaAdministrar modal = new VentanaAdministrar(_app)
                {
                    Owner = this
                };

                modal.ShowDialog();
            }
            // Si cancela o falla, el flujo termina aquí sin levantar el panel de administración
        }

        private void ActualizarTablaEmpleados()
        {
            _app.CargarEmpleadosDesdeDb();
            _app.CargarHistorialAsistencias(); // Asegúrate de recargar el historial también

            dgvEmpleados.ItemsSource = _app.Empleados.Select(emp => new
            {
                Datos = emp,
                Estado = _app.HistorialAsistencias.Any(a => a.EmpleadoID == emp.id && a.Tipo == 1) ? "Presente" : "Ausente"
            }).ToList();
        }
    }
}
