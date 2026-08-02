using System;
using System.IO;
using ControlAcceso.Application;
using ControlAcceso.Services;

namespace ControlAcceso.UI.controladores
{
    public class ReportesController
    {
        private readonly MyApp _app;
        private ReportesWindow? _ventana;

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
                _ventana.ShowDialog();
            }
            else
            {
                _ventana.Activate();
            }
        }

        public void GenerarReporte(DateTime fechaSeleccionada)
        {
            var reportService = new ReportService(_app.Db);
            var pdfService = new PdfGeneratorService();

            var datos = reportService.GenerarDatosReporteSemanal(fechaSeleccionada);

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
    }
}