namespace ControlAcceso.DTOs
{
    /// <summary>
    /// Representa la proyección de datos para el Reporte de Asistencia Diaria en la interfaz.
    /// </summary>
    public class EmpleadoViewDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;

        public string Estado { get; set; } = "Inasistente";

        // Propiedades de tiempo (pueden ser string o TimeSpan/TimeOnly según formatees en la consulta)
        public string HoraEntrada { get; set; } = "No calculado";
        public string HoraSalida { get; set; } = "No calculado";

        // Métricas calculadas para la jornada
        public string Retraso { get; set; } = "No calculado";
        public string TiempoExtra { get; set; } = "No calculado"; // En la UI dice "T. Extra"
        public string TotalLaborado { get; set; } = "Incompleto";  // En la UI dice "Total Lab."
    }
}
