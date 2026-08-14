using System;

namespace ControlAcceso.DTOs
{
    /// <summary>
    /// Vista de un marcaje de asistencia para el dashboard del día:
    /// incluye el nombre y cargo del empleado y un estado calculado
    /// ("A tiempo", "Tarde" o "Retardo justificado").
    /// </summary>
    public class AsistenciaDiaDto
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public DateTime Hora { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public bool EsPorAdmin { get; set; }
        public string? Observacion { get; set; }
    }
}
