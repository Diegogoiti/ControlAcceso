using System;
using System.Collections.Generic;
using System.Text;
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
                    Direccion = reader.GetString("direccion"),
                    Telefono = reader.GetString("telefono"),
                    TelefonoEmergencia = reader.GetString("telefono_emergencia"),
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

        public List<HuellaEmpleadoDto> ObtenerHuellasAdmin()
        {
            var huellas = new List<HuellaEmpleadoDto>();

            using var conn = GetConnection();
            conn.Open();

            string query = @"
                SELECT id, configuracion_id, dedo, template
                FROM admin_huellas
                WHERE activo = 1
                ORDER BY dedo ASC";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                huellas.Add(new HuellaEmpleadoDto
                {
                    Id = reader.GetInt32("id"),
                    EmpleadoId = reader.GetInt32("configuracion_id"),
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

            // Transacción para garantizar consistencia atómica
            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Insertar empleado y recuperar el ID autogenerado
                string queryEmpleado = @"
            INSERT INTO empleados (cedula, nombre_completo, fecha_nacimiento, direccion, telefono, telefono_emergencia, rol_id, fecha_ingreso, activo)
            VALUES (@cedula, @nombre, @fechaNac, @direccion, @telefono, @telEmergencia, @rolId, CURDATE(), 1);
            SELECT LAST_INSERT_ID();";

                using var cmdEmp = new MySqlCommand(queryEmpleado, conn, transaction);

                // Conversión limpia de la cédula a int para hacer match con el schema INT de MySQL
                int.TryParse(empleado.Cedula, out int cedulaNum);

                cmdEmp.Parameters.AddWithValue("@cedula", cedulaNum);
                cmdEmp.Parameters.AddWithValue("@nombre", empleado.NombreCompleto);

                // Formato adecuado para DATE en MySQL (yyyy-MM-dd)
                cmdEmp.Parameters.AddWithValue("@fechaNac", empleado.FechaNacimiento.ToString("yyyy-MM-dd"));

                cmdEmp.Parameters.AddWithValue("@direccion", (object?)empleado.Direccion ?? DBNull.Value);
                cmdEmp.Parameters.AddWithValue("@telefono", (object?)empleado.Telefono ?? DBNull.Value);
                cmdEmp.Parameters.AddWithValue("@telEmergencia", (object?)empleado.TelefonoEmergencia ?? DBNull.Value);

                // Asegurar que el rol sea al menos 1 (rol por defecto 'General')
                cmdEmp.Parameters.AddWithValue("@rolId", empleado.RolId <= 0 ? 1 : empleado.RolId);

                object result = cmdEmp.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    transaction.Rollback();
                    return false;
                }

                int empleadoId = Convert.ToInt32(result);

                // 2. Insertar huellas utilizando el nombre correcto del campo de la BD ('template')
                string queryHuella = @"
            INSERT INTO huellas (empleado_id, dedo, template)
            VALUES (@empleadoId, @dedo, @template);";

                using var cmdHuella = new MySqlCommand(queryHuella, conn, transaction);
                cmdHuella.Parameters.Add("@empleadoId", MySqlDbType.Int32);
                cmdHuella.Parameters.Add("@dedo", MySqlDbType.Int32);
                cmdHuella.Parameters.Add("@template", MySqlDbType.LongBlob);

                foreach (var huella in empleado.Huellas)
                {
                    if (huella == null || huella.TemplateHuella == null || huella.TemplateHuella.Length == 0)
                        continue;

                    cmdHuella.Parameters["@empleadoId"].Value = empleadoId;
                    cmdHuella.Parameters["@dedo"].Value = huella.Dedo;
                    cmdHuella.Parameters["@template"].Value = huella.TemplateHuella;

                    cmdHuella.ExecuteNonQuery();
                }

                // Confirmar la transacción
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

       public bool ActualizarEmpleado(EmpleadoSaveDto empleado)
{
    using var conn = GetConnection();
    conn.Open();

    using var transaction = conn.BeginTransaction();

    try
    {
        // 1. Actualizar los datos básicos del empleado
        string queryEmpleado = @"
            UPDATE empleados
            SET cedula = @cedula,
                nombre_completo = @nombre,
                fecha_nacimiento = @fechaNac,
                direccion = @direccion,
                telefono = @telefono,
                telefono_emergencia = @telEmergencia,
                rol_id = @rolId
            WHERE id = @id;";

        using var cmdEmp = new MySqlCommand(queryEmpleado, conn, transaction);

        int.TryParse(empleado.Cedula, out int cedulaNum);

        cmdEmp.Parameters.AddWithValue("@id", empleado.Id);
        cmdEmp.Parameters.AddWithValue("@cedula", cedulaNum);
        cmdEmp.Parameters.AddWithValue("@nombre", empleado.NombreCompleto);
        cmdEmp.Parameters.AddWithValue("@fechaNac", empleado.FechaNacimiento.ToString("yyyy-MM-dd"));
        cmdEmp.Parameters.AddWithValue("@direccion", (object?)empleado.Direccion ?? DBNull.Value);
        cmdEmp.Parameters.AddWithValue("@telefono", (object?)empleado.Telefono ?? DBNull.Value);
        cmdEmp.Parameters.AddWithValue("@telEmergencia", (object?)empleado.TelefonoEmergencia ?? DBNull.Value);
        cmdEmp.Parameters.AddWithValue("@rolId", empleado.RolId <= 0 ? 1 : empleado.RolId);

        cmdEmp.ExecuteNonQuery();

        // 2. Insertar o actualizar huellas re-capturadas (si las hay)
        if (empleado.Huellas != null)
        {
            foreach (var huella in empleado.Huellas)
            {
                if (huella == null || huella.TemplateHuella == null || huella.TemplateHuella.Length == 0)
                    continue;

                string queryExisteHuella = @"
                    SELECT id
                    FROM huellas
                    WHERE empleado_id = @empleadoId AND dedo = @dedo
                    LIMIT 1;";

                using var cmdExisteHuella = new MySqlCommand(queryExisteHuella, conn, transaction);
                cmdExisteHuella.Parameters.AddWithValue("@empleadoId", empleado.Id);
                cmdExisteHuella.Parameters.AddWithValue("@dedo", huella.Dedo);

                object? huellaIdResult = cmdExisteHuella.ExecuteScalar();

                if (huellaIdResult != null && huellaIdResult != DBNull.Value)
                {
                    int huellaId = Convert.ToInt32(huellaIdResult);
                    string queryActualizarHuella = @"
                        UPDATE huellas
                        SET template = @template
                        WHERE id = @id;";

                    using var cmdActualizarHuella = new MySqlCommand(queryActualizarHuella, conn, transaction);
                    cmdActualizarHuella.Parameters.AddWithValue("@template", huella.TemplateHuella);
                    cmdActualizarHuella.Parameters.AddWithValue("@id", huellaId);
                    cmdActualizarHuella.ExecuteNonQuery();
                }
                else
                {
                    string queryInsertarHuella = @"
                        INSERT INTO huellas (empleado_id, dedo, template)
                        VALUES (@empleadoId, @dedo, @template);";

                    using var cmdInsertarHuella = new MySqlCommand(queryInsertarHuella, conn, transaction);
                    cmdInsertarHuella.Parameters.AddWithValue("@empleadoId", empleado.Id);
                    cmdInsertarHuella.Parameters.AddWithValue("@dedo", huella.Dedo);
                    cmdInsertarHuella.Parameters.AddWithValue("@template", huella.TemplateHuella);
                    cmdInsertarHuella.ExecuteNonQuery();
                }
            }
        }

        transaction.Commit();
        return true;
    }
    catch
    {
        transaction.Rollback();
        return false;
    }
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

        public bool GuardarConfiguracion(string adminPassword, TimeSpan horaEntrada, TimeSpan horaLimite, IReadOnlyList<HuellaEmpleadoDto> huellasAdmin)
        {
            using var conn = GetConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                string passwordGuardado = Convert.ToBase64String(Encoding.UTF8.GetBytes(adminPassword));

                string query = @"
                    INSERT INTO configuracion (id, admin_password, hora_entrada, hora_limite)
                    VALUES (1, @adminPassword, @horaEntrada, @horaLimite)
                    ON DUPLICATE KEY UPDATE
                        admin_password = @adminPassword,
                        hora_entrada = @horaEntrada,
                        hora_limite = @horaLimite;";

                using var cmd = new MySqlCommand(query, conn, transaction);
                cmd.Parameters.AddWithValue("@adminPassword", passwordGuardado);
                cmd.Parameters.AddWithValue("@horaEntrada", horaEntrada);
                cmd.Parameters.AddWithValue("@horaLimite", horaLimite);
                cmd.ExecuteNonQuery();

                try
                {
                    string createTable = @"
                        CREATE TABLE IF NOT EXISTS admin_huellas (
                            id INT NOT NULL AUTO_INCREMENT,
                            configuracion_id INT NOT NULL,
                            dedo INT NOT NULL,
                            template LONGBLOB NOT NULL,
                            activo TINYINT(1) NOT NULL DEFAULT 1,
                            PRIMARY KEY (id),
                            UNIQUE (configuracion_id, dedo)
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

                    using var createCmd = new MySqlCommand(createTable, conn, transaction);
                    createCmd.ExecuteNonQuery();

                    string deleteHuellas = "DELETE FROM admin_huellas WHERE configuracion_id = 1;";
                    using var deleteCmd = new MySqlCommand(deleteHuellas, conn, transaction);
                    deleteCmd.ExecuteNonQuery();

                    if (huellasAdmin != null)
                    {
                        string insertHuella = @"
                            INSERT INTO admin_huellas (configuracion_id, dedo, template)
                            VALUES (@configuracionId, @dedo, @template);";

                        using var cmdHuella = new MySqlCommand(insertHuella, conn, transaction);
                        cmdHuella.Parameters.Add("@configuracionId", MySqlDbType.Int32);
                        cmdHuella.Parameters.Add("@dedo", MySqlDbType.Int32);
                        cmdHuella.Parameters.Add("@template", MySqlDbType.LongBlob);

                        foreach (var huella in huellasAdmin)
                        {
                            if (huella == null || huella.TemplateHuella == null || huella.TemplateHuella.Length == 0)
                                continue;

                            cmdHuella.Parameters["@configuracionId"].Value = 1;
                            cmdHuella.Parameters["@dedo"].Value = huella.Dedo;
                            cmdHuella.Parameters["@template"].Value = huella.TemplateHuella;
                            cmdHuella.ExecuteNonQuery();
                        }
                    }
                }
                catch
                {
                    // Si la tabla no está disponible, se ignora para no bloquear la configuración base.
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        #endregion

        public (string AdminPassword, TimeSpan HoraEntrada, TimeSpan HoraLimite)? ObtenerConfiguracion()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = "SELECT admin_password, hora_entrada, hora_limite FROM configuracion LIMIT 1;";
            using var command = new MySqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                string passwordStored = reader.GetString("admin_password");
                TimeSpan entrada = reader.GetTimeSpan("hora_entrada");
                TimeSpan limite = reader.GetTimeSpan("hora_limite");

                string password = passwordStored;
                try
                {
                    password = Encoding.UTF8.GetString(Convert.FromBase64String(passwordStored));
                }
                catch
                {
                    password = passwordStored;
                }

                return (password, entrada, limite);
            }

            return null;
        }

        public EmpleadoDto? ObtenerEmpleadoPorId(int id)
{
    using var conn = GetConnection();
    conn.Open();

    string query = @"
        SELECT id, cedula, nombre_completo, fecha_nacimiento, direccion,
               telefono, telefono_emergencia, rol_id, fecha_ingreso, activo
        FROM empleados
        WHERE id = @id LIMIT 1";

    using var cmd = new MySqlCommand(query, conn);
    cmd.Parameters.AddWithValue("@id", id);

    using var reader = cmd.ExecuteReader();
    if (reader.Read())
    {
        return new EmpleadoDto
        {
            Id = reader.GetInt32("id"),
            Cedula = reader.GetInt32("cedula"),
            NombreCompleto = reader.GetString("nombre_completo"),
            FechaNacimiento = DateOnly.FromDateTime(reader.GetDateTime("fecha_nacimiento")),
            Direccion = reader.IsDBNull(reader.GetOrdinal("direccion")) ? string.Empty : reader.GetString("direccion"),
            Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? string.Empty : reader.GetString("telefono"),
            TelefonoEmergencia = reader.IsDBNull(reader.GetOrdinal("telefono_emergencia")) ? string.Empty : reader.GetString("telefono_emergencia"),
            RolId = reader.GetInt32("rol_id"),
            FechaIngreso = DateOnly.FromDateTime(reader.GetDateTime("fecha_ingreso")),
            Activo = reader.GetBoolean("activo")
        };
    }

    return null;
}
    }
}
