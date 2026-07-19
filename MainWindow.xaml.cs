using System;
using System.Linq;
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

            // Asignamos los datos usando la nueva lógica de estados
            ActualizarOrigenDatosTabla();

            btnMarcarAsistencia.Click += btnMarcarAsistencia_click;
        }

        private void dgvEmpleados_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 1. Verificar si hay un elemento seleccionado
            if (dgvEmpleados.SelectedItem != null)
            {
                // 2. Asignar a una variable dynamic para leer las propiedades del tipo anónimo
                dynamic filaSeleccionada = dgvEmpleados.SelectedItem;
                Empleado emp = filaSeleccionada.Datos; // Extrae el objeto Empleado real

                // 3. Solicitar contraseña del administrador
                var (passwordCorrecta, _, _) = _app.Db.ObtenerConfiguracion();

                VentanaContrasena loginModal = new VentanaContrasena(passwordCorrecta)
                {
                    Owner = this
                };

                // 4. Si la autenticación es correcta, abrir la ventana de marcación manual
                if (loginModal.ShowDialog() == true)
                {
                    VentanaMarcacionManual marcacionModal = new VentanaMarcacionManual(_app, emp)
                    {
                        Owner = this
                    };

                    if (marcacionModal.ShowDialog() == true)
                    {
                        // Actualizar la grilla de la ventana principal si se guardó el registro
                        ActualizarTablaEmpleados();
                    }
                }
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
            var (passwordCorrecta, _, _) = _app.Db.ObtenerConfiguracion();
            Console.WriteLine($"Contraseña correcta: {passwordCorrecta}");

            VentanaContrasena loginModal = new VentanaContrasena(passwordCorrecta)
            {
                Owner = this
            };

            if (loginModal.ShowDialog() == true)
            {
                VentanaAdministrar modal = new VentanaAdministrar(_app)
                {
                    Owner = this
                };

                modal.ShowDialog();
                ActualizarTablaEmpleados();
            }
        }

        private void ActualizarTablaEmpleados()
        {
            _app.CargarEmpleadosDesdeDb();
            _app.CargarHistorialAsistencias();

            ActualizarOrigenDatosTabla();
        }

        /// <summary>
        /// Método auxiliar para evitar duplicar la lógica de proyección de LINQ
        /// </summary>
        private void ActualizarOrigenDatosTabla()
        {
            DateTime hoy = DateTime.Today;

            dgvEmpleados.ItemsSource = _app.Empleados.Select(emp =>
            {
                // 1. Buscar la última marcación del empleado del día actual
                var ultimaMarcaHoy = _app.HistorialAsistencias
                    .Where(a => a.EmpleadoID == emp.id && a.Timestamp.Date == hoy)
                    .OrderByDescending(a => a.Timestamp)
                    .FirstOrDefault();

                // 2. Determinar el estado textual según el tipo de la última marca
                string estadoCalculado = "Ausente";
                if (ultimaMarcaHoy != null)
                {
                    estadoCalculado = ultimaMarcaHoy.Tipo == 1 ? "Presente" : "Retirado";
                }

                return new
                {
                    Datos = emp,
                    Estado = estadoCalculado
                };
            }).ToList();
        }
    }
}
