using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ControlAcceso.Application;
using ControlAcceso.DTOs;

namespace ControlAcceso.UI.controladores
{
    public class AdminController
    {
        private readonly MyApp _app;
        private AdminWindow? _adminWindow;
        private CancellationTokenSource? _ctsCaptura;
        //private int? _cargoEditandoId = null;

        private readonly Dictionary<int, byte[]> _huellasCapturadas = new();
        private readonly Dictionary<int, byte[]> _huellasAdminCapturadas = new();

        public AdminController(MyApp app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        public void MostrarVentanaAdmin()
        {
            if (_adminWindow == null || !_adminWindow.IsLoaded)
            {
                _adminWindow = new AdminWindow(this);
                _app.CargarEmpleadosViewCache();
                CargarListaEmpleados();
                CargarConfiguracion();
                _adminWindow.ShowDialog();
            }
            else
            {
                _adminWindow.Activate();
            }
        }

        #region Limpieza y Gestión de Memoria
        public void LimpiarHuellasEnMemoria()
        {
            CancelarCaptura();
            _huellasCapturadas.Clear();
        }

        public void LimpiarHuellasAdminEnMemoria()
        {
            CancelarCaptura();
            _huellasAdminCapturadas.Clear();
        }
        #endregion

        #region Métodos de Empleados


        public async Task<bool> CambiarEstadoEmpleadoAsync(int empleadoId)
        {
            var empleado = _app.ObtenerEmpleadoPorId(empleadoId);
            if (empleado == null) return false;

            bool nuevoEstado = !empleado.Activo;
            bool exito = await _app.CambiarEstadoEmpleadoAsync(empleadoId, nuevoEstado);

            if (exito)
            {
                // Re-sincroniza el DataGrid con el estado actualizado
                _app.CargarHuellasActivas();
                CargarListaEmpleados();
            }

            return exito;
        }

        #endregion

        #region Métodos de Configuración

        public void ProcesarGuardadoConfiguracion()
        {
            if (_adminWindow == null) return;

            string horaEntradaTexto = _adminWindow.ObtenerHoraEntrada();
            string horaLimiteTexto = _adminWindow.ObtenerHoraLimite();

            if (!TimeSpan.TryParse(horaEntradaTexto, out TimeSpan horaEntrada))
            {
                _adminWindow.MostrarError("La hora de entrada debe tener formato HH:mm.");
                return;
            }

            if (!TimeSpan.TryParse(horaLimiteTexto, out TimeSpan horaLimite))
            {
                _adminWindow.MostrarError("La hora límite de ingreso debe tener formato HH:mm.");
                return;
            }

            var huellasExistentes = _app.ObtenerHuellasAdmin()
                .Where(h => h.TemplateHuella != null && h.TemplateHuella.Length > 0)
                .ToDictionary(h => h.Dedo, h => h.TemplateHuella);

            var huellasAdmin = new List<HuellaEmpleadoDto>();
            for (int dedo = 1; dedo <= 3; dedo++)
            {
                if (_huellasAdminCapturadas.TryGetValue(dedo, out byte[]? templateNuevo))
                {
                    huellasAdmin.Add(new HuellaEmpleadoDto
                    {
                        Dedo = dedo,
                        TemplateHuella = templateNuevo
                    });
                }
                else if (huellasExistentes.TryGetValue(dedo, out byte[]? templateExistente))
                {
                    huellasAdmin.Add(new HuellaEmpleadoDto
                    {
                        Dedo = dedo,
                        TemplateHuella = templateExistente
                    });
                }
            }

            huellasAdmin = huellasAdmin
                .OrderBy(h => h.Dedo)
                .ToList();

            // La contraseña ya no se edita desde esta ventana; se pasa vacía para
            // que el adaptador conserve el hash existente.
            bool exito = _app.GuardarConfiguracion(string.Empty, horaEntrada, horaLimite, huellasAdmin);
            if (exito)
            {
                _adminWindow.MostrarMensaje("Configuración guardada correctamente.");
                LimpiarHuellasAdminEnMemoria();
            }
            else
            {
                _adminWindow.MostrarError("No se pudo guardar la configuración.");
            }
        }

        public void CargarConfiguracion()
        {
            if (_adminWindow == null) return;

            _huellasAdminCapturadas.Clear();

            var huellasAdmin = _app.ObtenerHuellasAdmin();
            foreach (var huella in huellasAdmin)
            {
                if (huella.TemplateHuella != null && huella.TemplateHuella.Length > 0 && huella.Dedo >= 1 && huella.Dedo <= 3)
                {
                    _huellasAdminCapturadas[huella.Dedo] = huella.TemplateHuella;
                }
            }

            var config = _app.ObtenerConfiguracion();
            if (config.HasValue)
            {
                var (_, horaEntrada, horaLimite) = config.Value;
                _adminWindow.CargarConfiguracion(horaEntrada.ToString("hh\\:mm"), horaLimite.ToString("hh\\:mm"));
            }
            else
            {
                _adminWindow.CargarConfiguracion("08:00", "08:30");
            }

            // La contraseña nunca se muestra; solo su estado (configurada o no).
            ActualizarEstadoPassword();

            for (int dedo = 1; dedo <= 3; dedo++)
            {
                _adminWindow.ActualizarEstadoHuellaAdmin(dedo, _huellasAdminCapturadas.ContainsKey(dedo));
            }
        }

        /// <summary>
        /// Abre la ventana emergente para cambiar la contraseña de administrador.
        /// </summary>
        public void ProcesarCambioPassword()
        {
            if (_adminWindow == null) return;

            var dialog = new CambiarPasswordWindow
            {
                Owner = _adminWindow
            };

            if (dialog.ShowDialog() != true) return;

            var (exito, mensaje) = _app.CambiarPasswordAdmin(dialog.NuevaPassword);
            if (exito)
            {
                ActualizarEstadoPassword();
                _adminWindow.MostrarMensaje(mensaje);
            }
            else
            {
                _adminWindow.MostrarError(mensaje);
            }
        }

        private void ActualizarEstadoPassword()
        {
            if (_adminWindow == null) return;

            var config = _app.ObtenerConfiguracion();
            _adminWindow.MostrarEstadoPassword(config.HasValue && !string.IsNullOrEmpty(config.Value.Password));
        }

        #endregion

        public void ProcesarGuardadoEmpleado()
        {
            if (_adminWindow == null) return;

            EmpleadoSaveDto emp = _adminWindow.ObtenerDatosFormulario();

            if (!EsFormularioValido(emp, requiereHuellas: false))
            {
                return;
            }

            int index = 0;
            foreach (var kvp in _huellasCapturadas)
            {
                emp.Huellas[index] = new HuellaEmpleadoDto
                {
                    Dedo = kvp.Key,
                    TemplateHuella = kvp.Value
                };
                index++;
            }

            try
            {
                _app.GuardarEmpleado(emp);
                _adminWindow.MostrarMensaje("Empleado guardado exitosamente.");

                CargarListaEmpleados();
            }
            catch (Exception ex)
            {
                _adminWindow.MostrarError($"Error al guardar el empleado: {ex.Message}");
            }
        }

        public void ProcesarEdicionEmpleado()
        {
            if (_adminWindow == null || !_adminWindow.EmpleadoEditandoId.HasValue) return; //

            EmpleadoSaveDto emp = _adminWindow.ObtenerDatosEdicionFormulario(); //[cite: 13]

            if (!EsFormularioValido(emp, requiereHuellas: false)) //[cite: 13]
            {
                return;
            }

            try
            {
                emp.Huellas = new HuellaEmpleadoDto[3];

                if (_huellasCapturadas.Count > 0)
                {
                    int index = 0;
                    foreach (var huella in _huellasCapturadas.OrderBy(h => h.Key))
                    {
                        emp.Huellas[index] = new HuellaEmpleadoDto
                        {
                            Dedo = huella.Key,
                            TemplateHuella = huella.Value
                        };
                        index++;
                    }
                }

                // Se llama al nuevo método en MyApp para actualización
                var (exito, mensaje) = _app.EditarEmpleado(emp);

                if (!exito)
                {
                    _adminWindow.MostrarError($"Error al actualizar: {mensaje}");
                    return;
                }

                _adminWindow.MostrarMensaje("Empleado actualizado correctamente."); //[cite: 13]
                _adminWindow.OcultarModalEdicion(); //[cite: 13]
                _app.CargarEmpleadosViewCache();
                _app.CargarHuellasActivas();  //[cite: 13]
                CargarListaEmpleados(); //[cite: 13]
            }
            catch (Exception ex)
            {
                _adminWindow.MostrarError($"Error al actualizar el empleado: {ex.Message}"); //[cite: 13]
            }
        }

        private bool EsFormularioValido(EmpleadoSaveDto emp, bool requiereHuellas = true)
        {
            emp.Cedula = emp.Cedula.Trim();
            emp.Telefono = emp.Telefono.Trim();
            emp.TelefonoEmergencia = emp.TelefonoEmergencia.Trim();

            if (string.IsNullOrWhiteSpace(emp.Cedula) || !emp.Cedula.All(char.IsDigit))
            {
                _adminWindow?.MostrarError("La cédula es obligatoria y debe contener solo números.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(emp.NombreCompleto))
            {
                _adminWindow?.MostrarError("El nombre completo es obligatorio.");
                return false;
            }

            if (emp.FechaNacimiento == default(DateOnly))
            {
                _adminWindow?.MostrarError("Debe seleccionar una fecha de nacimiento.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(emp.Telefono) || !emp.Telefono.All(char.IsDigit))
            {
                _adminWindow?.MostrarError("El teléfono principal es obligatorio y debe tener formato numérico.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(emp.TelefonoEmergencia) || !emp.TelefonoEmergencia.All(char.IsDigit))
            {
                _adminWindow?.MostrarError("El teléfono de emergencia es obligatorio y debe tener formato numérico.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(emp.Direccion))
            {
                _adminWindow?.MostrarError("La dirección de habitación es obligatoria.");
                return false;
            }

            if (requiereHuellas && _huellasCapturadas.Count < 3)
            {
                _adminWindow?.MostrarError("Debe registrar al menos tres huellas dactilares para el empleado.");
                return false;
            }

            return true;
        }

        public async Task CapturarHuellaDedoAsync(int numeroDedo)
        {
            _ctsCaptura?.Cancel();
            _ctsCaptura?.Dispose();

            _ctsCaptura = new CancellationTokenSource();
            var token = _ctsCaptura.Token;

            bool exitoCaptura = false;
            RestaurarEstadoTodosLosBotones(esperandoDedo: numeroDedo);

            try
            {
                if (_adminWindow?.EmpleadoEditandoId.HasValue == true)
                {
                    _adminWindow.EstablecerEstadoEsperandoHuellaEdicion(numeroDedo);
                }
                else
                {
                    _adminWindow?.EstablecerEstadoEsperandoHuella(numeroDedo);
                }

                byte[]? rawImage = await _app.IniciarCapturaAsync(token);

                if (rawImage == null || rawImage.Length == 0)
                {
                    if (token.IsCancellationRequested) return;
                    _adminWindow?.MostrarError("No se logró capturar la imagen del sensor o la operación fue cancelada.");
                    return;
                }

                if (token.IsCancellationRequested) return;

                if (!_app.ProcesarHuellaBruta(rawImage, out byte[]? templateCapturado, out string msgError))
                {
                    if (!token.IsCancellationRequested)
                    {
                        _adminWindow?.MostrarError(msgError);
                    }
                    return;
                }

                if (templateCapturado == null)
                {
                    if (!token.IsCancellationRequested)
                    {
                        _adminWindow?.MostrarError("Ocurrió un error inesperado al procesar el template biométrico.");
                    }
                    return;
                }

                if (!token.IsCancellationRequested)
                {
                    _huellasCapturadas[numeroDedo] = templateCapturado;
                    exitoCaptura = true;
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    _adminWindow?.MostrarError($"Error en el proceso de captura: {ex.Message}");
                }
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    bool estaRegistradoActualmente = exitoCaptura || _huellasCapturadas.ContainsKey(numeroDedo);
                    if (_adminWindow?.EmpleadoEditandoId.HasValue == true)
                    {
                        _adminWindow.ActualizarEstadoHuellaEdicion(numeroDedo, registrada: estaRegistradoActualmente);
                    }
                    else
                    {
                        _adminWindow?.ActualizarEstadoHuella(numeroDedo, registrada: estaRegistradoActualmente);
                    }
                }
            }
        }

        public async Task CapturarHuellaAdminAsync(int numeroDedo)
        {
            _ctsCaptura?.Cancel();
            _ctsCaptura?.Dispose();

            _ctsCaptura = new CancellationTokenSource();
            var token = _ctsCaptura.Token;

            try
            {
                _adminWindow?.EstablecerEstadoEsperandoHuellaAdmin(numeroDedo);

                byte[]? rawImage = await _app.IniciarCapturaAsync(token);

                if (rawImage == null || rawImage.Length == 0)
                {
                    if (!token.IsCancellationRequested)
                    {
                        _adminWindow?.MostrarError("No se logró capturar la imagen del sensor o la operación fue cancelada.");
                    }
                    return;
                }

                if (token.IsCancellationRequested) return;

                if (!_app.ProcesarHuellaBruta(rawImage, out byte[]? templateCapturado, out string msgError))
                {
                    if (!token.IsCancellationRequested)
                    {
                        _adminWindow?.MostrarError(msgError);
                    }
                    return;
                }

                if (templateCapturado == null)
                {
                    if (!token.IsCancellationRequested)
                    {
                        _adminWindow?.MostrarError("Ocurrió un error inesperado al procesar el template biométrico.");
                    }
                    return;
                }

                if (!token.IsCancellationRequested)
                {
                    _huellasAdminCapturadas[numeroDedo] = templateCapturado;
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    _adminWindow?.MostrarError($"Error en la captura de huella de administrador: {ex.Message}");
                }
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    bool registrada = _huellasAdminCapturadas.ContainsKey(numeroDedo);
                    _adminWindow?.ActualizarEstadoHuellaAdmin(numeroDedo, registrada);
                }
            }
        }

        private void RestaurarEstadoTodosLosBotones(int esperandoDedo)
        {
            for (int i = 1; i <= 3; i++)
            {
                if (i != esperandoDedo)
                {
                    bool registrada = _huellasCapturadas.ContainsKey(i);
                    if (_adminWindow?.EmpleadoEditandoId.HasValue == true)
                    {
                        _adminWindow.ActualizarEstadoHuellaEdicion(i, registrada);
                    }
                    else
                    {
                        _adminWindow?.ActualizarEstadoHuella(i, registrada);
                    }
                }
            }
        }

        public void CancelarCaptura()
        {
            if (_ctsCaptura != null && !_ctsCaptura.IsCancellationRequested)
            {
                _ctsCaptura.Cancel();
                _ctsCaptura.Dispose();
                _ctsCaptura = null;
            }
        }

        public void CargarListaEmpleados(string filtroNombre = "", string estado = "Todos")
        {
            if (_adminWindow == null) return;

            var empleados = _app.EmpleadosViewCache.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filtroNombre))
            {
                empleados = empleados.Where(e =>
                    e.Nombre.Contains(filtroNombre, StringComparison.OrdinalIgnoreCase) ||
                    e.Cedula.Contains(filtroNombre));
            }

            if (estado != "Todos" && !string.IsNullOrWhiteSpace(estado))
            {
                empleados = empleados.Where(e => e.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase));
            }

            _adminWindow.MostrarListaEmpleados(empleados.ToList());
        }

        public void AbrirRegistroAsistenciaEmpleado()
        {
            if (_adminWindow == null) return;

            var empleado = _adminWindow.ObtenerEmpleadoSeleccionado();
            if (empleado == null)
            {
                _adminWindow.MostrarError("Por favor, seleccione un empleado de la lista.");
                return;
            }

            var dialog = new AdminAsistenciaWindow(empleado)
            {
                Owner = _adminWindow
            };

            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult != true)
            {
                return;
            }

            try
            {
                bool exito = _app.RegistrarAsistenciaAdministrador(empleado.Id, dialog.TipoAsistencia, dialog.Observacion);
                if (exito)
                {
                    _adminWindow.MostrarMensaje("Asistencia registrada correctamente.");
                    CargarListaEmpleados();
                }
                else
                {
                    _adminWindow.MostrarError("No se pudo registrar la asistencia.");
                }
            }
            catch (Exception ex)
            {
                _adminWindow.MostrarError($"Error al registrar la asistencia: {ex.Message}");
            }
        }

        public void AbrirEdicionEmpleado(int idEmpleado)
        {
            if (_adminWindow == null) return;

            // Obtener los datos completos de la base de datos o servicios
            var empleado = _app.ObtenerEmpleadoPorId(idEmpleado);

            if (empleado == null)
            {
                _adminWindow.MostrarError("No se pudieron obtener los datos del empleado seleccionado.");
                return;
            }

            // Pasar el objeto completo para llenar la modal
            _adminWindow.MostrarModalEdicion(empleado);
        }

        public List<CargoDto> ObtenerCargos(bool soloActivos) => _app.Db.ObtenerCargos(soloActivos);

        // Al inicio de la clase:
        private string _filtroEstadoCargo = "Todos";

        // Nuevos métodos:
        public void CargarListaCargos()
        {
            if (_adminWindow == null) return;
            var cargos = _app.ObtenerTodosLosCargos().Select(c => new CargoViewDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Estado = c.Activo ? "Activo" : "Inactivo"
            }).ToList();

            if (_filtroEstadoCargo == "Activo")
                cargos = cargos.Where(c => c.Estado == "Activo").ToList();
            else if (_filtroEstadoCargo == "Inactivo")
                cargos = cargos.Where(c => c.Estado == "Inactivo").ToList();

            _adminWindow.MostrarListaCargos(cargos);
        }

        public void CambiarFiltroCargo(string filtro)
        {
            _filtroEstadoCargo = filtro;
            CargarListaCargos();
        }

        public bool AgregarCargo(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return false;
            bool exito = _app.CrearCargo(nombre);
            if (exito) CargarListaCargos();
            return exito;
        }

        public bool EditarCargo(int id, string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return false;
            bool exito = _app.ActualizarCargo(id, nombre);
            if (exito) CargarListaCargos();
            return exito;
        }

        public bool CambiarEstadoCargo(int id)
        {
            var cargo = _app.ObtenerTodosLosCargos().FirstOrDefault(c => c.Id == id);
            if (cargo == null) return false;

            // Si el cargo está inactivo, solo lo reactivamos (sin restricciones)
            if (!cargo.Activo)
            {
                bool exito = _app.CambiarEstadoCargo(id, true);
                if (exito) CargarListaCargos();
                return exito;
            }

            // Si el cargo está activo, verificamos si tiene empleados activos
            var empleadosActivos = _app.ObtenerEmpleadosPorRol(id);
            if (empleadosActivos.Count > 0)
            {
                // Construir mensaje con la lista de empleados (máximo 5 nombres para no saturar)
                var nombres = empleadosActivos.Take(5).Select(e => $"• {e.NombreCompleto} (Cédula: {e.Cedula})").ToList();
                string lista = string.Join("\n", nombres);
                if (empleadosActivos.Count > 5)
                    lista += $"\n... y {empleadosActivos.Count - 5} empleados más.";

                _adminWindow?.MostrarError(
                    $"No se puede desactivar el cargo '{cargo.Nombre}' porque tiene {empleadosActivos.Count} empleados activos asignados.\n\n" +
                    $"Empleados afectados:\n{lista}\n\n" +
                    "Primero debe reasignar o desactivar a estos empleados."
                );
                return false;
            }

            // No hay empleados activos → desactivar sin problemas
            bool exitoDesactivar = _app.CambiarEstadoCargo(id, false);
            if (exitoDesactivar) CargarListaCargos();
            return exitoDesactivar;
        }

    }
}
