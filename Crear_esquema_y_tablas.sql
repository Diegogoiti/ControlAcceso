-- 1. Crear la base de datos si no existe y seleccionarla
CREATE DATABASE IF NOT EXISTS acceso_db 
CHARACTER SET utf8
;

USE acceso_db;

-- 2. Eliminar tablas previas si existen para un despliegue limpio
DROP TABLE IF EXISTS asistencia;
DROP TABLE IF EXISTS empleados;
DROP TABLE IF EXISTS configuracion;

-- 3. Tabla: empleados
CREATE TABLE empleados (
    ID INT(11) NOT NULL AUTO_INCREMENT,
    Nombre VARCHAR(100) NOT NULL,
    Cedula INT(11) NOT NULL,
    HuellaTemplate LONGBLOB DEFAULT NULL,
    Activo TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (ID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4. Tabla: asistencia
CREATE TABLE asistencia (
    ID INT(11) NOT NULL AUTO_INCREMENT,
    EmpleadoID INT(11) NOT NULL,
    Timestamp DATETIME NOT NULL,
    Tipo TINYINT(4) NOT NULL, -- 1 = Entrada, 0 = Salida
    PRIMARY KEY (ID),
    CONSTRAINT fk_asistencia_empleado 
        FOREIGN KEY (EmpleadoID) REFERENCES empleados(ID) 
        ON DELETE CASCADE 
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5. Tabla: configuracion
CREATE TABLE configuracion (
    id INT(11) NOT NULL,
    AdminPasword VARCHAR(255) DEFAULT NULL,
    HoraEntrada TIME NOT NULL,
    HoraSalida TIME NOT NULL,
    PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 6. Insertar valores iniciales para la configuración (Password cifrado 'admin' en Base64, Entrada 08:00, Salida 17:00)
INSERT INTO configuracion (id, AdminPasword, HoraEntrada, HoraSalida) 
VALUES (1, 'YWRtaW4=', '08:00:00', '17:00:00');