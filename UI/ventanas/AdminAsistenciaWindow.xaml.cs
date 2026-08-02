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
            TipoAsistencia = rbEntrada.IsChecked == true ? 1 : 0;
            Observacion = txtObservacion.Text?.Trim() ?? string.Empty;
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
