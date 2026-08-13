using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ControlAcceso.DTOs;

namespace ControlAcceso
{
    public partial class ReportesWindow : Window
    {
        private readonly UI.controladores.ReportesController _controller;
        private ReporteDetalladoEmpleadoDto? _datosDetalle;
        private List<EmpleadoViewDto> _empleados = new();

        public ReportesWindow(UI.controladores.ReportesController controller)
        {
            InitializeComponent();
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));

            dpFechaSemana.SelectedDate = DateTime.Now;
            dpDesde.SelectedDate = DateTime.Today.AddDays(-7);
            dpHasta.SelectedDate = DateTime.Today;

            CargarEmpleados();
            MostrarTab("Hoy");
        }

        #region --- Navegación entre pestañas ---

        private void btnTabHoy_Click(object sender, RoutedEventArgs e) => MostrarTab("Hoy");
        private void btnTabSemanal_Click(object sender, RoutedEventArgs e) => MostrarTab("Semanal");
        private void btnTabDetallado_Click(object sender, RoutedEventArgs e) => MostrarTab("Detallado");

        private void MostrarTab(string tab)
        {
            PanelHoy.Visibility = tab == "Hoy" ? Visibility.Visible : Visibility.Collapsed;
            PanelSemanal.Visibility = tab == "Semanal" ? Visibility.Visible : Visibility.Collapsed;
            PanelDetallado.Visibility = tab == "Detallado" ? Visibility.Visible : Visibility.Collapsed;

            SetTabActiva(btnTabHoy, tab == "Hoy");
            SetTabActiva(btnTabSemanal, tab == "Semanal");
            SetTabActiva(btnTabDetallado, tab == "Detallado");

            if (tab == "Hoy")
            {
                // Datos frescos al volver a la pestaña
                _controller.RefrescarDashboard();
            }
            else if (tab == "Semanal")
            {
                CargarSemanal();
            }
        }

        private static void SetTabActiva(Button btn, bool activa)
        {
            btn.Background = activa
                ? new SolidColorBrush(Color.FromRgb(0x25, 0x63, 0xEB))
                : new SolidColorBrush(Color.FromRgb(0xE2, 0xE8, 0xF0));
            btn.Foreground = activa
                ? Brushes.White
                : new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55));
            btn.BorderBrush = activa
                ? new SolidColorBrush(Color.FromRgb(0x1D, 0x4E, 0xD8))
                : new SolidColorBrush(Color.FromRgb(0xCB, 0xD5, 0xE1));
        }

        #endregion

        #region --- Pestaña Hoy (dashboard) ---

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

        #endregion

        #region --- Pestaña Semanal ---

        private void CargarSemanal()
        {
            if (!dpFechaSemana.SelectedDate.HasValue)
            {
                lblRangoSemanal.Text = "Seleccione una semana y pulse Consultar.";
                dgSemanal.ItemsSource = null;
                return;
            }

            try
            {
                DateTime fecha = dpFechaSemana.SelectedDate.Value;

                DateTime inicio = fecha;
                while (inicio.DayOfWeek != DayOfWeek.Monday)
                {
                    inicio = inicio.AddDays(-1);
                }
                DateTime fin = inicio.AddDays(5);

                lblRangoSemanal.Text = $"Semana del {inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}";
                dgSemanal.ItemsSource = _controller.ObtenerDatosReporteSemanal(fecha);
            }
            catch (Exception ex)
            {
                MostrarError("No se pudieron cargar los datos de la semana: " + ex.Message);
            }
        }

        private void btnConsultarSemana_Click(object sender, RoutedEventArgs e)
        {
            CargarSemanal();
        }

        private void btnExportarSemana_Click(object sender, RoutedEventArgs e)
        {
            if (!dpFechaSemana.SelectedDate.HasValue)
            {
                MostrarError("Debe seleccionar una fecha para la semana a exportar.");
                return;
            }

            btnExportarSemana.IsEnabled = false;
            btnExportarSemana.Content = "Generando...";

            try
            {
                _controller.GenerarReporte(dpFechaSemana.SelectedDate.Value);
                MostrarMensaje("PDF semanal generado y abierto en Documentos/ControlAcceso_Reportes.");
            }
            catch (Exception ex)
            {
                MostrarError("Error al generar el PDF: " + ex.Message);
            }
            finally
            {
                btnExportarSemana.IsEnabled = true;
                btnExportarSemana.Content = "📄 Exportar semana (PDF)";
            }
        }

        #endregion

        #region --- Pestaña Por empleado ---

        private void CargarEmpleados()
        {
            _empleados = _controller.ObtenerEmpleadosParaReporte();
            AplicarFiltroEmpleados(string.Empty);
        }

        private void AplicarFiltroEmpleados(string texto)
        {
            var filtrados = string.IsNullOrWhiteSpace(texto)
                ? _empleados
                : _empleados.Where(e =>
                    e.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    e.Cedula.Contains(texto)).ToList();

            lstEmpleados.ItemsSource = filtrados;
            if (filtrados.Count > 0)
            {
                lstEmpleados.SelectedIndex = 0;
            }
        }

        private void txtBuscarEmpleado_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltroEmpleados(txtBuscarEmpleado.Text?.Trim() ?? string.Empty);
        }

        private void btnConsultarDetalle_Click(object sender, RoutedEventArgs e)
        {
            if (!(lstEmpleados.SelectedItem is EmpleadoViewDto empleado))
            {
                MostrarError("Seleccione un empleado de la lista (o escriba para filtrar).");
                return;
            }

            int empleadoId = empleado.Id;

            if (!dpDesde.SelectedDate.HasValue || !dpHasta.SelectedDate.HasValue)
            {
                MostrarError("Debe seleccionar las fechas Desde y Hasta.");
                return;
            }

            DateTime desde = dpDesde.SelectedDate.Value;
            DateTime hasta = dpHasta.SelectedDate.Value;

            if (desde > hasta)
            {
                MostrarError("La fecha Desde no puede ser posterior a Hasta.");
                return;
            }

            try
            {
                _datosDetalle = _controller.ConsultarReporteDetallado(empleadoId, desde, hasta);
                MostrarDetalle(_datosDetalle);
            }
            catch (Exception ex)
            {
                MostrarError("No se pudo consultar el reporte: " + ex.Message);
            }
        }

        private void MostrarDetalle(ReporteDetalladoEmpleadoDto datos)
        {
            lblTituloDetalle.Text = $"{datos.Nombre} — Cédula {datos.Cedula} ({datos.Cargo}) · " +
                                    $"{datos.Desde:dd/MM/yyyy} al {datos.Hasta:dd/MM/yyyy}";

            txtDetTrabajados.Text = datos.DiasTrabajados.ToString();
            txtDetFaltas.Text = datos.Faltas.ToString();
            txtDetRetardos.Text = datos.Retardos.ToString();
            txtDetJustificados.Text = datos.RetardosJustificados.ToString();
            txtDetPorcentaje.Text = $"{datos.PorcentajeAsistencia}% asistencia";

            dgDetalle.ItemsSource = datos.Dias;
        }

        private void btnExportarDetalle_Click(object sender, RoutedEventArgs e)
        {
            if (_datosDetalle == null)
            {
                MostrarError("Primero consulte un reporte para poder exportarlo.");
                return;
            }

            btnExportarDetalle.IsEnabled = false;
            btnExportarDetalle.Content = "Generando...";

            try
            {
                _controller.GenerarReporteDetalladoPdf(_datosDetalle);
                MostrarMensaje("PDF detallado generado y abierto en Documentos/ControlAcceso_Reportes.");
            }
            catch (Exception ex)
            {
                MostrarError("Error al generar el PDF: " + ex.Message);
            }
            finally
            {
                btnExportarDetalle.IsEnabled = true;
                btnExportarDetalle.Content = "📄 Exportar PDF";
            }
        }

        #endregion

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private static void MostrarMensaje(string mensaje)
        {
            MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static void MostrarError(string mensaje)
        {
            MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
