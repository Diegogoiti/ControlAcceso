using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using ControlAcceso.DTOs;

namespace ControlAcceso.Database
{
    public class MySqlDatabaseAdapter : IDatabase
    {
        private readonly string _connString;

        public MySqlDatabaseAdapter(string connString = "Server=localhost;Database=acceso_db;Uid=root;Pwd=;")
        {
            _connString = connString;
        }

        // READ: Obtención dinámica basada en Filtros
        public List<EmpleadoDto> ObtenerEmpleados(EmpleadoFilter? filtro = null)
        {
            var empleados = new List<EmpleadoDto>();

            using (var conn = new MySqlConnection(_connString))
            {
                conn.Open();

                // Base de la consulta
                var queryBuilder = new StringBuilder("SELECT id, Nombre, Cedula, HuellaTemplate, Activo FROM Empleados WHERE 1=1");
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;

                    // Si mandaron un filtro, agregamos condiciones dinámicamente
                    if (filtro != null)
                    {
                        if (filtro.Id.HasValue)
                        {
                            queryBuilder.Append(" AND id = @id");
                            cmd.Parameters.AddWithValue("@id", filtro.Id.Value);
                        }

                        if (filtro.Cedula.HasValue)
                        {
                            queryBuilder.Append(" AND Cedula = @cedula");
                            cmd.Parameters.AddWithValue("@cedula", filtro.Cedula.Value);
                        }

                        if (!string.IsNullOrWhiteSpace(filtro.Nombre))
                        {
                            queryBuilder.Append(" AND Nombre LIKE @nombre");
                            cmd.Parameters.AddWithValue("@nombre", $"%{filtro.Nombre}%");
                        }

                        if (filtro.SoloActivos.HasValue)
                        {
                            queryBuilder.Append(" AND Activo = @activo");
                            cmd.Parameters.AddWithValue("@activo", filtro.SoloActivos.Value);
                        }
                    }

                    cmd.CommandText = queryBuilder.ToString();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int idActual = reader.GetInt32("id");
                            string nombre = reader.GetString("Nombre");
                            int cedula = reader.GetInt32("Cedula");
                            bool activo = reader.GetBoolean("Activo");

                            // Extraemos solo los bytes crudos (La DB no sabe qué es SourceAFIS)
                            byte[]? huellaBytes = null;
                            if (reader["HuellaTemplate"] != DBNull.Value)
                            {
                                huellaBytes = (byte[])reader["HuellaTemplate"];
                            }

                            // Retornamos un DTO puro y limpio
                            empleados.Add(new EmpleadoDto
                            {
                                Id = idActual,
                                Nombre = nombre,
                                Cedula = cedula,
                                HuellaBytes = huellaBytes,
                                Activo = activo
                            });
                        }
                    }
                }
            }
            return empleados;
        }

        public void AgregarEmpleado(EmpleadoDto emp)
        {
            using (var conn = new MySqlConnection(_connString))
            {
                conn.Open();
                string query = "INSERT INTO Empleados (Nombre, Cedula, HuellaTemplate, Activo) VALUES (@nombre, @cedula, @huellaTemplate, @activo)";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", emp.Nombre);
                    cmd.Parameters.AddWithValue("@cedula", emp.Cedula);
                    cmd.Parameters.AddWithValue("@huellaTemplate", emp.HuellaBytes);
                    cmd.Parameters.AddWithValue("@activo", emp.Activo);
                    cmd.ExecuteNonQuery();
                }
            }
        }

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

        public void RegistrarAsistencia(AsistenciaDto asistencia)
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

        public List<AsistenciaDto> ObtenerAsistencias(AsistenciaFilter? filtro = null)
        {
            var asistencias = new List<AsistenciaDto>();

            using (var conn = new MySqlConnection(_connString))
            {
                conn.Open();

                var queryBuilder = new StringBuilder("SELECT EmpleadoID, Timestamp, Tipo FROM Asistencia WHERE 1=1");

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;

                    if (filtro != null)
                    {
                        if (filtro.EmpleadoId.HasValue)
                        {
                            queryBuilder.Append(" AND EmpleadoID = @empleadoId");
                            cmd.Parameters.AddWithValue("@empleadoId", filtro.EmpleadoId.Value);
                        }

                        if (filtro.FechaInicio.HasValue)
                        {
                            // Desde las 00:00:00 del día
                            queryBuilder.Append(" AND Timestamp >= @fechaInicio");
                            cmd.Parameters.AddWithValue("@fechaInicio", filtro.FechaInicio.Value.Date);
                        }

                        if (filtro.FechaFin.HasValue)
                        {
                            // Hasta las 23:59:59 del día
                            queryBuilder.Append(" AND Timestamp <= @fechaFin");
                            cmd.Parameters.AddWithValue("@fechaFin", filtro.FechaFin.Value.Date.AddDays(1).AddTicks(-1));
                        }

                        if (filtro.Tipo.HasValue)
                        {
                            queryBuilder.Append(" AND Tipo = @tipo");
                            cmd.Parameters.AddWithValue("@tipo", filtro.Tipo.Value);
                        }
                    }

                    queryBuilder.Append(" ORDER BY Timestamp DESC");
                    cmd.CommandText = queryBuilder.ToString();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            asistencias.Add(new AsistenciaDto
                            {
                                EmpleadoID = reader.GetInt32("EmpleadoID"),
                                Timestamp = reader.GetDateTime("Timestamp"),
                                Tipo = reader.GetInt32("Tipo") // tinyint de MySQL mapea directo a int
                            });
                        }
                    }
                }
            }
            return asistencias;
        }

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
                            passDecodificada = Encoding.UTF8.GetString(bytes);
                        }

                        horaEntrada = reader.GetTimeSpan(reader.GetOrdinal("HoraEntrada"));
                        horaSalida = reader.GetTimeSpan(reader.GetOrdinal("HoraSalida"));
                    }
                }
            }

            return (passDecodificada, horaEntrada, horaSalida);
        }

        public void ActualizarEmpleado(EmpleadoDto emp)
        {
            using (var conn = new MySqlConnection(_connString))
            {
                conn.Open();
                string query = @"UPDATE Empleados
                                 SET Nombre = @nombre,
                                     Cedula = @cedula,
                                     HuellaTemplate = @huellaTemplate,
                                     Activo = @activo
                                 WHERE id = @id";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", emp.Id);
                    cmd.Parameters.AddWithValue("@nombre", emp.Nombre);
                    cmd.Parameters.AddWithValue("@cedula", emp.Cedula);
                    cmd.Parameters.AddWithValue("@huellaTemplate", (object?)emp.HuellaBytes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@activo", emp.Activo);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void GuardarConfiguracion(TimeSpan entrada, TimeSpan salida, string passwordPlana)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(passwordPlana);
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

        public List<HuellaEmpleadoDto> ObtenerHuellasActivas()
        {
            var huellas = new List<HuellaEmpleadoDto>();

            using (var conn = new MySqlConnection(_connString))
            {
                conn.Open();

                // Trae únicamente ID y Huella de los empleados activos que sí tengan huella registrada
                string query = @"SELECT id, HuellaTemplate
                                FROM Empleados
                                WHERE Activo = 1
                                  AND HuellaTemplate IS NOT NULL";

                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        huellas.Add(new HuellaEmpleadoDto
                        {
                            EmpleadoId = reader.GetInt32("id"),
                            TemplateHuella = (byte[])reader["HuellaTemplate"]
                        });
                    }
                }
            }

            return huellas;
        }
    }
}
