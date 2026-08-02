using System;
using System.Threading;
using System.Threading.Tasks;
using ControlAcceso.Application;

namespace ControlAcceso.UI.controladores
{
    public class MainController
    {
        private readonly MyApp _app;
        private MainWindow? _vista;
        //private readonly AdminController _adminController;
        private CancellationTokenSource? _ctsCaptura;

        public MainController(MyApp app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            //_adminController = new AdminController(_app);
        }

        public void IniciarAplicacion()
        {
            _vista = new MainWindow(this);
            _vista.Show();
            RefrescarListaEmpleados();
        }

        public void RefrescarListaEmpleados()
        {
            if (_vista == null) return;
            _app.CargarEmpleadosViewCache();
        }

        public async Task ProcesarMarcajeAsistenciaAsync(int tipoAsistencia)
        {
            if (_vista == null) return;

            // Prevenir doble ejecución si ya hay una captura en curso
            if (_ctsCaptura != null && !_ctsCaptura.IsCancellationRequested)
                return;

            _ctsCaptura = new CancellationTokenSource();
            _vista.ModoEsperaHuella(true);

            try
            {
                // Desempaquetamos los valores retornados por el método en MyApp
                var (exito, denegado, mensaje, nombreEmpleado, hora) = await _app.MarcarAsistenciaAsync(tipoAsistencia, _ctsCaptura.Token);

                _vista.ModoEsperaHuella(false);

                // Le pasamos los datos a la Vista para que ella maneje los paneles, textos y el temporizador
                _vista.MostrarResultadoMarcaje(exito, denegado, mensaje, nombreEmpleado, hora);

                if (exito)
                {
                    RefrescarListaEmpleados();
                }
            }
            catch (OperationCanceledException)
            {
                _vista.ModoEsperaHuella(false);
                _vista.MostrarMensaje("Operación de lectura cancelada por el usuario.", false);
            }
            catch (Exception ex)
            {
                _vista.ModoEsperaHuella(false);
                _vista.MostrarMensaje($"Error inesperado: {ex.Message}", false);
            }
            finally
            {
                _ctsCaptura?.Dispose();
                _ctsCaptura = null;
            }
        }

        public void CancelarCapturaHuella()
        {
            _ctsCaptura?.Cancel();
        }

        public void AbrirRegistroEmpleado()
        {
            var authController = new AuthController(_app);
            authController.MostrarVentanaAutenticacion();
        }
    }
}
