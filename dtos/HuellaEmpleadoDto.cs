namespace ControlAcceso.DTOs
{
    public class HuellaEmpleadoDto
    {
        public int EmpleadoId { get; set; }
        public byte[] TemplateHuella { get; set; } = Array.Empty<byte>();
    }
}