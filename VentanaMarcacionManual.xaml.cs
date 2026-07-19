using System;
using System.Linq;
using System.Windows;

namespace ControlAcceso
{
    public partial class VentanaMarcacionManual : Window
    {
        private readonly MyApp _app;
        private readonly Empleado _empleado;

        public VentanaMarcacionManual(MyApp app, Empleado empleado)
        {
            InitializeComponent();
            _app = app;
            _empleado = empleado;

            // Mostrar la información del usuario en la interfaz
            lblEmpleado.Text = $"Empleado: {_empleado.Nombre}";
            lblCedula.Text = $"Cédula: {_empleado.Cedula}";

            // Preseleccionar automáticamente el tipo de marca basándonos en la última registrada hoy
            EvaluarSiguienteMarca();
        }

        private void EvaluarSiguienteMarca()
        {
            DateTime hoy = DateTime.Today;
            var ultimaAsistenciaHoy = _app.HistorialAsistencias
                .Where(a => a.EmpleadoID == _empleado.id && a.Timestamp.Date == hoy)
                .OrderByDescending(a => a.Timestamp)
                .FirstOrDefault();

            if (ultimaAsistenciaHoy != null && ultimaAsistenciaHoy.Tipo == 1)
            {
                rbSalida.IsChecked = true; // Si la última fue Entrada (1), sugerimos Salida (0)
            }
            else
            {
                rbEntrada.IsChecked = true; // De lo contrario sugerimos Entrada (1)
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Determinar tipo según el RadioButton seleccionado
            int tipoAsistencia = rbEntrada.IsChecked == true ? 1 : 0;

            try
            {
                // Registrar marca en Base de Datos e Historial en memoria
                var asistencia = new Asistencia(_empleado.id, DateTime.Now, tipoAsistencia);
                _app.Db.RegistrarAsistencia(asistencia);
                _app.CargarHistorialAsistencias();

                string accion = tipoAsistencia == 1 ? "Entrada" : "Salida";
                MessageBox.Show($"{accion} guardada manualmente de forma correcta.", "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el registro: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
