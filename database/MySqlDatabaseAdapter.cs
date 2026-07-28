using System;
using System.Collections.Generic;
using ControlAcceso.DTOs;
using MySql.Data.MySqlClient;

namespace ControlAcceso.Database
{
    public class MySqlDatabaseAdapter : IDatabase
    {
        private readonly string _connectionString;

        public MySqlDatabaseAdapter(string connectionString = "Server=localhost;Database=acceso_db;Uid=root;Pwd=;")
        {
            _connectionString = connectionString;
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        #region --- Consultas / Lectura ---

        public List<EmpleadoDto> ObtenerEmpleados(EmpleadoFilter filtro)
        {
            var empleados = new List<EmpleadoDto>();

            using var conn = GetConnection();
            conn.Open();

            string query = @"
                SELECT id, cedula, nombre_completo, fecha_nacimiento, direccion,
                       telefono, telefono_emergencia, rol_id, fecha_ingreso, activo
                FROM empleados
                WHERE 1=1";

            if (filtro.SoloActivos)
            {
                query += " AND activo = 1";
            }

            if (!string.IsNullOrWhiteSpace(filtro.NombreOCedula))
            {
                query += " AND (nombre_completo LIKE @busqueda OR CAST(cedula AS CHAR) LIKE @busqueda)";
            }

            using var cmd = new MySqlCommand(query, conn);

            if (!string.IsNullOrWhiteSpace(filtro.NombreOCedula))
            {
                cmd.Parameters.AddWithValue("@busqueda", $"%{filtro.NombreOCedula.Trim()}%");
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                empleados.Add(new EmpleadoDto
                {
                    Id = reader.GetInt32("id"),
                    Cedula = reader.GetInt32("cedula"),
                    NombreCompleto = reader.GetString("nombre_completo"),
                    FechaNacimiento = DateOnly.FromDateTime(reader.GetDateTime("fecha_nacimiento")),
                    Direccion = reader.IsDBNull(reader.GetOrdinal("direccion")) ? null : reader.GetString("direccion"),
                    Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? null : reader.GetString("telefono"),
                    TelefonoEmergencia = reader.IsDBNull(reader.GetOrdinal("telefono_emergencia")) ? null : reader.GetString("telefono_emergencia"),
                    RolId = reader.GetInt32("rol_id"),
                    FechaIngreso = DateOnly.FromDateTime(reader.GetDateTime("fecha_ingreso")),
                    Activo = reader.GetBoolean("activo")
                });
            }

            return empleados;
        }

        public List<HuellaEmpleadoDto> ObtenerHuellasActivas()
        {
            var huellas = new List<HuellaEmpleadoDto>();

            using var conn = GetConnection();
            conn.Open();

            string query = @"
                SELECT h.id, h.empleado_id, h.dedo, h.template
                FROM huellas h
                INNER JOIN empleados e ON h.empleado_id = e.id
                WHERE e.activo = 1";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                huellas.Add(new HuellaEmpleadoDto
                {
                    Id = reader.GetInt32("id"),
                    EmpleadoId = reader.GetInt32("empleado_id"),
                    Dedo = reader.GetInt32("dedo"),
                    TemplateHuella = (byte[])reader["template"]
                });
            }

            return huellas;
        }

        public List<AsistenciaDto> ObtenerAsistencias(AsistenciaFilter filtro)
        {
            var asistencias = new List<AsistenciaDto>();

            using var conn = GetConnection();
            conn.Open();

            string query = @"
                SELECT id, empleado_id, fecha, hora, por_administrador, observacion
                FROM asistencia
                WHERE 1=1";

            if (filtro.FechaInicio.HasValue)
            {
                query += " AND fecha >= @fechaInicio";
            }

            if (filtro.FechaFin.HasValue)
            {
                query += " AND fecha <= @fechaFin";
            }

            if (filtro.EmpleadoId.HasValue)
            {
                query += " AND empleado_id = @empleadoId";
            }

            query += " ORDER BY fecha DESC, hora DESC";

            using var cmd = new MySqlCommand(query, conn);

            if (filtro.FechaInicio.HasValue)
                cmd.Parameters.AddWithValue("@fechaInicio", filtro.FechaInicio.Value.Date);

            if (filtro.FechaFin.HasValue)
                cmd.Parameters.AddWithValue("@fechaFin", filtro.FechaFin.Value.Date);

            if (filtro.EmpleadoId.HasValue)
                cmd.Parameters.AddWithValue("@empleadoId", filtro.EmpleadoId.Value);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                DateTime fecha = reader.GetDateTime("fecha");
                TimeSpan hora = reader.GetTimeSpan("hora");
                DateTime timestampCombinado = fecha.Date.Add(hora);

                asistencias.Add(new AsistenciaDto
                {
                    Id = reader.GetInt32("id"),
                    EmpleadoID = reader.GetInt32("empleado_id"),
                    Timestamp = timestampCombinado,
                    Tipo = 1,
                    PorAdministrador = reader.GetBoolean("por_administrador"),
                    Observacion = reader.IsDBNull(reader.GetOrdinal("observacion")) ? null : reader.GetString("observacion")
                });
            }

