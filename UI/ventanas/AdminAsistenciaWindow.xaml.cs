using System.Windows;
using ControlAcceso.DTOs;

namespace ControlAcceso
{
    public partial class AdminAsistenciaWindow : Window
    {
        public int TipoAsistencia { get; private set; } = 1;
        public string Observacion { get; private set; } = string.Empty;

        public AdminAsistenciaWindow(EmpleadoViewDto empleado)
        {
            InitializeComponent();
            txtEmpleadoSeleccionado.Text = $"{empleado.Nombre} ({empleado.Cedula})";
        }

        private void btnAceptar_Click(object sender, RoutedEventArgs e)
        {
            Observacion = txtObservacion.Text?.Trim() ?? string.Empty;

            // El motivo es obligatorio porque el registro se computa como un
            // retardo justificado y la observación es la justificación.
            if (string.IsNullOrWhiteSpace(Observacion))
            {
                MessageBox.Show(
                    "Debe escribir el motivo de la asistencia, ya que el registro se computará como un retardo justificado.",
                    "Motivo obligatorio",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtObservacion.Focus();
                return;
            }

            TipoAsistencia = 1;
            DialogResult = true;
            Close();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
