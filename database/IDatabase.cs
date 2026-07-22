using System;
using System.Collections.Generic;
using ControlAcceso.DTOs;

namespace ControlAcceso.Database
{
    public interface IDatabase
    {
        // READ con Filtro
        List<EmpleadoDto> ObtenerEmpleados(EmpleadoFilter? filtro = null);

        // CRUD Empleados
        void AgregarEmpleado(EmpleadoDto emp);
        void ActualizarEmpleado(EmpleadoDto emp);
        void CambiarEstadoEmpleado(int id, bool activo);
        void ActualizarHuellaEmpleado(int id, byte[] nuevaHuellaBytes);
        void EliminarEmpleado(int id);

        // Asistencias
        void RegistrarAsistencia(AsistenciaDto asistencia);
        List<AsistenciaDto> ObtenerAsistencias(AsistenciaFilter? filtro = null);
        // Configuración
        (string password, TimeSpan entrada, TimeSpan salida) ObtenerConfiguracion();
        void GuardarConfiguracion(TimeSpan entrada, TimeSpan salida, string passwordPlana);
        List<HuellaEmpleadoDto> ObtenerHuellasActivas();
    }
}
