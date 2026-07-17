-- 1. Crear y seleccionar el esquema
CREATE SCHEMA IF NOT EXISTS acceso_db;
USE acceso_db;

CREATE TABLE Configuracion (
    id INT PRIMARY KEY DEFAULT 1,
    AdminPasswordBase64 VARCHAR(255) NOT NULL,
    HoraEntrada TIME NOT NULL,
    HoraSalida TIME NOT NULL,
    CONSTRAINT unica_fila CHECK (id = 1) -- Evita que se inserte más de un registro
);