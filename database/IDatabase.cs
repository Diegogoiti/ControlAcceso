using System;
using System.Collections.Generic;
using ControlAcceso.DTOs;

namespace ControlAcceso.Database
{
    public interface IDatabase
    {
        // --- Empleados ---
        bool AgregarEmpleado(EmpleadoSaveDto empleado);
        bool ActualizarEmpleado(EmpleadoSaveDto empleado);
        bool CambiarEstadoEmpleado(int id, bool activo);
        List<EmpleadoDto> ObtenerEmpleados(EmpleadoFilter filtro);
        EmpleadoDto? ObtenerEmpleadoPorId(int id);

        // --- Asistencia ---
        bool RegistrarAsistencia(AsistenciaDto asistencia);
        List<AsistenciaDto> ObtenerAsistencias(AsistenciaFilter filtro);

        // --- Huellas ---
        List<HuellaEmpleadoDto> ObtenerHuellasActivas();
        List<HuellaEmpleadoDto> ObtenerHuellasAdmin();
        List<HuellaEmpleadoDto> ObtenerHuellasDeEmpleado(int empleadoId);
        bool InsertarHuella(int empleadoId, int dedo, byte[] template); // Opcional, pero puede ser útil

        // --- Configuración ---
        bool GuardarConfiguracion(string adminPassword, TimeSpan horaEntrada, TimeSpan horaLimite, IReadOnlyList<HuellaEmpleadoDto> huellasAdmin);
        (string AdminPassword, TimeSpan HoraEntrada, TimeSpan HoraLimite)? ObtenerConfiguracion();
        bool ActualizarPasswordAdmin(string hashedPassword);

        // --- NUEVOS: Gestión de Roles / Cargos ---
        List<CargoDto> ObtenerCargos(bool soloActivos = false);
        bool CrearCargo(string nombre);
        bool CambiarEstadoCargo(int id, bool activo);
        bool ActualizarCargo(int id, string nuevoNombre);
    }
}
