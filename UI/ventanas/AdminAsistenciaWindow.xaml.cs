using System.Windows;
using ControlAcceso.DTOs;

namespace ControlAcceso
{
    public partial class AdminAsistenciaWindow : Window
    {
        public int TipoAsistencia { get; private set; } = 1;
        public string Observacion { get; private set; } = string.Empty;
        public bool EsRetardoJustificado { get; private set; }

        public AdminAsistenciaWindow(EmpleadoViewDto empleado)
        {
            InitializeComponent();
            txtEmpleadoSeleccionado.Text = $"{empleado.Nombre} ({empleado.Cedula})";
            rdbNormal.IsChecked = true;
        }

        private void rdbTipo_Checked(object sender, RoutedEventArgs e)
        {
            // Solo se pide motivo cuando es retardo justificado: en una asistencia
            // normal el admin no está obligado a escribir nada.
            bool esRJ = rdbRetardoJustificado.IsChecked == true;
            panelObservacion.Visibility = esRJ ? Visibility.Visible : Visibility.Collapsed;
            if (!esRJ)
            {
                txtObservacion.Clear();
            }
        }

        private void btnAceptar_Click(object sender, RoutedEventArgs e)
        {
            Observacion = txtObservacion.Text?.Trim() ?? string.Empty;

            EsRetardoJustificado = rdbRetardoJustificado.IsChecked == true;

            // El motivo es obligatorio solo en el retardo justificado, porque la
            // observación es la justificación del retardo.
            if (EsRetardoJustificado && string.IsNullOrWhiteSpace(Observacion))
            {
                MessageBox.Show(
                    "Debe escribir el motivo del retardo, ya que será la justificación del registro.",
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