            return asistencias;
        }

        #endregion

        #region --- Escritura / Registro ---

        public bool RegistrarAsistencia(AsistenciaDto asistencia)
        {
            using var conn = GetConnection();
            conn.Open();

            string query = @"
                INSERT INTO asistencia (empleado_id, fecha, hora, por_administrador, observacion)
                VALUES (@empleadoId, CURDATE(), CURTIME(), @porAdmin, @observacion)";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@empleadoId", asistencia.EmpleadoID);
            cmd.Parameters.AddWithValue("@porAdmin", asistencia.PorAdministrador ? 1 : 0);
            cmd.Parameters.AddWithValue("@observacion", (object?)asistencia.Observacion ?? DBNull.Value);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool AgregarEmpleado(EmpleadoSaveDto empleado)
        {
            using var conn = GetConnection();
            conn.Open();

            string query = @"
                INSERT INTO empleados (cedula, nombre_completo, fecha_nacimiento, direccion, telefono, telefono_emergencia, rol_id, fecha_ingreso, activo)
                VALUES (@cedula, @nombre, @fechaNac, @direccion, @telefono, @telEmergencia, @rolId, CURDATE(), 1)";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@cedula", empleado.Cedula);
            cmd.Parameters.AddWithValue("@nombre", empleado.NombreCompleto);
            cmd.Parameters.AddWithValue("@fechaNac", empleado.FechaNacimiento);
            cmd.Parameters.AddWithValue("@direccion", (object?)empleado.Direccion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@telefono", (object?)empleado.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@telEmergencia", (object?)empleado.TelefonoEmergencia ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rolId", empleado.RolId <= 0 ? 1 : empleado.RolId);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool ActualizarEmpleado( EmpleadoSaveDto empleado)
        {
            using var conn = GetConnection();
            conn.Open();

            string query = @"
                UPDATE empleados
                SET cedula = @cedula,
                    nombre_completo = @nombre,
                    fecha_nacimiento = @fechaNac,
                    direccion = @direccion,
                    telefono = @telefono,
                    telefono_emergencia = @telEmergencia,
                    rol_id = @rolId
                WHERE id = @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", empleado.Id);
            cmd.Parameters.AddWithValue("@cedula", empleado.Cedula);
            cmd.Parameters.AddWithValue("@nombre", empleado.NombreCompleto);
            cmd.Parameters.AddWithValue("@fechaNac", empleado.FechaNacimiento);
            cmd.Parameters.AddWithValue("@direccion", (object?)empleado.Direccion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@telefono", (object?)empleado.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@telEmergencia", (object?)empleado.TelefonoEmergencia ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rolId", empleado.RolId <= 0 ? 1 : empleado.RolId);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool InsertarHuella(int empleadoId, int dedo, byte[] template)
        {
            using var conn = GetConnection();
            conn.Open();

            string query = @"
                INSERT INTO huellas (empleado_id, dedo, template)
                VALUES (@empleadoId, @dedo, @template)";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@empleadoId", empleadoId);
            cmd.Parameters.AddWithValue("@dedo", dedo);
            cmd.Parameters.AddWithValue("@template", template);

            return cmd.ExecuteNonQuery() > 0;
        }

        #endregion

        #region --- Administración / Configuración ---

        public bool CambiarEstadoEmpleado(int id, bool activo)
        {
            using var conn = GetConnection();
            conn.Open();

            string query = "UPDATE empleados SET activo = @activo WHERE id = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@activo", activo ? 1 : 0);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool GuardarConfiguracion(string adminPassword, TimeSpan horaEntrada, TimeSpan horaSalida)
        {
            using var conn = GetConnection();
            conn.Open();

            string query = @"
                INSERT INTO configuracion (id, admin_password, hora_entrada, hora_salida)
                VALUES (1, @adminPassword, @horaEntrada, @horaSalida)
                ON DUPLICATE KEY UPDATE
                    admin_password = @adminPassword,
                    hora_entrada = @horaEntrada,
                    hora_salida = @horaSalida;";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@adminPassword", adminPassword);
            cmd.Parameters.AddWithValue("@horaEntrada", horaEntrada);
            cmd.Parameters.AddWithValue("@horaSalida", horaSalida);

            return cmd.ExecuteNonQuery() > 0;
        }

        #endregion

        public (string AdminPassword, TimeSpan HoraEntrada, TimeSpan HoraSalida)? ObtenerConfiguracion()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = "SELECT admin_password, hora_entrada, hora_salida FROM configuracion LIMIT 1;";
            using var command = new MySqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                string password = reader.GetString("admin_password");
                TimeSpan entrada = reader.GetTimeSpan("hora_entrada");
                TimeSpan salida = reader.GetTimeSpan("hora_salida");

                return (password, entrada, salida);
            }

            return null;
        }
    }
}
