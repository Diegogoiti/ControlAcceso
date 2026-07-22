using System;
using System.Windows;
using ControlAcceso.Application;

namespace ControlAcceso
{
    public partial class MainWindow : Window
    {
        private readonly MyApp _app;

        public MainWindow()
        {
            InitializeComponent();

            // Asignación mediante la instancia global Singleton
            _app = App.AppInstance;

            // Refrescar y cargar la vista
            ActualizarTablaEmpleados();
        }

        /// <summary>
        /// Consume directamente la proyección optimizada en la Capa de Aplicación.
        /// </summary>
        private void ActualizarTablaEmpleados()
        {
            try
            {
                // Re-calcula y carga la caché expuesta por MyApp
                _app.CargarEmpleadosViewCache();

                // Binding directo a la colección IReadOnlyList<EmpleadoViewDto>
                dgvEmpleados.ItemsSource = null;
                dgvEmpleados.ItemsSource = _app.EmpleadosViewCache;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al refrescar el monitoreo: {ex.Message}",
                                "Error de Capa de Presentación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Ejecuta el caso de uso de marcado asistido por biometría.
        /// </summary>
        private async void btnMarcarAsistencia_Click(object sender, RoutedEventArgs e)
        {
            btnMarcarAsistencia.IsEnabled = false;

            try
            {
                // Por defecto marcamos entrada (1) o lógica requerida por el caso de uso
                var (exito, mensaje) = await _app.MarcarAsistenciaAsync(tipoAsistencia: 1);

                if (exito)
                {
                    MessageBox.Show(mensaje, "Asistencia Registrada", MessageBoxButton.OK, MessageBoxImage.Information);
                    ActualizarTablaEmpleados();
                }
                else
                {
                    MessageBox.Show(mensaje, "Validación de Acceso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió una falla durante el proceso: {ex.Message}",
                                "Error de Sistema", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnMarcarAsistencia.IsEnabled = true;
            }
        }

        private void dgvEmpleados_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Sin comportamiento hacia otras ventanas
        }

        private void btnAdministrar_Click(object sender, RoutedEventArgs e)
        {
            // Sin comportamiento hacia otras ventanas
        }
    }
}
