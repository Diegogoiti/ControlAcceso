namespace ControlAcceso.DTOs
{
    public class HuellaEmpleadoDto
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public int Dedo { get; set; }
        public byte[] TemplateHuella { get; set; } = Array.Empty<byte>();
    }
}
