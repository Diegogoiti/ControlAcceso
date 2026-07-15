-- 1. Crear y seleccionar el esquema
CREATE SCHEMA IF NOT EXISTS acceso_db;
USE acceso_db;

-- 2. Tabla de Empleados (Nombre, Cédula única e índice para búsquedas)
CREATE TABLE Empleados (
    ID INT PRIMARY KEY AUTO_INCREMENT,
    Nombre VARCHAR(100) NOT NULL,
    Cedula INT NOT NULL UNIQUE,
    HuellaTemplate LONGBLOB NOT NULL
);

CREATE INDEX idx_cedula ON Empleados(Cedula);

-- 3. Tabla de Asistencia (Timestamp para delegar la lógica a MySQL)
CREATE TABLE Asistencia (
    ID INT PRIMARY KEY AUTO_INCREMENT,
    EmpleadoID INT NOT NULL,
    Timestamp DATETIME NOT NULL,
    CONSTRAINT fk_empleado 
        FOREIGN KEY (EmpleadoID) REFERENCES Empleados(ID)
        ON DELETE CASCADE
);

-- 4. Índice compuesto para que las consultas por Empleado y Fecha vuelen
CREATE INDEX idx_empleado_timestamp ON Asistencia(EmpleadoID, Timestamp);