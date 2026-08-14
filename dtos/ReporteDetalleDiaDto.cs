using System;

namespace ControlAcceso.DTOs
{
    /// <summary>
    /// Un día dentro del reporte detallado por empleado.
    /// Estado: "A tiempo" | "Retardo" | "Retardo justificado" | "Falta".
    /// Los registros manuales del administrador con motivo son "Retardo justificado".
    /// </summary>
    public class ReporteDetalleDiaDto
    {
        public DateTime Fecha { get; set; }
        public string Dia { get; set; } = string.Empty;
        public TimeSpan? HoraEntrada { get; set; }

        /// <summary>
        /// Hora de entrada en formato AM/PM ("08:00 AM", "06:18 PM").
        /// TimeSpan no soporta el designador "tt", por eso se convierte a DateTime.
        /// </summary>
        public string HoraEntradaTexto
        {
            get
            {
                if (!HoraEntrada.HasValue) return "—";
                return DateTime.Today.Add(HoraEntrada.Value).ToString("hh:mm tt");
            }
        }

        public string Estado { get; set; } = string.Empty;
        public int MinutosRetraso { get; set; }

        /// <summary>
        /// Texto de la duración del retardo con formato mixto: "1 h 15 min" cuando
        /// hay horas, "15 min" cuando no, "—" si no hubo retardo.
        /// </summary>
        public string MinutosTexto
        {
            get
            {
                if (MinutosRetraso <= 0) return "—";

                int horas = MinutosRetraso / 60;
                int minutos = MinutosRetraso % 60;

                if (horas > 0 && minutos > 0) return $"{horas} h {minutos} min";
                if (horas > 0) return $"{horas} h";
                return $"{minutos} min";
            }
        }

        public string Observacion { get; set; } = string.Empty;
    }
}
