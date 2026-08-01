using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ControlAcceso.DTOs;

namespace ControlAcceso
{
    public partial class AdminWindow : Window
    {
        private readonly UI.controladores.AdminController _controller;
        public int? EmpleadoEditandoId { get; private set; }

        public AdminWindow(UI.controladores.AdminController controller)
        {
            InitializeComponent();
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        #region Navegación entre Pestañas

        private void btnTabRegistrar_Click(object sender, RoutedEventArgs e)
        {
            PanelRegistrar.Visibility = Visibility.Visible;
            PanelEmpleados.Visibility = Visibility.Collapsed;
            PanelConfiguracion.Visibility = Visibility.Collapsed;
            _controller.LimpiarHuellasEnMemoria();
        }

        private void btnTabEmpleados_Click(object sender, RoutedEventArgs e)
        {
            PanelRegistrar.Visibility = Visibility.Collapsed;
            PanelEmpleados.Visibility = Visibility.Visible;
            PanelConfiguracion.Visibility = Visibility.Collapsed;
            _controller.CargarListaEmpleados();
        }

        private void btnTabConfig_Click(object sender, RoutedEventArgs e)
        {
            PanelRegistrar.Visibility = Visibility.Collapsed;
            PanelEmpleados.Visibility = Visibility.Collapsed;
            PanelConfiguracion.Visibility = Visibility.Visible;
            _controller.CargarConfiguracion();
        }

        #endregion

        #region Obtención de Datos

        public EmpleadoSaveDto ObtenerDatosFormulario()
        {
            return new EmpleadoSaveDto
            {
                Cedula = txtCedula.Text,
                NombreCompleto = txtNombreCompleto.Text,
                FechaNacimiento = dpFechaNacimiento.SelectedDate.HasValue 
                    ? DateOnly.FromDateTime(dpFechaNacimiento.SelectedDate.Value) 
                    : default,
                Telefono = txtTelefono.Text,
                TelefonoEmergencia = txtTelefonoEmergencia.Text,
                Direccion = txtDireccion.Text,
                RolId = cmbRol.SelectedIndex + 1,
                //FechaIngreso = DateOnly.FromDateTime(DateTime.Today)
            };
        }

        public EmpleadoSaveDto ObtenerDatosEdicionFormulario()
        {
            return new EmpleadoSaveDto
            {
                Id = EmpleadoEditandoId ?? 0,
                Cedula = txtEditCedula.Text,
                NombreCompleto = txtEditNombreCompleto.Text,
                FechaNacimiento = dpEditFechaNacimiento.SelectedDate.HasValue 
                    ? DateOnly.FromDateTime(dpEditFechaNacimiento.SelectedDate.Value) 
                    : default,
                Telefono = txtEditTelefono.Text,
                TelefonoEmergencia = txtEditTelefonoEmergencia.Text,
                Direccion = txtEditDireccion.Text,
                RolId = cmbEditRol.SelectedIndex + 1
            };
        }

        #endregion

        #region Eventos de Botones

        private void btnGuardarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            _controller.ProcesarGuardadoEmpleado();
        }

