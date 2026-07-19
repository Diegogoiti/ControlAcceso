using System;
using System.Linq;
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
            for (int h = 0; h < 24; h++)
            {
                string itemHora = h.ToString("D2");
                cmbHoraEntrada.Items.Add(itemHora);
                cmbHoraSalida.Items.Add(itemHora);
            }

            for (int m = 0; m < 60; m++)
            {
                string itemMinuto = m.ToString("D2");
                cmbMinutoEntrada.Items.Add(itemMinuto);
                cmbMinutoSalida.Items.Add(itemMinuto);
            }

            var (passDecodificada, horaEntrada, horaSalida) = _app.Db.ObtenerConfiguracion();
            AsignarValoresTiempo(horaEntrada, horaSalida);
        }

        private void AsignarValoresTiempo(TimeSpan entrada, TimeSpan salida)
        {
            cmbHoraEntrada.SelectedItem = entrada.ToString("hh");
            cmbMinutoEntrada.SelectedItem = entrada.ToString("mm");

            cmbHoraSalida.SelectedItem = salida.ToString("hh");
            cmbMinutoSalida.SelectedItem = salida.ToString("mm");
        }

        private void CargarDatosReporte()
        {
            if (_app.Empleados == null || _app.HistorialAsistencias == null) return;

            // 1. Obtener los horarios límites configurados en el sistema
            var (_, horaEntradaConfig, horaSalidaConfig) = _app.Db.ObtenerConfiguracion();

            // 2. Obtener las marcas del día actual
            DateTime hoy = DateTime.Today;
            var marcasHoy = _app.HistorialAsistencias
                .Where(a => a.Timestamp.Date == hoy)
                .ToList();

            // Función auxiliar interna para formatear los TimeSpan de manera clara y compacta
            string FormatearTiempo(TimeSpan tiempo)
            {
                if (tiempo.TotalMinutes <= 0) return "0m";

                int horas = (int)tiempo.TotalHours;
                int minutos = tiempo.Minutes;

                if (horas > 0)
                {
                    return $"{horas}h {minutos}m";
                }
                return $"{minutos}m";
            }

            // 3. Generar el reporte con las columnas formateadas e incluyendo el ID
            dgvReporteDiario.ItemsSource = _app.Empleados.Select(emp =>
            {
                // Buscar primera entrada (Tipo = 1) y primera salida (Tipo = 0)
                var entrada = marcasHoy
                    .Where(a => a.EmpleadoID == emp.id && a.Tipo == 1)
                    .OrderBy(a => a.Timestamp)
                    .FirstOrDefault();

                var salida = marcasHoy
                    .Where(a => a.EmpleadoID == emp.id && a.Tipo == 0)
                    .OrderBy(a => a.Timestamp)
                    .FirstOrDefault();

                string retrasoEntradaStr = "0m";
                string tiempoExtraStr = "0m";
                string tiempoTrabajadoStr = "Incompleto";

                // --- CÁLCULO DE RETRASO EN ENTRADA ---
                if (entrada != null)
                {
                    TimeSpan tiempoEntradaReal = entrada.Timestamp.TimeOfDay;
                    if (tiempoEntradaReal > horaEntradaConfig)
                    {
                        retrasoEntradaStr = FormatearTiempo(tiempoEntradaReal - horaEntradaConfig);
                    }
                }
                else
                {
                    retrasoEntradaStr = "No calculado";
                }

                // --- CÁLCULO DE TIEMPO EXTRA EN SALIDA ---
                if (salida != null)
                {
                    TimeSpan tiempoSalidaReal = salida.Timestamp.TimeOfDay;
                    if (tiempoSalidaReal > horaSalidaConfig)
                    {
                        tiempoExtraStr = FormatearTiempo(tiempoSalidaReal - horaSalidaConfig);
                    }
                }
                else
                {
                    tiempoExtraStr = "No calculado";
                }

                // --- CÁLCULO DE TIEMPO TOTAL TRABAJADO ---
                if (entrada != null && salida != null)
                {
                    TimeSpan diferenciaTrabajada = salida.Timestamp - entrada.Timestamp;
                    tiempoTrabajadoStr = FormatearTiempo(diferenciaTrabajada);
                }

                return new
                {
                    Id = emp.id,
                    Nombre = emp.Nombre,
                    Cedula = emp.Cedula,
                    HoraEntrada = entrada != null ? entrada.Timestamp.ToString("hh:mm tt") : "No calculado",
                    HoraSalida = salida != null ? salida.Timestamp.ToString("hh:mm tt") : "No calculado",
                    Retraso = retrasoEntradaStr,
                    TiempoExtra = tiempoExtraStr,
                    TiempoTrabajado = tiempoTrabajadoStr
                };
            }).ToList();
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

            _cts?.Cancel();
            _cts = new System.Threading.CancellationTokenSource();

            btnGuardarEmpleado.IsEnabled = false;
            btnGuardarEmpleado.Content = "Coloque el dedo en el lector...";

            try
            {
                byte[]? rawData = await Services.HardwareService.CapturarHuellaAsync(_cts.Token);

                if (rawData == null)
                {
                    MessageBox.Show("Operación de captura cancelada o fallida.", "Registro Interrumpido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                btnGuardarEmpleado.Content = "Procesando huella...";

                Services.FingerprintService fpService = new Services.FingerprintService();
                SourceAFIS.FingerprintTemplate template = fpService.CrearTemplate(rawData);

                Empleado nuevoEmpleado = new Empleado(0, nombre, cedula, template);
                _app.Db.AgregarEmpleado(nuevoEmpleado);
                _app.CargarEmpleadosDesdeDb();

                MessageBox.Show($"Empleado {nombre} registrado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                _app.CargarEmpleadosDesdeDb();

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
