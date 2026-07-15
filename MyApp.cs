using ControlAcceso.Services;

namespace ControlAcceso
{
    public class MyApp
    {
        // Instancia global de la base de datos
        public Database Db { get; } = new Database();

        // Estado global: lista de empleados cargada en memoria
        public List<Empleado> Empleados { get; set; } = new List<Empleado>();

        public void CargarEmpleadosDesdeDb()
        {
            Empleados = Db.ObtenerEmpleados();
        }

        public Empleado ObtenerEmpleadoPorId(int id)
        {
            return Empleados.FirstOrDefault(e => e.id == id);
        }
    }
}
