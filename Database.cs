using System.Collections.Generic;
using MySql.Data.MySqlClient;



namespace ControlAcceso.Services

{
    public class Database
    {
        private readonly string _connString = "Server=localhost;Database=acceso_db;Uid=root;Pwd=;";

        // READ: Obtener todos los empleados
        public List<Empleado> ObtenerEmpleados()
        {
            var empleados = new List<Empleado>();
            using (var conn = new MySqlConnection(_connString))
            {
                conn.Open();
                string query = "SELECT id, Nombre, Cedula, HuellaTemplate FROM Empleados";
                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        empleados.Add(new Empleado(
                            reader.GetInt32("id"),
                            reader.GetString("Nombre"),
                            reader.GetInt32("Cedula"),
                            FingerprintService.CargarDesdeBytes((byte[])reader["HuellaTemplate"])
                        ));
                    }
                }
            }
            return empleados;
        }

        // CREATE: Insertar un empleado (ajusta si tienes más campos)
        public void AgregarEmpleado(Empleado emp)
        {
            using (var conn = new MySqlConnection(_connString))
            {
                conn.Open();
                string query = "INSERT INTO Empleados (Nombre, Cedula, HuellaTemplate) VALUES (@nombre, @cedula, @huellaTemplate)";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", emp.Nombre);
                    cmd.Parameters.AddWithValue("@cedula", emp.Cedula);
                    cmd.Parameters.AddWithValue("@huellaTemplate", emp.Huella.ToByteArray());
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // DELETE: Eliminar
        public void EliminarEmpleado(int id)
        {
            using (var conn = new MySqlConnection(_connString))
            {
                conn.Open();
                string query = "DELETE FROM Empleados WHERE id = @id";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void RegistrarAsistencia(Asistencia asistencia)
        {
            using (var conn = new MySqlConnection(_connString))
            {
                conn.Open();
                string query = "INSERT INTO Asistencia (EmpleadoID, Timestamp) VALUES (@empleadoId, NOW())";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@empleadoId", asistencia.EmpleadoID);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
