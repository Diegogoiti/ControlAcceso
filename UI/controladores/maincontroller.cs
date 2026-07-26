using System;
using System.Threading.Tasks;
using ControlAcceso.Application;

namespace ControlAcceso.UI.controladores
{
    public class MainController
    {
        private readonly MyApp _app;
        private MainWindow? _vista;

        public MainController(MyApp app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
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

            _vista.SetEstadoCargando(true);
            _app.CargarEmpleadosViewCache();
            _vista.MostrarEmpleados(_app.EmpleadosViewCache);
            _vista.SetEstadoCargando(false);
        }

        public async Task ProcesarMarcajeAsistenciaAsync(int tipoAsistencia)
        {
            if (_vista == null) return;

            try
            {
                _vista.SetEstadoCargando(true);
                var (exito, mensaje) = await _app.MarcarAsistenciaAsync(tipoAsistencia);
                _vista.MostrarMensaje(mensaje, exito);

                if (exito)
                {
                    RefrescarListaEmpleados();
                }
            }
            catch (Exception ex)
            {
                _vista.MostrarMensaje($"Error inesperado: {ex.Message}", false);
            }
            finally
            {
                _vista.SetEstadoCargando(false);
            }
        }

        public void AbrirRegistroEmpleado()
        {
            if (_vista == null) return;
            _vista.MostrarMensaje("Módulo de registro en desarrollo.", true);
        }
    }
}
