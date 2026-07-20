using System.Windows;
using System.Threading;
using ControlAcceso.Services;

namespace ControlAcceso
{
    public partial class VentanaHuella : Window
    {
        // 1. Declarar el campo para el servicio
        private readonly FingerprintService _fingerprintService = new FingerprintService();
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly MyApp _app;
        private readonly int _empleadoId;

        public VentanaHuella(MyApp app, int empleadoId)
        {
            InitializeComponent();
            _app = app;
            _empleadoId = empleadoId;
        }

        private async void BtnCapturar_Click(object sender, RoutedEventArgs e)
        {
            _cts = new CancellationTokenSource();

            lblMensaje.Text = "Coloque su dedo en el lector...";

            byte[]? rawData = await HardwareService.CapturarHuellaAsync(_cts.Token);

            if (rawData != null)
            {
                var template = _fingerprintService.CrearTemplate(rawData);

                // Buscar coincidencia de huella en la lista de empleados
                Empleado? empleadoEncontrado = null;
                foreach (var empleado in _app.Empleados)
                {
                    if (_fingerprintService.Comparar(empleado.Huella, template))
                    {
                        empleadoEncontrado = empleado;
                        break;
                    }
                }

                if (empleadoEncontrado != null)
                {
                    // 1. Buscar la última marcación del empleado del día de hoy en memoria
                    DateTime hoy = DateTime.Today;
                    var ultimaAsistenciaHoy = _app.HistorialAsistencias
                        .Where(a => a.EmpleadoID == empleadoEncontrado.id && a.Timestamp.Date == hoy)
                        .OrderByDescending(a => a.Timestamp) // Ordenamos para tener la más reciente arriba
                        .FirstOrDefault();

                    // 2. Determinar el tipo basándonos en la última registrada
                    int tipoAsistencia = 1; // Por defecto Entrada (1) si es la primera del día
                    string accionTexto = "Entrada";

                    if (ultimaAsistenciaHoy != null)
                    {
                        // Si la última fue Entrada (1), la siguiente es Salida (0)
                        if (ultimaAsistenciaHoy.Tipo == 1)
                        {
                            tipoAsistencia = 0;
                            accionTexto = "Salida";
                        }
                        // Si la última fue Salida (0), la siguiente vuelve a ser Entrada (1)
                        else
                        {
                            tipoAsistencia = 1;
                            accionTexto = "Entrada";
                        }
                    }

                    // 3. Registrar en la base de datos con el tipo correcto
                    var asistencia = new Asistencia(empleadoEncontrado.id, DateTime.Now, tipoAsistencia);
                    _app.Db.RegistrarAsistencia(asistencia);

                    // 4. Forzar la recarga del historial en la aplicación para que la siguiente lectura esté actualizada
                    _app.CargarHistorialAsistencias();

                    lblMensaje.Text = $"{accionTexto} registrada con éxito.\nEmpleado: {empleadoEncontrado.Nombre}.";
                }
                else
                {
                    lblMensaje.Text = "Huella no reconocida. Intente de nuevo.";
                }
            }
        }
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            _cts.Cancel();
            this.Close();
        }
    }
}
