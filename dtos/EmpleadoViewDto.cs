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
        public string Estado { get; set; } = string.Empty;
        public DateOnly FechaNacimiento { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string TelefonoEmergencia { get; set; } = string.Empty;
        public int RolNombre { get; set; }
        public DateOnly FechaIngreso { get; set; }


    }
}
