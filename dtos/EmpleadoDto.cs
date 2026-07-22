namespace ControlAcceso.DTOs
{
    public class EmpleadoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Cedula { get; set; }
        public byte[]? HuellaBytes { get; set; }
        public bool Activo { get; set; }
    }
}
