using System;
using System.Collections.Generic;
using ControlAcceso.DTOs;

namespace ControlAcceso.Database
{
    public interface IDatabase
    {
        // Métodos que faltaban en IDatabase:
        bool AgregarEmpleado(EmpleadoSaveDto empleado);
        bool ActualizarEmpleado(EmpleadoSaveDto empleado);
        bool CambiarEstadoEmpleado(int id, bool activo);

        // Corrige la firma de RegistrarAsistencia para incluir tipoAsistencia si aplica, o sus parámetros requeridos:
        bool RegistrarAsistencia(AsistenciaDto asistencia);

        bool GuardarConfiguracion(string adminPassword, TimeSpan horaEntrada, TimeSpan horaLimite, IReadOnlyList<HuellaEmpleadoDto> huellasAdmin);

        // Asegúrate de que los métodos de consulta existentes usen las firmas correctas:
        List<EmpleadoDto> ObtenerEmpleados(EmpleadoFilter filtro);
        List<AsistenciaDto> ObtenerAsistencias(AsistenciaFilter filtro);
        List<HuellaEmpleadoDto> ObtenerHuellasActivas();
        (string AdminPassword, TimeSpan HoraEntrada, TimeSpan HoraLimite)? ObtenerConfiguracion();
        EmpleadoDto? ObtenerEmpleadoPorId(int id);
    }
}
