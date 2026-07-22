using System;
using System.Threading;
using System.Threading.Tasks;
using ControlAcceso.Hardware;

namespace ControlAcceso.Services;

public class CaptahuellasService
{
    private readonly ICaptahuellasService _captahuellasAdapter;
    private CancellationTokenSource? _cts;

    // Inyectamos el contrato del hardware por constructor
    public CaptahuellasService(ICaptahuellasService captahuellasAdapter)
    {
        _captahuellasAdapter = captahuellasAdapter;
    }

    /// <summary>
    /// Inicia la captura de la huella llamando al adaptador de hardware.
    /// </summary>
    public async Task<byte[]?> IniciarCapturaAsync()
    {
        // Cancelamos cualquier lectura previa activa por seguridad
        CancelarCaptura();

        _cts = new CancellationTokenSource();

        try
        {
            // Llama a la implementación que encapsula el bucle y la DLL
            return await _captahuellasAdapter.CapturarHuellaAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Se canceló la lectura intencionalmente
            return null;
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }
    }

    /// <summary>
    /// Permite a la UI cancelar la lectura si el usuario cierra la ventana o presiona "Cancelar".
    /// </summary>
    public void CancelarCaptura()
    {
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
    }
}
