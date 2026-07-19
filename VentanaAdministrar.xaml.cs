using System;
using System.Windows;
using System.Windows.Media;

namespace ControlAcceso
{
    public partial class VentanaAdministrar : Window
    {
        private readonly MyApp _app;
        private System.Threading.CancellationTokenSource? _cts;

        public VentanaAdministrar(MyApp app)
        {
            InitializeComponent();
            _app = app;


            InicializarSelectoresTiempo();
            CargarDatosReporte();
        }

        private void InicializarSelectoresTiempo()
        {
            // 1. Poblamos los selectores de Horas (00 a 23)
            for (int h = 0; h < 24; h++)
            {
                string itemHora = h.ToString("D2");
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

            var (passDecodificada, horaEntrada, horaSalida) = _app.Db.ObtenerConfiguracion();
            AsignarValoresTiempo(horaEntrada, horaSalida);
        }

        /// <summary>
        /// Método encargado de mapear objetos TimeSpan de forma robusta hacia los ComboBox visuales.
        /// </summary>
        private void AsignarValoresTiempo(TimeSpan entrada, TimeSpan salida)
        {
            // El formato "hh" y "mm" extrae la hora y minuto con dos dígitos ("08", "05", etc.)
            cmbHoraEntrada.SelectedItem = entrada.ToString("hh");
            cmbMinutoEntrada.SelectedItem = entrada.ToString("mm");

            cmbHoraSalida.SelectedItem = salida.ToString("hh");
            cmbMinutoSalida.SelectedItem = salida.ToString("mm");
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
            if (cmbHoraEntrada.SelectedItem == null || cmbMinutoEntrada.SelectedItem == null ||
                cmbHoraSalida.SelectedItem == null || cmbMinutoSalida.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione horas y minutos válidos.", "Campos Incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int hEntrada = Convert.ToInt32(cmbHoraEntrada.SelectedItem);
            int mEntrada = Convert.ToInt32(cmbMinutoEntrada.SelectedItem);

            int hSalida = Convert.ToInt32(cmbHoraSalida.SelectedItem);
            int mSalida = Convert.ToInt32(cmbMinutoSalida.SelectedItem);

            TimeSpan tiempoEntrada = new TimeSpan(hEntrada, mEntrada, 0);
            TimeSpan tiempoSalida = new TimeSpan(hSalida, mSalida, 0);

            string password = txtPassword.Password;

            if (tiempoSalida <= tiempoEntrada)
            {
                MessageBox.Show("La hora de salida no puede ser menor o igual a la hora de entrada.", "Error de Consistencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _app.Db.GuardarConfiguracion(tiempoEntrada, tiempoSalida, password);

            MessageBox.Show($"Configuración procesada con éxito.\nEntrada: {tiempoEntrada:hh\\:mm}\nSalida: {tiempoSalida:hh\\:mm}",
                            "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnGuardarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txtNombreEmpleado.Text.Trim();
            string cedulaTexto = txtCedulaEmpleado.Text.Trim();

            // 1. Validaciones de interfaz
            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(cedulaTexto))
            {
                MessageBox.Show("Por favor, rellene todos los campos del empleado.", "Campos Vacíos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(cedulaTexto, out int cedula))
            {
                MessageBox.Show("La identificación/cédula debe ser un número válido.", "Error de Formato", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Preparar el token de cancelación y feedback visual en el botón
            _cts?.Cancel();
            _cts = new System.Threading.CancellationTokenSource();

            btnGuardarEmpleado.IsEnabled = false;
            btnGuardarEmpleado.Content = "Coloque el dedo en el lector...";

            try
            {
                // 3. Usar tu HardwareService asíncrono pasándole el token
                byte[]? rawData = await Services.HardwareService.CapturarHuellaAsync(_cts.Token);

                if (rawData == null)
                {
                    MessageBox.Show("Operación de captura cancelada o fallida.", "Registro Interrumpido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 4. Cambiar estado visual indicando el procesamiento
                btnGuardarEmpleado.Content = "Procesando huella...";

                // 5. Usar tu FingerprintService para generar el template
                Services.FingerprintService fpService = new Services.FingerprintService();
                SourceAFIS.FingerprintTemplate template = fpService.CrearTemplate(rawData);

                // 6. Instanciar y persistir en base de datos
                Empleado nuevoEmpleado = new Empleado(0, nombre, cedula, template);
                _app.Db.AgregarEmpleado(nuevoEmpleado);
                _app.CargarEmpleadosDesdeDb();

                MessageBox.Show($"Empleado {nombre} registrado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                _app.CargarEmpleadosDesdeDb();

                // Limpiar campos
                txtNombreEmpleado.Clear();
                txtCedulaEmpleado.Clear();
                CargarDatosReporte();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar empleado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Restablecer el botón pase lo que pase
                btnGuardarEmpleado.IsEnabled = true;
                btnGuardarEmpleado.Content = "Escanear Huella y Guardar";
            }
        }

        private void BtnNavReporte_Click(object sender, RoutedEventArgs e)
        {
            panelReporte.Visibility = Visibility.Visible;
            panelConfiguracion.Visibility = Visibility.Collapsed;
            panelRegistrar.Visibility = Visibility.Collapsed;

            btnNavReporte.FontWeight = FontWeights.Bold;
            btnNavConfig.FontWeight = FontWeights.Normal;
            btnNavRegistrar.FontWeight = FontWeights.Normal;
            CargarDatosReporte();
        }

        private void BtnNavRegistrar_Click(object sender, RoutedEventArgs e)
        {
            panelReporte.Visibility = Visibility.Collapsed;
            panelConfiguracion.Visibility = Visibility.Collapsed;
            panelRegistrar.Visibility = Visibility.Visible;

            btnNavReporte.FontWeight = FontWeights.Normal;
            btnNavConfig.FontWeight = FontWeights.Normal;
            btnNavRegistrar.FontWeight = FontWeights.Bold;
        }

        private void BtnNavConfig_Click(object sender, RoutedEventArgs e)
        {
            panelReporte.Visibility = Visibility.Collapsed;
            panelConfiguracion.Visibility = Visibility.Visible;
            panelRegistrar.Visibility = Visibility.Collapsed;

            btnNavReporte.FontWeight = FontWeights.Normal;
            btnNavConfig.FontWeight = FontWeights.Bold;
            btnNavRegistrar.FontWeight = FontWeights.Normal;
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
