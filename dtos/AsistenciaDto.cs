using System;

namespace ControlAcceso.DTOs
{
    public class AsistenciaDto
    {
        public int Id { get; set; }
        public int EmpleadoID { get; set; }
        public DateTime Timestamp { get; set; }
        public int Tipo { get; set; } // 1 = Entrada, 0 = Salida
        public bool PorAdministrador { get; set; }
        public string? Observacion { get; set; }
    }
}
