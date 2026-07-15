using System.Windows;
using System.Windows.Input;

namespace ControlAcceso
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CargarDatosPrueba();
        }

        private void dgvEmpleados_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvEmpleados.SelectedItem is Empleado emp)
            {
                // 1. Instanciamos la ventana y le pasamos el ID
                VentanaHuella modal = new VentanaHuella(emp.ID);

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
                new Empleado { ID = 1, Nombre = "Diego", CI = "V-12345678" },
                new Empleado { ID = 2, Nombre = "Luis", CI = "V-87654321" },
                new Empleado { ID = 3, Nombre = "Jostin", CI = "V-11223344" }
            };

            lista = lista.SelectMany(x => Enumerable.Repeat(x, 15)).ToList();

            // Asignamos la lista al DataGrid
            dgvEmpleados.ItemsSource = lista;
        }
    }
}
