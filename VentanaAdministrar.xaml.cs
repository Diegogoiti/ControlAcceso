using System;
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
            _app = app;

            // Cargar las opciones del reloj y los reportes base
            InicializarSelectoresTiempo();
            CargarDatosReporte();
        }

        private void InicializarSelectoresTiempo()
        {
            // 1. Poblamos los selectores de Horas (00 a 23)
            for (int h = 0; h < 24; h++)
            {
                string itemHora = h.ToString("D2"); // Formato "00", "01", "02"...
                cmbHoraEntrada.Items.Add(itemHora);
                cmbHoraSalida.Items.Add(itemHora);
            }

            // 2. Poblamos los selectores de Minutos (00 a 59)
            for (int m = 0; m < 60; m++)
            {
                string itemMinuto = m.ToString("D2");
                cmbMinutoEntrada.Items.Add(itemMinuto);
                cmbMinutoSalida.Items.Add(itemMinuto);
            }

            // 3. Predefinir valores por defecto para que no inicien vacíos (Ej: 08:00 y 17:00)
            cmbHoraEntrada.SelectedItem = "08";
            cmbMinutoEntrada.SelectedItem = "00";
            cmbHoraSalida.SelectedItem = "17";
            cmbMinutoSalida.SelectedItem = "00";
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

        private void BtnGuardarConfig_Click(object sender, RoutedEventArgs e)
        {
            // Verificación preventiva de seguridad
            if (cmbHoraEntrada.SelectedItem == null || cmbMinutoEntrada.SelectedItem == null ||
                cmbHoraSalida.SelectedItem == null || cmbMinutoSalida.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione horas y minutos válidos.", "Campos Incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1. Extracción e interpretación limpia a enteros
            int hEntrada = Convert.ToInt32(cmbHoraEntrada.SelectedItem);
            int mEntrada = Convert.ToInt32(cmbMinutoEntrada.SelectedItem);

            int hSalida = Convert.ToInt32(cmbHoraSalida.SelectedItem);
            int mSalida = Convert.ToInt32(cmbMinutoSalida.SelectedItem);

            // 2. CONVERSIÓN A DATOS NORMALIZADOS (TimeSpan)
            // Esto garantiza un manejo correcto y compatibilidad directa con campos TIME de SQL
            TimeSpan tiempoEntrada = new TimeSpan(hEntrada, mEntrada, 0);
            TimeSpan tiempoSalida = new TimeSpan(hSalida, mSalida, 0);

            string password = txtPassword.Password;

            // 3. Validar consistencia lógica de los tiempos (Opcional pero recomendado)
            if (tiempoSalida <= tiempoEntrada)
            {
                MessageBox.Show("La hora de salida no puede ser menor o igual a la hora de entrada.", "Error de Consistencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Aquí tus variables nativas listas para enviar a tu clase '_app' o capa de datos:
            // _app.GuardarConfiguracionHorarios(tiempoEntrada, tiempoSalida, password);

            MessageBox.Show($"Configuración procesada con éxito.\nEntrada: {tiempoEntrada:hh\\:mm}\nSalida: {tiempoSalida:hh\\:mm}",
                            "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnNavReporte_Click(object sender, RoutedEventArgs e)
        {
            panelReporte.Visibility = Visibility.Visible;
            panelConfiguracion.Visibility = Visibility.Collapsed;
            btnNavReporte.FontWeight = FontWeights.Bold;
            btnNavConfig.FontWeight = FontWeights.Normal;
            CargarDatosReporte();
        }

        private void BtnNavConfig_Click(object sender, RoutedEventArgs e)
        {
            panelReporte.Visibility = Visibility.Collapsed;
            panelConfiguracion.Visibility = Visibility.Visible;
            btnNavReporte.FontWeight = FontWeights.Normal;
            btnNavConfig.FontWeight = FontWeights.Bold;
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
