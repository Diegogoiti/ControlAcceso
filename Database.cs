using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace ControlAcceso.Services
{
    public class Database
    {
        private readonly string _connString = "Server=localhost;Database=acceso_db;Uid=root;Pwd=;";

        // READ: Obtener todos los empleados (Incluyendo el estado Activo)
        public List<Empleado> ObtenerEmpleados()
        {
            var empleados = new List<Empleado>();
            using (var conn = new MySqlConnection(_connString))
            {
                conn.Open();
                string query = "SELECT id, Nombre, Cedula, HuellaTemplate, Activo FROM Empleados";
                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        // Declaramos la ID fuera del try para que sea accesible en el bloque catch
                        int idActual = -1;

                        try
                        {
                            // 1. Extraemos los datos básicos
                            idActual = reader.GetInt32("id");
                            string nombre = reader.GetString("Nombre");
                            int cedula = reader.GetInt32("Cedula");
                            bool activo = reader.GetBoolean("Activo");

                            // 2. Procesamos la huella de forma estricta
                            SourceAFIS.FingerprintTemplate huella;

                            if (reader["HuellaTemplate"] != DBNull.Value)
                            {
                                byte[] bytes = (byte[])reader["HuellaTemplate"];
                                // Si esto truena por el CBOR, salta directo al catch
                                huella = FingerprintService.CargarDesdeBytes(bytes);
                            }
                            else
                            {
                                Console.WriteLine($"Empleado [ID: {idActual}] no tiene datos en el campo HuellaTemplate (NULL).");
                                continue;
                            }
                            i++;

                            // 3. Si todo salió bien, lo guardamos en la lista
                            empleados.Add(new Empleado(idActual, nombre, cedula, huella, activo));
                        }
                        catch (Exception ex)
                        {
                            // Ahora sí puedes ver exactamente cuál ID generó la excepción de Dahomey.Cbor
                            Console.WriteLine($"[ERROR] Error al procesar la huella del Empleado [ID: {idActual}]. Detalles: {ex.Message}");
                            continue;
                        }
                    }
                }
            }
            return empleados;
        }

        // CREATE: Insertar un empleado (Guardando el estado activo por defecto)
        public void AgregarEmpleado(Empleado emp)
        {
            using (var conn = new MySqlConnection(_connString))
            {
                conn.Open();
                // Añadimos 'Activo' en el INSERT para asegurar la consistencia del modelo
                string query = "INSERT INTO Empleados (Nombre, Cedula, HuellaTemplate, Activo) VALUES (@nombre, @cedula, @huellaTemplate, @activo)";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", emp.Nombre);
                    cmd.Parameters.AddWithValue("@cedula", emp.Cedula);
                    cmd.Parameters.AddWithValue("@huellaTemplate", emp.Huella.ToByteArray());
                    cmd.Parameters.AddWithValue("@activo", emp.Activo); // Envía true/false mapeado a 1/0
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // UPDATE: Alternar o cambiar el estado de activación de un empleado
        public void CambiarEstadoEmpleado(int id, bool activo)
        {
            using (var conn = new MySqlConnection(_connString))
            {
                conn.Open();
                string query = "UPDATE Empleados SET Activo = @activo WHERE id = @id";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@activo", activo);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // UPDATE: Reemplazar o actualizar la huella digital de un empleado existente
        public void ActualizarHuellaEmpleado(int id, byte[] nuevaHuellaBytes)
        {
            using (var conn = new MySqlConnection(_connString))
            {
                conn.Open();
                string query = "UPDATE Empleados SET HuellaTemplate = @huellaTemplate WHERE id = @id";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@huellaTemplate", nuevaHuellaBytes);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // DELETE: Eliminar físicamente de la base de datos
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

        // CREATE: Registrar marca de asistencia diaria
        public void RegistrarAsistencia(Asistencia asistencia)
        {
            using (var conn = new MySqlConnection(_connString))
            {
                conn.Open();
                string query = "INSERT INTO Asistencia (EmpleadoID, Timestamp, Tipo) VALUES (@empleadoId, NOW(), @tipo)";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@empleadoId", asistencia.EmpleadoID);
                    cmd.Parameters.AddWithValue("@tipo", asistencia.Tipo);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // READ: Obtener asistencias del día actual
        public List<Asistencia> ObtenerAsistencias()
        {
            var asistencias = new List<Asistencia>();
            using (var conn = new MySqlConnection(_connString))
            {
                conn.Open();
                string query = "SELECT EmpleadoID, Timestamp, Tipo FROM Asistencia WHERE DATE(Timestamp) = CURDATE() ORDER BY Timestamp DESC";
                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        asistencias.Add(new Asistencia(
                            reader.GetInt32("EmpleadoID"),
                            reader.GetDateTime("Timestamp"),
                            reader.GetInt32("Tipo")
                        ));
                    }
                }
            }
            return asistencias;
        }

        // READ: Obtener parámetros de configuración global
        public (string password, TimeSpan entrada, TimeSpan salida) ObtenerConfiguracion()
        {
            string passDecodificada = "admin";
            TimeSpan horaEntrada = new TimeSpan(8, 0, 0);
            TimeSpan horaSalida = new TimeSpan(17, 0, 0);

            using (var conn = new MySqlConnection(_connString))
            {
                conn.Open();
                string query = "SELECT AdminPasword, HoraEntrada, HoraSalida FROM configuracion WHERE id = 1";

                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal("AdminPasword")))
                        {
                            string passBase64 = reader.GetString("AdminPasword");
                            byte[] bytes = Convert.FromBase64String(passBase64);
                            passDecodificada = System.Text.Encoding.UTF8.GetString(bytes);
                        }

                        horaEntrada = reader.GetTimeSpan(reader.GetOrdinal("HoraEntrada"));
                        horaSalida = reader.GetTimeSpan(reader.GetOrdinal("HoraSalida"));
                    }
                }
            }

            return (passDecodificada, horaEntrada, horaSalida);
        }

        // UPDATE: Guardar los datos de configuración cifrados
        public void GuardarConfiguracion(TimeSpan entrada, TimeSpan salida, string passwordPlana)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(passwordPlana);
            string passBase64 = Convert.ToBase64String(bytes);

            using (var conn = new MySqlConnection(_connString))
            {
                conn.Open();
                string query = @"UPDATE configuracion
                                 SET AdminPasword = @password,
                                     HoraEntrada = @entrada,
                                     HoraSalida = @salida
                                 WHERE id = 1";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@password", passBase64);
                    cmd.Parameters.AddWithValue("@entrada", entrada);
                    cmd.Parameters.AddWithValue("@salida", salida);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
