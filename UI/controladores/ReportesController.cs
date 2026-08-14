using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ControlAcceso.Application;
using ControlAcceso.DTOs;
using ControlAcceso.Services;

namespace ControlAcceso.UI.controladores
{
    public class ReportesController
    {
        private readonly MyApp _app;
        private ReportesWindow? _ventana;
        private DashboardDiaDto? _dashboard;
        private string _filtroActual = "Todos";

        public ReportesController(MyApp app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        public void MostrarVentanaReportes(System.Windows.Window owner)
        {
            if (_ventana == null || !_ventana.IsLoaded)
            {
                _ventana = new ReportesWindow(this)
                {
                    Owner = owner
                };
                RefrescarDashboard();
                _ventana.ShowDialog();
            }
            else
            {
                _ventana.Activate();
            }
        }

        /// <summary>
        /// Recarga los datos del día de la base de datos y refresca la vista.
        /// </summary>
        public void RefrescarDashboard()
        {
            if (_ventana == null) return;

            try
            {
                var reportService = new ReportService(_app.Db);
                _dashboard = reportService.GenerarDatosDashboardDia(DateTime.Today);
                AplicarFiltro(_filtroActual);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"No se pudieron cargar las asistencias del día:\n{ex.Message}",
                    "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Cambia el filtro de la tabla de marcajes (Todos / Tardanzas / Por admin)
        /// usando los datos ya cargados, sin volver a consultar la base de datos.
        /// </summary>
        public void CambiarFiltro(string filtro)
        {
            _filtroActual = filtro;
            AplicarFiltro(filtro);
        }

        private void AplicarFiltro(string filtro)
        {
            if (_ventana == null || _dashboard == null) return;

            // Los valores coinciden con el Content de los ComboBoxItem de la UI.
            List<AsistenciaDiaDto> marcajes = filtro switch
            {
                "Solo tardanzas" => _dashboard.Marcajes.Where(m => m.Estado == "Tarde").ToList(),
                "Solo por admin" => _dashboard.Marcajes.Where(m => m.EsPorAdmin).ToList(),
                _ => _dashboard.Marcajes
            };

            _ventana.MostrarDashboard(new DashboardDiaDto
            {
                Fecha = _dashboard.Fecha,
                EmpleadosActivos = _dashboard.EmpleadosActivos,
                MarcajesHoy = _dashboard.MarcajesHoy,
                TardanzasHoy = _dashboard.TardanzasHoy,
                PorAdminHoy = _dashboard.PorAdminHoy,
                Marcajes = marcajes,
                SinMarcar = _dashboard.SinMarcar
            });
        }

        /// <summary>
        /// Datos del reporte semanal (para la vista previa de la pestaña y el PDF).
        /// </summary>
        public List<ReporteEmpleadoDto> ObtenerDatosReporteSemanal(DateTime fechaSeleccionada)
        {
            var reportService = new ReportService(_app.Db);
            return reportService.GenerarDatosReporteSemanal(fechaSeleccionada);
        }

        public void GenerarReporte(DateTime fechaSeleccionada)
        {
            var pdfService = new PdfGeneratorService();
            var datos = ObtenerDatosReporteSemanal(fechaSeleccionada);

            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string reportPath = Path.Combine(docPath, "ControlAcceso_Reportes");

            if (!Directory.Exists(reportPath))
            {
                Directory.CreateDirectory(reportPath);
            }

            // Ajustar la fecha inicio para el nombre del archivo (siempre lunes)
            DateTime fechaInicioSemana = fechaSeleccionada;
            while (fechaInicioSemana.DayOfWeek != DayOfWeek.Monday)
            {
                fechaInicioSemana = fechaInicioSemana.AddDays(-1);
            }

            string archivo = Path.Combine(reportPath, $"Reporte_Semanal_{fechaInicioSemana:yyyy_MM_dd}.pdf");

            pdfService.GenerarReporteSemanalPdf(datos, fechaInicioSemana, archivo);
        }

        /// <summary>
        /// Empleados activos para el selector del reporte detallado.
        /// </summary>
        public List<EmpleadoViewDto> ObtenerEmpleadosParaReporte()
        {
            return _app.EmpleadosViewCache
                .Where(e => e.Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.Nombre)
                .ToList();
        }

        /// <summary>
        /// Consulta el reporte detallado por empleado y rango de fechas.
        /// </summary>
        public ReporteDetalladoEmpleadoDto ConsultarReporteDetallado(int empleadoId, DateTime desde, DateTime hasta)
        {
            var reportService = new ReportService(_app.Db);
            return reportService.GenerarDatosReporteDetallado(empleadoId, desde, hasta);
        }

        /// <summary>
        /// Genera el PDF del reporte detallado y devuelve la ruta del archivo.
        /// </summary>
        public string GenerarReporteDetalladoPdf(ReporteDetalladoEmpleadoDto datos)
        {
            var pdfService = new PdfGeneratorService();

            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string reportPath = Path.Combine(docPath, "ControlAcceso_Reportes");
            if (!Directory.Exists(reportPath))
            {
                Directory.CreateDirectory(reportPath);
            }

            string archivo = Path.Combine(reportPath,
                $"Reporte_Detallado_{datos.EmpleadoId}_{datos.Desde:yyyy_MM_dd}_{datos.Hasta:yyyy_MM_dd}.pdf");

            pdfService.GenerarReporteDetalladoPdf(datos, archivo);
            return archivo;
        }
    }
}
