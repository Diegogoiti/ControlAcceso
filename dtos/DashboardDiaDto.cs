using System;
using System.Collections.Generic;

namespace ControlAcceso.DTOs
{
    /// <summary>
    /// Datos completos del dashboard de un día: estadísticas resumidas,
    /// los marcajes del día (ordenados por hora) y la lista de empleados
    /// activos que aún no han marcado.
    /// </summary>
    public class DashboardDiaDto
    {
        public DateTime Fecha { get; set; }
        public int EmpleadosActivos { get; set; }
        public int MarcajesHoy { get; set; }
        public int TardanzasHoy { get; set; }
        public int PorAdminHoy { get; set; }
        public List<AsistenciaDiaDto> Marcajes { get; set; } = new();
        public List<EmpleadoPendienteDto> SinMarcar { get; set; } = new();
    }
}
