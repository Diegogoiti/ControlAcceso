-- 1. Crear la base de datos si no existe y seleccionarla
CREATE DATABASE IF NOT EXISTS acceso_db
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE acceso_db;

-- 2. Eliminar tablas previas en el orden correcto de dependencias
DROP TABLE IF EXISTS asistencia;
DROP TABLE IF EXISTS huellas;
DROP TABLE IF EXISTS empleados;
DROP TABLE IF EXISTS roles;
DROP TABLE IF EXISTS admin_huellas;
DROP TABLE IF EXISTS configuracion;

-- 3. Tabla: roles
CREATE TABLE roles (
    id INT NOT NULL AUTO_INCREMENT,
    nombre_rol VARCHAR(50) NOT NULL,
    activo TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4. Tabla: empleados
CREATE TABLE empleados (
    id INT NOT NULL AUTO_INCREMENT,
    cedula INT NOT NULL UNIQUE,
    nombre_completo VARCHAR(120) NOT NULL,
    fecha_nacimiento DATE NOT NULL,
    direccion VARCHAR(255) NULL,
    telefono VARCHAR(20) NULL,
    telefono_emergencia VARCHAR(20) NULL,
    rol_id INT NOT NULL,
    fecha_ingreso DATE NOT NULL,
    activo TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (id),
    CONSTRAINT fk_empleados_roles
        FOREIGN KEY (rol_id) REFERENCES roles(id)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5. Tabla: huellas (permite múltiples huellas por empleado)
CREATE TABLE huellas (
    id INT NOT NULL AUTO_INCREMENT,
    empleado_id INT NOT NULL,
    dedo INT NOT NULL, -- Número identificador del dedo (ej. 1 al 10)
    template LONGBLOB NOT NULL,
    activo TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (id),
    CONSTRAINT fk_huellas_empleados
        FOREIGN KEY (empleado_id) REFERENCES empleados(id)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 6. Tabla: asistencia
CREATE TABLE asistencia (
    id INT NOT NULL AUTO_INCREMENT,
    empleado_id INT NOT NULL,
    fecha DATE NOT NULL DEFAULT (CURRENT_DATE),
    hora TIME NOT NULL DEFAULT (CURRENT_TIME),
    por_administrador TINYINT(1) NOT NULL DEFAULT 0, -- 1 si fue asignado por permiso/admin, 0 si fue por huella
    observacion VARCHAR(255) NULL,
    activo TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (id),
    CONSTRAINT fk_asistencia_empleados
        FOREIGN KEY (empleado_id) REFERENCES empleados(id)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 7. Tabla: configuracion (Tabla de 1 sola fila para credenciales y parámetros generales)
CREATE TABLE configuracion (
    id INT NOT NULL AUTO_INCREMENT,
    admin_password VARCHAR(255) NOT NULL,
    hora_entrada TIME NOT NULL,
    hora_limite TIME NOT NULL,
    activo TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 8. Tabla: admin_huellas (3 huellas máximo para el administrador)
CREATE TABLE admin_huellas (
    id INT NOT NULL AUTO_INCREMENT,
    configuracion_id INT NOT NULL,
    dedo INT NOT NULL,
    template LONGBLOB NOT NULL,
    activo TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (id),
    UNIQUE (configuracion_id, dedo),
    CONSTRAINT fk_admin_huellas_configuracion
        FOREIGN KEY (configuracion_id) REFERENCES configuracion(id)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 9. Datos iniciales mínimos requeridos
-- Insertar un rol básico por defecto
INSERT INTO roles (nombre_rol, activo) VALUES ('General', 1);

-- Insertar fila única de configuración
INSERT INTO configuracion (id, admin_password, hora_entrada, hora_limite, activo)
VALUES (1, 'YWRtaW4=', '08:00:00', '08:30:00', 1);
