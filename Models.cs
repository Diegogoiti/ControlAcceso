using Microsoft.VisualBasic;
using SourceAFIS;
using System;


public class Empleado
{
    public int id { get; set; }
    public string Nombre { get; set; }
    public int Cedula { get; set; }
    public FingerprintTemplate Huella { get; set; }

    // Constructor que obliga a pasar los datos
    public Empleado(int id_empleado, string nombre, int cedula, FingerprintTemplate huella)
    {
        id = id_empleado;
        Nombre = nombre;
        Cedula = cedula;
        Huella = huella;
    }
}


public class Asistencia
{
    public int EmpleadoID { get; set; }
    public DateTime Timestamp { get; set; }

    public Asistencia(int empleadoId, DateTime timestamp)
    {
        EmpleadoID = empleadoId;
        Timestamp = timestamp;
    }
}
