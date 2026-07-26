using System;
using System.Threading;
using System.Threading.Tasks;
using ControlAcceso.Hardware;

namespace ControlAcceso.Services;

public class CaptahuellasService
{
    private readonly ICaptahuellasService _captahuellasAdapter;

    public CaptahuellasService(ICaptahuellasService captahuellasAdapter)
    {
        _captahuellasAdapter = captahuellasAdapter ?? throw new ArgumentNullException(nameof(captahuellasAdapter));
    }

    /// <summary>
    /// Inicia la captura de la huella delegando el token recibido directamente al adaptador.
    /// </summary>
    public async Task<byte[]?> IniciarCapturaAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _captahuellasAdapter.CapturarHuellaAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Retorna null de manera limpia si la lectura fue cancelada
            return null;
        }
    }
}
