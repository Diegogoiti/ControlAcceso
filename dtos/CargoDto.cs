namespace ControlAcceso.DTOs
{
    /// <summary>
    /// Representa un rol/cargo dentro del sistema.
    /// Se mapea a la tabla 'roles' de la base de datos.
    /// </summary>
    public class CargoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
