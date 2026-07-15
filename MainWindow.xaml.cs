using System.Windows;
using System.Windows.Input;

namespace ControlAcceso
{
    public partial class MainWindow : Window
    {
        private MyApp _app = new MyApp();
        public MainWindow()
        {

              InitializeComponent();
            //CargarDatosPrueba();
            _app.CargarEmpleadosDesdeDb(); // Trae los datos
            dgvEmpleados.ItemsSource = _app.Empleados; // Los muestra

        }

        private void dgvEmpleados_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvEmpleados.SelectedItem is Empleado emp)
            {
                // 1. Instanciamos la ventana y le pasamos el ID
                VentanaHuella modal = new VentanaHuella();

                // 2. Establecemos a MainWindow como dueño para que el modal se centre sobre ella
                modal.Owner = this;

                // 3. Abrimos como modal (bloquea la UI principal hasta que se cierre)
                modal.ShowDialog();
            }
        }
        private void CargarDatosPrueba()
        {
            // Creamos una lista de prueba
            var lista = new List<Empleado>
            {
                new Empleado(1, "Diego", 12345678),
                new Empleado(2, "Luis", 87654321),
                new Empleado(3, "Jostin", 11223344)
            };

            lista = lista.SelectMany(x => Enumerable.Repeat(x, 15)).ToList();

            // Asignamos la lista al DataGrid
            dgvEmpleados.ItemsSource = lista;
        }
    }
}
