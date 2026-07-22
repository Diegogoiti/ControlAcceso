namespace ControlAcceso.DTOs
{
    public class EmpleadoFilter
    {
        public int? Id { get; set; }
        public int? Cedula { get; set; }
        public string? Nombre { get; set; }
        public bool? SoloActivos { get; set; }
    }
}
