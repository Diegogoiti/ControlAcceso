using System;
using System.Collections.Generic;
using ControlAcceso.Biometrics;
using ControlAcceso.DTOs;

namespace ControlAcceso.Services
{
    public class BiometricService
    {
        private readonly IBiometricAdapter _biometricAdapter;

        // Inyección del adaptador (Capa 1/Infraestructura)
        public BiometricService(IBiometricAdapter biometricAdapter)
        {
            _biometricAdapter = biometricAdapter ?? throw new ArgumentNullException(nameof(biometricAdapter));
        }

        #region --- Casos de Uso Biométricos ---

        /// <summary>
        /// Caso de Uso: Procesa una imagen en bruto del sensor y genera un template binario seguro.
        /// </summary>
        public bool ProcesarHuellaBruta(byte[] rawImageData, out byte[]? templateGenerado, out string mensajeError)
        {
            templateGenerado = null;
            mensajeError = string.Empty;

            if (rawImageData == null || rawImageData.Length == 0)
            {
                mensajeError = "No se recibieron datos de la imagen de la huella.";
                return false;
            }

            try
            {
                templateGenerado = _biometricAdapter.GenerarTemplateBytes(rawImageData);
                return true;
            }
            catch (Exception ex)
            {
                mensajeError = $"Error al procesar la biometría: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Caso de Uso: Identificación 1:N. Recorre una lista de empleados para saber a quién pertenece la huella capturada.
        /// </summary>
        public int? IdentificarEmpleado(byte[] templateCapturado, List<EmpleadoDto> empleadosRegistrados, double umbral = 50.0)
        {
            if (templateCapturado == null || templateCapturado.Length == 0 || empleadosRegistrados == null)
                return null;

            int? idEmpleadoEncontrado = null;
            double mejorPuntaje = 0.0;

            foreach (var emp in empleadosRegistrados)
            {
                // Ignoramos empleados inactivosa o sin huella registrada
                if (!emp.Activo || emp.HuellaBytes == null || emp.HuellaBytes.Length == 0)
                    continue;

                try
                {
                    double similitud = _biometricAdapter.CalcularSimilitud(templateCapturado, emp.HuellaBytes);

                    if (similitud > mejorPuntaje && similitud >= umbral)
                    {
                        mejorPuntaje = similitud;
                        idEmpleadoEncontrado = emp.Id;
                    }
                }
                catch
                {
                    // Si un template binario corrupto en DB falla, saltamos ese registro sin detener el flujo
                    continue;
                }
            }

            return idEmpleadoEncontrado;
        }

        /// <summary>
        /// Caso de Uso: Verificación 1:1. Compara si la huella dada pertenece a un empleado específico.
        /// </summary>
        public bool VerificarEmpleado(byte[] templateCapturado, byte[] templateAlmacenado, double umbral = 50.0)
        {
            if (templateCapturado == null || templateAlmacenado == null)
                return false;

            try
            {
                return _biometricAdapter.EsCoincidencia(templateCapturado, templateAlmacenado, umbral);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Caso de Uso: Valida si la nueva huella que se va a registrar ya pertenece a otro empleado existente.
        /// </summary>
        public bool ExisteHuellaDuplicada(byte[] nuevoTemplate, List<EmpleadoDto> empleadosExistentes, out string nombreEmpleadoDuplicado)
        {
            nombreEmpleadoDuplicado = string.Empty;

            int? idEncontrado = IdentificarEmpleado(nuevoTemplate, empleadosExistentes);
            if (idEncontrado.HasValue)
            {
                var duplicado = empleadosExistentes.Find(e => e.Id == idEncontrado.Value);
                if (duplicado != null)
                {
                    nombreEmpleadoDuplicado = duplicado.Nombre;
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
