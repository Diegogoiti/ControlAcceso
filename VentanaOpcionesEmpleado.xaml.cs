using System;
using System.Windows;
using System.Windows.Media;

namespace ControlAcceso
{
    public partial class VentanaOpcionesEmpleado : Window
    {
        private readonly MyApp _app;
        private readonly Empleado _empleado;
        private System.Threading.CancellationTokenSource? _cts;

        public VentanaOpcionesEmpleado(MyApp app, Empleado empleado)
        {
            InitializeComponent();
            _app = app;
            _empleado = empleado;

            ActualizarInterfazEstado();
        }

        private void ActualizarInterfazEstado()
        {
            lblNombre.Text = $"Nombre: {_empleado.Nombre}";
            lblCedula.Text = $"Cédula: {_empleado.Cedula}";

            if (_empleado.Activo)
            {
                lblEstado.Text = "ACTIVO";
                lblEstado.Foreground = Brushes.Green;
                btnCambiarEstado.Content = "Desactivar Empleado";
                btnCambiarEstado.Background = Brushes.LightCoral;
            }
            else
            {
                lblEstado.Text = "INACTIVO / DADO DE BAJA";
                lblEstado.Foreground = Brushes.Red;
                btnCambiarEstado.Content = "Activar Empleado";
                btnCambiarEstado.Background = Brushes.LightGreen;
            }
        }

        private void BtnCambiarEstado_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Invertimos el estado lógico en la base de datos
                bool nuevoEstado = !_empleado.Activo;
                _app.Db.CambiarEstadoEmpleado(_empleado.id, nuevoEstado);

                // Sincronizamos la memoria de la aplicación inmediatamente
                _empleado.Activo = nuevoEstado;

                MessageBox.Show("El estado del empleado ha sido actualizado con éxito.", "Estado Actualizado", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar estado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnActualizarHuella_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            _cts = new System.Threading.CancellationTokenSource();

            btnActualizarHuella.IsEnabled = false;
            btnCambiarEstado.IsEnabled = false;
            btnActualizarHuella.Content = "Coloque el dedo...";

            try
            {
                byte[]? rawData = await Services.HardwareService.CapturarHuellaAsync(_cts.Token);

                if (rawData == null)
                {
                    MessageBox.Show("Operación de captura cancelada o fallida.", "Interrumpido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                btnActualizarHuella.Content = "Procesando...";

                // Actualizamos los bytes binarios del template en la base de datos
                _app.Db.ActualizarHuellaEmpleado(_empleado.id, rawData);

                // Recargamos la memoria de la aplicación para actualizar el objeto global
                _app.CargarEmpleadosDesdeDb();

                MessageBox.Show("Huella dactilar actualizada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar la huella: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnActualizarHuella.IsEnabled = true;
                btnCambiarEstado.IsEnabled = true;
                btnActualizarHuella.Content = "Actualizar Huella";
            }
        }
    }
}
