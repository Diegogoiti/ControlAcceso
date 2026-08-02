using System;
using System.Collections.Generic;

namespace ControlAcceso.DTOs
{
    public class ReporteEmpleadoDto
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Posicion { get; set; } = string.Empty;
        
        // 0 = Domingo, 1 = Lunes, 2 = Martes, 3 = Miercoles, 4 = Jueves, 5 = Viernes, 6 = Sabado
        // Contendrá 'A' para Asistencia, 'F' para Falta, 'T' para Tardanza.
        public Dictionary<int, string> DiasAsistencia { get; set; } = new Dictionary<int, string>();
        
        public int DiasAsistidos { get; set; }
        public int DiasFaltados { get; set; }
        public int Tardanzas { get; set; }
        public double PorcentajeAsistencia { get; set; }
    }
}
