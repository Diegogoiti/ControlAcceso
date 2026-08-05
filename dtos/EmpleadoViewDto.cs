namespace ControlAcceso.DTOs
{
    public class EmpleadoViewDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;   // "Activo" / "Inactivo"
        public DateOnly FechaNacimiento { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string TelefonoEmergencia { get; set; } = string.Empty;
        public string NombreRol { get; set; } = string.Empty;   // ← Nombre del rol (para UI)
        public DateOnly FechaIngreso { get; set; }
    }
}
