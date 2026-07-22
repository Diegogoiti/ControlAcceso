using System;

namespace ControlAcceso.DTOs
{
    public class AsistenciaFilter
    {
        public int? EmpleadoId { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? Tipo { get; set; } // 1 = Entrada, 0 = Salida
    }
}
