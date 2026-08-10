namespace ControlAcceso.DTOs
{
    /// <summary>
    /// Empleado activo que aún no ha marcado en el día: se muestra en el
    /// dashboard para ver de un vistazo quién está pendiente de llegar.
    /// </summary>
    public class EmpleadoPendienteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
    }
}
