using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ControlAcceso.DTOs;

namespace ControlAcceso
{
    public partial class ReportesWindow : Window
    {
        private readonly UI.controladores.ReportesController _controller;

        public ReportesWindow(UI.controladores.ReportesController controller)
        {
            InitializeComponent();
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            dpFechaSemana.SelectedDate = DateTime.Now;
        }

        /// <summary>
        /// Pinta el dashboard completo: fecha, tarjetas de estadísticas, tabla de
        /// marcajes (ya filtrada por el controlador) y empleados pendientes.
        /// </summary>
        public void MostrarDashboard(DashboardDiaDto datos)
        {
            lblFechaHoy.Text = "Hoy, " + datos.Fecha.ToString("dddd, d 'de' MMMM 'de' yyyy");

            txtEmpleadosActivos.Text = datos.EmpleadosActivos.ToString();
            txtMarcajesHoy.Text = datos.MarcajesHoy.ToString();
            txtTardanzasHoy.Text = datos.TardanzasHoy.ToString();
            txtPorAdminHoy.Text = datos.PorAdminHoy.ToString();

            txtRegistros.Text = $"{datos.Marcajes.Count} registro(s)";
            dgAsistencias.ItemsSource = datos.Marcajes;

            MostrarSinMarcar(datos.SinMarcar);
        }

        private void MostrarSinMarcar(List<EmpleadoPendienteDto> sinMarcar)
        {
            txtSinMarcarTitulo.Text = $"⏳ Empleados que aún no han marcado ({sinMarcar.Count})";

            if (sinMarcar.Count == 0)
            {
                itemsSinMarcar.Visibility = Visibility.Collapsed;
                txtSinMarcarVacio.Visibility = Visibility.Visible;
            }
            else
            {
                itemsSinMarcar.ItemsSource = sinMarcar;
                itemsSinMarcar.Visibility = Visibility.Visible;
                txtSinMarcarVacio.Visibility = Visibility.Collapsed;
            }
        }

        private void cmbFiltro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string filtro = (cmbFiltro.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos";
            // Puede dispararse durante InitializeComponent() (por SelectedIndex="0"),
            // antes de que _controller esté asignado: se ignora con el operador ?.
            _controller?.CambiarFiltro(filtro);
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            _controller.RefrescarDashboard();
        }

        private void btnExportarSemana_Click(object sender, RoutedEventArgs e)
        {
            txtMensajeExport.Visibility = Visibility.Collapsed;

            if (!dpFechaSemana.SelectedDate.HasValue)
            {
                MostrarErrorExport("Debe seleccionar una fecha para la semana a exportar.");
                return;
            }

            btnExportarSemana.IsEnabled = false;
            btnExportarSemana.Content = "Generando...";

            try
            {
                _controller.GenerarReporte(dpFechaSemana.SelectedDate.Value);
                // Si llegamos aquí sin excepciones, asumimos éxito (el PDF se abrió)
                MostrarErrorExport("PDF generado y abierto en Documentos/ControlAcceso_Reportes.");
            }
            catch (Exception ex)
            {
                MostrarErrorExport("Error al generar: " + ex.Message);
            }
            finally
            {
                btnExportarSemana.IsEnabled = true;
                btnExportarSemana.Content = "📄 Exportar semana (PDF)";
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MostrarErrorExport(string mensaje)
        {
            txtMensajeExport.Text = mensaje;
            txtMensajeExport.Visibility = Visibility.Visible;
        }
    }
}
