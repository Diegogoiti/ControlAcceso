using System;

namespace ControlAcceso.DTOs
{
    /// <summary>
    /// Un día dentro del reporte detallado por empleado.
    /// Estado: "A tiempo" | "Retardo" | "Retardo justificado" | "Falta".
    /// Los registros manuales del administrador siempre son "Retardo justificado".
    /// </summary>
    public class ReporteDetalleDiaDto
    {
        public DateTime Fecha { get; set; }
        public string Dia { get; set; } = string.Empty;
        public TimeSpan? HoraEntrada { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int MinutosRetraso { get; set; }
        public string Observacion { get; set; } = string.Empty;
    }
}
