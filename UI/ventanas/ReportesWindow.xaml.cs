using System;
using System.Windows;

namespace ControlAcceso
{
    public partial class ReportesWindow : Window
    {
        private readonly UI.controladores.ReportesController _controller;

        public ReportesWindow(UI.controladores.ReportesController controller)
        {
            InitializeComponent();
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            dpFechaReporte.SelectedDate = DateTime.Now; // Default hoy
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnGenerarPDF_Click(object sender, RoutedEventArgs e)
        {
            txtMensaje.Visibility = Visibility.Collapsed;

            if (!dpFechaReporte.SelectedDate.HasValue)
            {
                MostrarMensajeError("Debe seleccionar una fecha válida.");
                return;
            }

            btnGenerarPDF.IsEnabled = false;
            btnGenerarPDF.Content = "Generando...";

            try
            {
                _controller.GenerarReporte(dpFechaReporte.SelectedDate.Value);
                // Si llegamos aquí sin excepciones, asumimos éxito (el archivo se abrió)
                Close();
            }
            catch (Exception ex)
            {
                MostrarMensajeError("Error al generar: " + ex.Message);
            }
            finally
            {
                btnGenerarPDF.IsEnabled = true;
                btnGenerarPDF.Content = "📄 Generar PDF";
            }
        }

        public void MostrarMensajeError(string mensaje)
        {
            txtMensaje.Text = mensaje;
            txtMensaje.Visibility = Visibility.Visible;
        }
    }
}