        private async void btnCapturarDedo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int numeroDedo))
            {
                await _controller.CapturarHuellaDedoAsync(numeroDedo);
            }
        }

        private async void btnCapturarDedoEdicion_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int numeroDedo))
            {
                await _controller.CapturarHuellaDedoAsync(numeroDedo);
            }
        }

        private void btnEditarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            if (dgEmpleados.SelectedItem is EmpleadoViewDto emp)
            {
                _controller.AbrirEdicionEmpleado(emp.Id);
            }
            else
            {
                MostrarError("Por favor, seleccione un empleado de la lista.");
            }
        }

        private void btnGuardarEdicion_Click(object sender, RoutedEventArgs e)
        {
            _controller.ProcesarEdicionEmpleado();
        }

        private void btnGuardarConfig_Click(object sender, RoutedEventArgs e)
        {
            _controller.ProcesarGuardadoConfiguracion();
        }

        private async void btnCapturarAdminHuella_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int numeroDedo))
            {
                await _controller.CapturarHuellaAdminAsync(numeroDedo);
            }
        }

        private void btnCancelarEdicion_Click(object sender, RoutedEventArgs e)
        {
            OcultarModalEdicion();
        }

        private void txtBuscarNombre_TextChanged(object sender, TextChangedEventArgs e)
{
    _controller?.CargarListaEmpleados(
        txtBuscarNombre.Text, 
        (cmbFiltroEstado.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos"
    );
}

        private void cmbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    // Usar _controller? previene la excepción durante el InitializeComponent()
    _controller?.CargarListaEmpleados(
        txtBuscarNombre?.Text ?? string.Empty, 
        (cmbFiltroEstado.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos"
    );
}

        #endregion

        #region Gestión de UI Biométrica y Modal

        public void EstablecerEstadoEsperandoHuella(int dedo)
        {
            Button? btn = dedo switch { 1 => btnHuella1, 2 => btnHuella2, 3 => btnHuella3, _ => null };
            if (btn != null) btn.Content = $"⏳ Coloque el Dedo {dedo}...";
        }

        public void ActualizarEstadoHuella(int dedo, bool registrada)
        {
            Button? btn = dedo switch { 1 => btnHuella1, 2 => btnHuella2, 3 => btnHuella3, _ => null };
            if (btn != null)
            {
                btn.Content = registrada ? $"✅ Dedo {dedo} (Capturado)" : $"👍 Dedo {dedo} (No registrado)";
            }
        }

        public void EstablecerEstadoEsperandoHuellaEdicion(int dedo)
        {
            Button? btn = dedo switch { 1 => btnEditHuella1, 2 => btnEditHuella2, 3 => btnEditHuella3, _ => null };
            if (btn != null) btn.Content = $"⏳ Coloque el Dedo {dedo}...";
        }

        public void ActualizarEstadoHuellaEdicion(int dedo, bool registrada)
        {
            Button? btn = dedo switch { 1 => btnEditHuella1, 2 => btnEditHuella2, 3 => btnEditHuella3, _ => null };
            if (btn != null)
            {
                btn.Content = registrada ? $"✅ Dedo {dedo} (Capturado)" : $"👍 Dedo {dedo} (Sin cambios)";
            }
        }

        public void MostrarModalEdicion(EmpleadoDto emp)
{
    EmpleadoEditandoId = emp.Id;
    _controller.LimpiarHuellasEnMemoria();

    // Llenar los campos de la interfaz
    CargarDatosFormularioEdicion(emp);

    OverlayEditarEmpleado.Visibility = Visibility.Visible;
}

        public string ObtenerPasswordConfiguracion() => pwdAdminPassword.Password;

        public string ObtenerHoraEntrada() => txtHoraEntrada.Text;

        public string ObtenerHoraLimite() => txtHoraLimite.Text;

        public void CargarConfiguracion(string password, string horaEntrada, string horaLimite)
        {
            pwdAdminPassword.Password = password ?? string.Empty;
            txtHoraEntrada.Text = horaEntrada ?? string.Empty;
            txtHoraLimite.Text = horaLimite ?? string.Empty;
        }

        public void EstablecerEstadoEsperandoHuellaAdmin(int dedo)
        {
            Button? btn = dedo switch { 1 => btnAdminHuella1, 2 => btnAdminHuella2, 3 => btnAdminHuella3, _ => null };
            if (btn != null) btn.Content = $"⏳ Huella {dedo}...";
        }

        public void ActualizarEstadoHuellaAdmin(int dedo, bool registrada)
        {
            Button? btn = dedo switch { 1 => btnAdminHuella1, 2 => btnAdminHuella2, 3 => btnAdminHuella3, _ => null };
            if (btn != null)
            {
                btn.Content = registrada ? $"✅ Huella {dedo}" : $"📷 Huella {dedo}";
            }
        }

        public void OcultarModalEdicion()
        {
            EmpleadoEditandoId = null;
            _controller.LimpiarHuellasEnMemoria();
            OverlayEditarEmpleado.Visibility = Visibility.Collapsed;
        }

        public void MostrarListaEmpleados(List<EmpleadoViewDto> lista)
        {
            dgEmpleados.ItemsSource = lista;
        }

        public void MostrarMensaje(string mensaje)
        {
            MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void MostrarError(string mensaje)
        {
            MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion

        private async void btnCambiarEstado_Click(object sender, RoutedEventArgs e)
{
    if (dgEmpleados.SelectedItem is EmpleadoViewDto emp)
    {
        bool exito = await _controller.CambiarEstadoEmpleadoAsync(emp.Id);
        if (!exito)
        {
            MostrarError("No se pudo cambiar el estado del empleado.");
        }
    }
    else
    {
        MostrarError("Por favor, seleccione un empleado de la lista.");
    }
}

public void CargarDatosFormularioEdicion(EmpleadoDto emp)
{
    txtEditCedula.Text = emp.Cedula.ToString();
    txtEditNombreCompleto.Text = emp.NombreCompleto;
    
    // Asignar la fecha de nacimiento convertida de DateOnly a DateTime?
    dpEditFechaNacimiento.SelectedDate = emp.FechaNacimiento.ToDateTime(TimeOnly.MinValue);
    
    txtEditTelefono.Text = emp.Telefono;
    txtEditTelefonoEmergencia.Text = emp.TelefonoEmergencia;
    txtEditDireccion.Text = emp.Direccion;
    
    // Asignar la selección del ComboBox según el RolId (restando 1 por índice base 0)
    cmbEditRol.SelectedIndex = Math.Max(0, emp.RolId - 1);
}
    }
}