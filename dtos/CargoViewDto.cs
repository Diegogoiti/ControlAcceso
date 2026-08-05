namespace ControlAcceso.DTOs
{
    public class CargoViewDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty; // "Activo" o "Inactivo"
    }
}
