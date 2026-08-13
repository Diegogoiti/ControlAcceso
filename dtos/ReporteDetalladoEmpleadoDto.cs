using System;
using System.Collections.Generic;

namespace ControlAcceso.DTOs
{
    /// <summary>
    /// Reporte de asistencia detallado de un solo empleado entre dos fechas:
    /// un fila por día con hora, estado y observación, más los totales del período.
    /// </summary>
    public class ReporteDetalladoEmpleadoDto
    {
        public int EmpleadoId { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;

        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }

        public List<ReporteDetalleDiaDto> Dias { get; set; } = new();

        public int DiasTrabajados { get; set; }
        public int Faltas { get; set; }
        public int Retardos { get; set; }
        public int RetardosJustificados { get; set; }
        public double PorcentajeAsistencia { get; set; }
    }
}
