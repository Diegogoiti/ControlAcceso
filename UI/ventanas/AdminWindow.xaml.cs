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
        private int? _cargoEditandoId = null;

        public AdminWindow(UI.controladores.AdminController controller)
        {
            InitializeComponent();
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            InicializarCombosHora();
        }

        private void InicializarCombosHora()
        {
            for (int hora = 0; hora <= 23; hora++)
            {
                cmbHoraEntrada.Items.Add(hora.ToString("D2"));
                cmbHoraLimite.Items.Add(hora.ToString("D2"));
            }

            for (int minuto = 0; minuto <= 59; minuto++)
            {
                cmbMinutoEntrada.Items.Add(minuto.ToString("D2"));
                cmbMinutoLimite.Items.Add(minuto.ToString("D2"));
            }

            cmbHoraEntrada.SelectedIndex = 8;
            cmbMinutoEntrada.SelectedIndex = 0;
            cmbHoraLimite.SelectedIndex = 8;
            cmbMinutoLimite.SelectedIndex = 30;
        }

        #region Navegación entre Pestañas (CORREGIDO)

        private void btnTabEmpleados_Click(object sender, RoutedEventArgs e)
        {
            // Ocultar todos los paneles
            PanelEmpleados.Visibility = Visibility.Collapsed;
            PanelCargos.Visibility = Visibility.Collapsed;
            PanelConfiguracion.Visibility = Visibility.Collapsed;

            // Mostrar solo el de empleados
            PanelEmpleados.Visibility = Visibility.Visible;
            _controller.CargarListaEmpleados();
        }

        private void btnTabCargos_Click(object sender, RoutedEventArgs e)
        {
            // Ocultar todos los paneles
            PanelEmpleados.Visibility = Visibility.Collapsed;
            PanelCargos.Visibility = Visibility.Collapsed;
            PanelConfiguracion.Visibility = Visibility.Collapsed;

            // Mostrar solo el de cargos
            PanelCargos.Visibility = Visibility.Visible;
            _controller.CargarListaCargos();
        }

        private void btnTabConfig_Click(object sender, RoutedEventArgs e)
        {
            // Ocultar todos los paneles
            PanelEmpleados.Visibility = Visibility.Collapsed;
            PanelCargos.Visibility = Visibility.Collapsed;
            PanelConfiguracion.Visibility = Visibility.Collapsed;

            // Mostrar solo el de configuración
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
                RolId = cmbRol.SelectedValue is int id ? id : 1,
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
                RolId = cmbEditRol.SelectedValue is int id ? id : 1,
            };
        }

        #endregion

        #region Eventos de Botones

        private void btnNuevoEmpleado_Click(object sender, RoutedEventArgs e)
        {
            MostrarModalRegistro();
        }

        private void btnGuardarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            _controller.ProcesarGuardadoEmpleado();
        }

        private void btnCancelarRegistro_Click(object sender, RoutedEventArgs e)
        {
            OcultarModalRegistro();
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

        private async void btnVerificarHuella_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int numeroDedo))
            {
                await _controller.VerificarHuellaDedoAsync(numeroDedo);
            }
        }

        private async void btnVerificarEditHuella_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int numeroDedo))
            {
                await _controller.VerificarHuellaDedoAsync(numeroDedo);
            }
        }

        private async void btnVerificarAdminHuella_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int numeroDedo))
            {
                await _controller.VerificarHuellaAdminAsync(numeroDedo);
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

        public void MostrarModalRegistro()
        {
            EmpleadoEditandoId = null;
            _controller.LimpiarHuellasEnMemoria();

            LimpiarFormularioRegistro();
            CargarRolesRegistro();
            OverlayEditarEmpleado.Visibility = Visibility.Collapsed;
            OverlayRegistrarEmpleado.Visibility = Visibility.Visible;
        }

        public void OcultarModalRegistro()
        {
            _controller.LimpiarHuellasEnMemoria();
            OverlayRegistrarEmpleado.Visibility = Visibility.Collapsed;
        }

        private void LimpiarFormularioRegistro()
        {
            txtCedula.Text = string.Empty;
            txtNombreCompleto.Text = string.Empty;
            dpFechaNacimiento.SelectedDate = null;
            txtTelefono.Text = string.Empty;
            txtTelefonoEmergencia.Text = string.Empty;
            txtDireccion.Text = string.Empty;
            cmbRol.SelectedIndex = 0;

            for (int dedo = 1; dedo <= 3; dedo++)
            {
                ActualizarEstadoHuella(dedo, registrada: false);
            }
        }

        public void MostrarModalEdicion(EmpleadoDto emp)
        {
            EmpleadoEditandoId = emp.Id;
            _controller.LimpiarHuellasEnMemoria();

            CargarDatosFormularioEdicion(emp);
            CargarRolesEdicion(emp.RolId);

            OverlayRegistrarEmpleado.Visibility = Visibility.Collapsed;
            OverlayEditarEmpleado.Visibility = Visibility.Visible;
        }

        public string ObtenerHoraEntrada()
        {
            return $"{cmbHoraEntrada.SelectedItem ?? "08"}:{cmbMinutoEntrada.SelectedItem ?? "00"}";
        }

        public string ObtenerHoraLimite()
        {
            return $"{cmbHoraLimite.SelectedItem ?? "08"}:{cmbMinutoLimite.SelectedItem ?? "30"}";
        }

        public void CargarConfiguracion(string horaEntrada, string horaLimite)
        {
            if (TimeSpan.TryParse(horaEntrada, out TimeSpan entrada))
            {
                cmbHoraEntrada.SelectedItem = entrada.Hours.ToString("D2");
                cmbMinutoEntrada.SelectedItem = entrada.Minutes.ToString("D2");
            }

            if (TimeSpan.TryParse(horaLimite, out TimeSpan limite))
            {
                cmbHoraLimite.SelectedItem = limite.Hours.ToString("D2");
                cmbMinutoLimite.SelectedItem = limite.Minutes.ToString("D2");
            }
        }

        public void MostrarEstadoPassword(bool configurada)
        {
            txtEstadoPassword.Text = configurada ? "✅ Configurada" : "⚠️ No configurada";
            txtEstadoPassword.Foreground = configurada ? new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A)) : new SolidColorBrush(Color.FromRgb(0xD9, 0x77, 0x06));
        }

        private void btnCambiarPassword_Click(object sender, RoutedEventArgs e)
        {
            _controller.ProcesarCambioPassword();
        }

        public void EstablecerEstadoEsperandoHuellaAdmin(int dedo)
        {
            Button? btn = dedo switch { 1 => btnAdminHuella1, 2 => btnAdminHuella2, 3 => btnAdminHuella3, _ => null };
            if (btn != null) btn.Content = $"⏳ Huella {dedo}...";
        }

        /// <summary>
        /// Al abrir la edición, pinta el estado real de las huellas del empleado
        /// y muestra el aviso si no tiene ninguna registrada.
        /// </summary>
        public void ActualizarEstadoHuellasEdicion(Dictionary<int, byte[]> huellas)
        {
            MostrarAvisoHuellasEdicion(huellas.Count > 0);
            for (int dedo = 1; dedo <= 3; dedo++)
            {
                ActualizarEstadoHuellaEdicion(dedo, huellas.ContainsKey(dedo));
            }
        }

        public void MostrarAvisoHuellasEdicion(bool tieneHuellas)
        {
            txtAvisoHuellasEdicion.Visibility = tieneHuellas ? Visibility.Collapsed : Visibility.Visible;
        }

        public void ActualizarEstadoHuellaAdmin(int dedo, bool registrada)
        {
            Button? btn = dedo switch { 1 => btnAdminHuella1, 2 => btnAdminHuella2, 3 => btnAdminHuella3, _ => null };
            if (btn != null)
            {
                btn.Content = registrada ? $"✅ Modificar huella {dedo}" : $"📷 Registrar huella {dedo}";
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

        public EmpleadoViewDto? ObtenerEmpleadoSeleccionado()
        {
            return dgEmpleados.SelectedItem as EmpleadoViewDto;
        }

        private void btnRegistrarAsistencia_Click(object sender, RoutedEventArgs e)
        {
            _controller?.AbrirRegistroAsistenciaEmpleado();
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
            dpEditFechaNacimiento.SelectedDate = emp.FechaNacimiento.ToDateTime(TimeOnly.MinValue);
            txtEditTelefono.Text = emp.Telefono;
            txtEditTelefonoEmergencia.Text = emp.TelefonoEmergencia;
            txtEditDireccion.Text = emp.Direccion;
            cmbEditRol.SelectedIndex = Math.Max(0, emp.RolId - 1);
        }

        private void CargarCombosRoles(bool soloActivos, int? rolSeleccionado = null)
        {
            var roles = _controller.ObtenerCargos(soloActivos);
            cmbRol.ItemsSource = roles;
            if (rolSeleccionado.HasValue)
                cmbRol.SelectedValue = rolSeleccionado.Value;
            else
                cmbRol.SelectedIndex = 0;

            cmbEditRol.ItemsSource = roles;
            if (rolSeleccionado.HasValue)
                cmbEditRol.SelectedValue = rolSeleccionado.Value;
        }

        private void CargarRolesRegistro()
        {
            var roles = _controller.ObtenerCargos(true);
            cmbRol.ItemsSource = roles;
            cmbRol.DisplayMemberPath = "Nombre";
            cmbRol.SelectedValuePath = "Id";
            if (roles.Any())
                cmbRol.SelectedIndex = 0;
        }

        private void CargarRolesEdicion(int rolActualId)
        {
            var roles = _controller.ObtenerCargos(false);
            cmbEditRol.ItemsSource = roles;
            cmbEditRol.DisplayMemberPath = "Nombre";
            cmbEditRol.SelectedValuePath = "Id";
            cmbEditRol.SelectedValue = rolActualId;
        }

        #region --- Pestaña de Cargos (eventos ya integrados) ---

        private void cmbFiltroEstadoCargo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_controller == null) return;
            string filtro = (cmbFiltroEstadoCargo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos";
            _controller.CambiarFiltroCargo(filtro);
        }

        private void btnAgregarCargo_Click(object sender, RoutedEventArgs e)
        {
            _cargoEditandoId = null;
            lblModoCargo.Text = "Nuevo Cargo";
            txtNombreCargo.Text = string.Empty;
            PanelEdicionCargo.Visibility = Visibility.Visible;
            txtNombreCargo.Focus();
        }

        private void btnEditarCargo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int id))
            {
                var cargo = (dgCargos.ItemsSource as IEnumerable<CargoViewDto>)?.FirstOrDefault(c => c.Id == id);
                if (cargo != null)
                {
                    _cargoEditandoId = id;
                    lblModoCargo.Text = "Editar Cargo";
                    txtNombreCargo.Text = cargo.Nombre;
                    PanelEdicionCargo.Visibility = Visibility.Visible;
                    txtNombreCargo.Focus();
                    txtNombreCargo.SelectAll();
                }
            }
        }

        private void btnGuardarCargo_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txtNombreCargo.Text.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            {
                MostrarError("El nombre del cargo no puede estar vacío.");
                return;
            }

            try
            {
                bool exito;
                if (_cargoEditandoId.HasValue)
                    exito = _controller.EditarCargo(_cargoEditandoId.Value, nombre);
                else
                    exito = _controller.AgregarCargo(nombre);

                if (exito)
                {
                    PanelEdicionCargo.Visibility = Visibility.Collapsed;
                    _cargoEditandoId = null;
                    MostrarMensaje("Cargo guardado correctamente.");
                }
                else
                {
                    MostrarError("No se pudo guardar el cargo. Verifique que no exista uno con el mismo nombre.");
                }
            }
            catch (Database.DatabaseException dbEx)
            {
                // Errores de negocio conocidos (ej. "Ya existe un cargo con ese nombre.")
                MostrarError(dbEx.Message);
            }
            catch (Exception ex)
            {
                // Cualquier otro error no debe tumbar la aplicación.
                MostrarError($"Error inesperado al guardar el cargo: {ex.Message}");
            }
        }

        private void btnCancelarEdicionCargo_Click(object sender, RoutedEventArgs e)
        {
            PanelEdicionCargo.Visibility = Visibility.Collapsed;
            _cargoEditandoId = null;
        }

        private void btnCambiarEstadoCargo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int id))
            {
                _controller.CambiarEstadoCargo(id);
            }
        }

        public void MostrarListaCargos(List<CargoViewDto> lista)
        {
            dgCargos.ItemsSource = lista;
        }

        #endregion
    }
}
