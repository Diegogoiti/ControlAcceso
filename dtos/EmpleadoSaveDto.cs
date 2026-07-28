namespace ControlAcceso.DTOs
{
    public class EmpleadoSaveDto
    {
        public int Id { get; set; }

        public string NombreCompleto { get; set; } = string.Empty;
        public int Cedula { get; set; }
        public DateOnly FechaNacimiento { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string TelefonoEmergencia { get; set; } = string.Empty;
        public int RolId { get; set; }

        public HuellaEmpleadoDto[] Huellas { get; set; } = new HuellaEmpleadoDto[3];
    }
}
