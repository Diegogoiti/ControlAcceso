using System.Threading;
using System.Threading.Tasks;

namespace ControlAcceso.Hardware;

public interface ICaptahuellasService
{
    Task<byte[]?> CapturarHuellaAsync(CancellationToken cancellationToken = default);
}
