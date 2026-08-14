using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ControlAcceso.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ControlAcceso.Services
{
    public class PdfGeneratorService
    {
        public PdfGeneratorService()
        {
            // Requerido por la licencia Community de QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public void GenerarReporteSemanalPdf(List<ReporteEmpleadoDto> datos, DateTime fechaInicio, string rutaDestino)
        {
            DateTime fechaFin = fechaInicio.AddDays(5);
            string subtitulo = $"Semana del {fechaInicio:dd/MM/yyyy} al {fechaFin:dd/MM/yyyy}";

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => ComposeHeader(c, subtitulo));
                    page.Content().Element(c => ComposeContent(c, datos));
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            })
            .GeneratePdf(rutaDestino);

            // Intentar abrir el PDF generado automáticamente
            try
            {
                Process.Start(new ProcessStartInfo(rutaDestino) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // Manejar error si no hay lector de PDF
                Console.WriteLine("No se pudo abrir el PDF automáticamente: " + ex.Message);
            }
        }

        private void ComposeHeader(IContainer container, string subtitulo)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("REPORTE DE ASISTENCIA SEMANAL").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text(subtitulo).FontSize(14).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private void ComposeContent(IContainer container, List<ReporteEmpleadoDto> datos)
        {
            container.PaddingVertical(1, Unit.Centimetre).Table(table =>
            {
                // Definición de las columnas
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(70);  // Cedula
                    columns.RelativeColumn();    // Empleado (toma el espacio sobrante)
                    columns.ConstantColumn(90);  // Posicion

                    // Dias de la semana (L a S) - ancho aumentado para nombres largos como 'Miércoles'
                    for(int i = 0; i < 6; i++) columns.ConstantColumn(48);

                    columns.ConstantColumn(55); // Dias Asist.
                    columns.ConstantColumn(55); // Dias Falt.
                    columns.ConstantColumn(55); // Tardanzas
                    columns.ConstantColumn(55); // Excusa (Por Admin)
                    columns.ConstantColumn(70); // % Asist
                });

                // Cabecera de la tabla
                table.Header(header =>
                {
                    header.Cell().Element(BlockHeader).Text("CEDULA");
                    header.Cell().Element(BlockHeader).Text("Empleado");
                    header.Cell().Element(BlockHeader).Text("POSICION");

                    header.Cell().Element(BlockHeader).Text("Lunes");
                    header.Cell().Element(BlockHeader).Text("Martes");
                    header.Cell().Element(BlockHeader).Text("Miércoles");
                    header.Cell().Element(BlockHeader).Text("Jueves");
                    header.Cell().Element(BlockHeader).Text("Viernes");
                    header.Cell().Element(BlockHeader).Text("Sábado");

                    header.Cell().Element(BlockHeader).Text("Días\nAsistidos");
                    header.Cell().Element(BlockHeader).Text("Días\nFaltados");
                    header.Cell().Element(BlockHeader).Text("Tardanzas");
                    header.Cell().Element(BlockHeader).Text("Ret.\nJustif.");
                    header.Cell().Element(BlockHeader).Text("% Asistencia");
                });

                // Contenido de la tabla (Iterar por cada empleado)
                foreach (var emp in datos)
                {
                    table.Cell().Element(BlockCell).Text(emp.Cedula);
                    table.Cell().Element(BlockCell).Text(emp.Nombre);

                    // Posicion con fondo rojo/texto blanco
                    table.Cell().Background(Colors.White).Padding(2).AlignCenter().AlignMiddle()
                         .Text(emp.Posicion).FontColor(Colors.Black).FontSize(9).Bold();

                    // Días de la semana (Lunes a Sábado -> enum 1 a 6)
                    for (int dia = 1; dia <= 6; dia++)
                    {
                        string estado = emp.DiasAsistencia.ContainsKey(dia) ? emp.DiasAsistencia[dia] : "";

                        if (string.IsNullOrEmpty(estado))
                        {
                            // Día futuro (en blanco)
                            table.Cell().Background(Colors.White).Border(1).BorderColor(Colors.Grey.Lighten3);
                        }
                        else
                        {
                            // A = a tiempo (verde), T = retardo (amarillo), RJ = retardo
                            // justificado por administrador (azul), F = falta (rojo).
                            string colorFondo = estado switch
                            {
                                "A" => Colors.Green.Lighten4,
                                "T" => Colors.Yellow.Lighten3,
                                "RJ" => Colors.Blue.Lighten3,
                                _ => Colors.Red.Lighten4
                            };
                            string colorTexto = estado switch
                            {
                                "A" => Colors.Green.Darken2,
                                "T" => Colors.Orange.Darken2,
                                "RJ" => Colors.Blue.Darken2,
                                _ => Colors.Red.Darken2
                            };

                            table.Cell().Background(colorFondo).Border(1).BorderColor(Colors.Grey.Lighten3)
                                 .AlignCenter().AlignMiddle()
                                 .Text(estado).FontColor(colorTexto).Bold();
                        }
                    }

                    // Resúmenes numéricos
                    table.Cell().Element(BlockNumCell).Text(emp.DiasAsistidos.ToString());
                    table.Cell().Element(BlockNumCell).Text(emp.DiasFaltados.ToString());
                    table.Cell().Element(BlockNumCell).Text(emp.Tardanzas.ToString());
                    table.Cell().Element(BlockNumCell).Text(emp.PorAdministrador.ToString());

                    // Barra de progreso visual para el porcentaje (similar a la imagen)
                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(2).AlignMiddle().Row(row =>
                    {
                        row.AutoItem().Width(40).Height(12).Background(Colors.Grey.Lighten3).Row(bar =>
                        {
                            bar.AutoItem().Width((float)(40 * emp.PorcentajeAsistencia / 100.0)).Height(12).Background(Colors.Blue.Darken1);
                        });
                        row.RelativeItem().AlignCenter().Text($"{emp.PorcentajeAsistencia}%").FontSize(9).Bold();
                    });
                }
            });
        }

        /// <summary>
        /// Reporte detallado por empleado: por cada día del rango muestra la hora
        /// de entrada, el estado (a tiempo / retardo / retardo justificado / falta)
        /// y la observación, con totales al final. Por día se toma un solo registro
        /// (el del admin si existe, si no la lectura más temprana). Los registros manuales del
        /// administrador se computan como retardo justificado.
        /// </summary>
        public void GenerarReporteDetalladoPdf(ReporteDetalladoEmpleadoDto datos, string rutaDestino)
        {
            string subtitulo = $"{datos.Nombre} — Cédula {datos.Cedula} ({datos.Cargo})\n" +
                               $"Del {datos.Desde:dd/MM/yyyy} al {datos.Hasta:dd/MM/yyyy}";

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => ComposeHeaderDetallado(c, subtitulo));
                    page.Content().Element(c => ComposeContentDetallado(c, datos));
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            })
            .GeneratePdf(rutaDestino);

            try
            {
                Process.Start(new ProcessStartInfo(rutaDestino) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("No se pudo abrir el PDF automáticamente: " + ex.Message);
            }
        }

        private void ComposeHeaderDetallado(IContainer container, string subtitulo)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("REPORTE DE ASISTENCIA DETALLADO").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text(subtitulo).FontSize(13).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private void ComposeContentDetallado(IContainer container, ReporteDetalladoEmpleadoDto datos)
        {
            container.Column(column =>
            {
                // Tarjetas de resumen. Se usa RelativeItem() para que las 4 tarjetas
                // repartan el ancho disponible de la página (los anchos fijos en
                // puntos desbordaban el A4 y causaban "conflicting size constraints").
                column.Item().PaddingVertical(0.5f, Unit.Centimetre).Row(row =>
                {
                    row.RelativeItem().Element(c => CardResumen(c, "Días trabajados", datos.DiasTrabajados.ToString(), Colors.Green.Darken2, Colors.Green.Lighten4));
                    row.ConstantItem(16);
                    row.RelativeItem().Element(c => CardResumen(c, "Faltas", datos.Faltas.ToString(), Colors.Red.Darken2, Colors.Red.Lighten4));
                    row.ConstantItem(16);
                    row.RelativeItem().Element(c => CardResumen(c, "Retardos", datos.Retardos.ToString(), Colors.Orange.Darken2, Colors.Yellow.Lighten3));
                    row.ConstantItem(16);
                    row.RelativeItem().Element(c => CardResumen(c, "Retardos justif.", datos.RetardosJustificados.ToString(), Colors.Blue.Darken2, Colors.Blue.Lighten3));
                });

                column.Item().PaddingVertical(0.5f, Unit.Centimetre).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(70);  // Fecha
                        columns.ConstantColumn(80);  // Día
                        columns.ConstantColumn(70);  // Hora
                        columns.RelativeColumn();    // Estado
                        columns.ConstantColumn(70);  // Min ret.
                        columns.RelativeColumn(1.5f); // Observación
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(BlockHeader).Text("Fecha");
                        header.Cell().Element(BlockHeader).Text("Día");
                        header.Cell().Element(BlockHeader).Text("Hora entrada");
                        header.Cell().Element(BlockHeader).Text("Estado");
                        header.Cell().Element(BlockHeader).Text("Min ret.");
                        header.Cell().Element(BlockHeader).Text("Observación / Motivo");
                    });

                    foreach (var dia in datos.Dias)
                    {
                        table.Cell().Element(BlockCell).Text(dia.Fecha.ToString("dd/MM/yyyy"));
                        table.Cell().Element(BlockCell).Text(dia.Dia);
                        table.Cell().Element(BlockCell).AlignCenter().Text(dia.HoraEntradaTexto);

                        string colorFondo = dia.Estado switch
                        {
                            "A tiempo" => Colors.Green.Lighten4,
                            "Retardo" => Colors.Yellow.Lighten3,
                            "Retardo justificado" => Colors.Blue.Lighten3,
                            _ => Colors.Red.Lighten4
                        };
                        string colorTexto = dia.Estado switch
                        {
                            "A tiempo" => Colors.Green.Darken2,
                            "Retardo" => Colors.Orange.Darken2,
                            "Retardo justificado" => Colors.Blue.Darken2,
                            _ => Colors.Red.Darken2
                        };

                        table.Cell().Background(colorFondo).Border(1).BorderColor(Colors.Grey.Lighten3)
                            .Padding(2).AlignMiddle().AlignCenter()
                            .Text(dia.Estado).FontColor(colorTexto).Bold().FontSize(9);

                        table.Cell().Element(BlockNumCell).Text(dia.MinutosTexto);
                        table.Cell().Element(BlockCell).Text(string.IsNullOrWhiteSpace(dia.Observacion) ? "—" : dia.Observacion);
                    }
                });

                column.Item().PaddingTop(0.6f, Unit.Centimetre).Text(
                    $"Total: {datos.DiasTrabajados} días trabajados · {datos.Faltas} faltas · {datos.Retardos} retardos · {datos.RetardosJustificados} retardos justificados · {datos.PorcentajeAsistencia}% asistencia")
                    .FontSize(10).SemiBold().FontColor(Colors.Grey.Darken2);
            });
        }

        private static void CardResumen(IContainer container, string titulo, string valor, string colorTexto, string colorFondo)
        {
            container.Background(colorFondo).Border(1).BorderColor(Colors.Grey.Lighten3)
                .Padding(8).Column(c =>
                {
                    c.Item().Text(titulo).FontSize(9).FontColor(Colors.Grey.Darken2);
                    c.Item().Text(valor).FontSize(16).Bold().FontColor(colorTexto);
                });
        }

        // Estilos auxiliares para QuestPDF
        static IContainer BlockHeader(IContainer container)
        {
            return container.Background(Colors.Blue.Darken3)
                .Border(1).BorderColor(Colors.White)
                .PaddingVertical(4).PaddingHorizontal(2)
                .AlignCenter().AlignMiddle()
                .DefaultTextStyle(x => x.FontColor(Colors.White).Bold().FontSize(9));
        }

        static IContainer BlockCell(IContainer container)
        {
            return container.Border(1).BorderColor(Colors.Grey.Lighten3)
                .Padding(2).AlignMiddle()
                .DefaultTextStyle(x => x.FontSize(9));
        }

        static IContainer BlockNumCell(IContainer container)
        {
            return container.Border(1).BorderColor(Colors.Grey.Lighten3)
                .Padding(2).AlignCenter().AlignMiddle()
                .DefaultTextStyle(x => x.FontSize(9).Bold());
        }
    }
}
