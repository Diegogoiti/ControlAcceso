public class Empleado
{
    public int id { get; set; }
    public string Nombre { get; set; }
    public int Cedula { get; set; }

    // Constructor que obliga a pasar los datos
    public Empleado(int id_empleado, string nombre, int cedula)
    {
        id = id_empleado;
        Nombre = nombre;
        Cedula = cedula;
    }
}
