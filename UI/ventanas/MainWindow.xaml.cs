using System;
using System.Linq;
using System.Windows;
using ControlAcceso.Application;
using ControlAcceso.DTOs;

namespace ControlAcceso
{
    public partial class MainWindow : Window
    {
        private readonly MyApp _app;

        public MainWindow()
        {
            InitializeComponent();

            // Usamos la propiedad corregida de la instancia Singleton en App.xaml.cs
            _app = App.AppInstance;

            // Cargar y mostrar los datos en la tabla directamente
            ActualizarTablaEmpleados();
        }

        /// <summary>
        /// Consulta la base de datos a través de los servicios y mapea los datos a la tabla.
        /// </summary>
        private void ActualizarTablaEmpleados()
        {
            try
            {
                // 1. Consultar empleados activos directamente desde DatabaseService
                var empleadosActivos = _app.DatabaseService.ObtenerEmpleados(new EmpleadoFilter
                {
                    SoloActivos = true
                });

                // 2. Consultar asistencias a través de la Capa de Aplicación (MyApp)
                var asistenciasHoy = _app.ObtenerAsistenciasDelDia();

                // 3. Optimizar búsqueda: Agrupar por empleado para evitar escaneos O(N) dentro del Select
                var ultimasMarcasPorEmpleado = asistenciasHoy
                    .GroupBy(a => a.EmpleadoID)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderByDescending(a => a.Timestamp).FirstOrDefault()
                    );

                // 4. Mapear al tipo anónimo para el DataGrid
                dgvEmpleados.ItemsSource = empleadosActivos.Select(emp =>
                {
                    ultimasMarcasPorEmpleado.TryGetValue(emp.Id, out var ultimaMarcaHoy);

                    string estadoCalculado = ultimaMarcaHoy switch
                    {
                        null => "Ausente",
                        { Tipo: 1 } => "Presente",
                        _ => "Retirado"
                    };

                    return new
                    {
                        Datos = emp,
                        Estado = estadoCalculado
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos en la tabla: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Manejadores de eventos deshabilitados o simplificados para no invocar otras UIs
        private void dgvEmpleados_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Sin comportamiento hacia otras ventanas[cite: 13]
        }

        private void btnAdministrar_Click(object sender, RoutedEventArgs e)
        {
            // Sin comportamiento hacia otras ventanas[cite: 13]
        }
    }
}
