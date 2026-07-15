using System.Threading.Tasks;

namespace ControlAcceso.Services;

public static class HardwareService
{
    public static async Task<byte[]?> CapturarHuellaAsync(CancellationToken token)
    {
        return await Task.Run(() => {
            while (!token.IsCancellationRequested) { // Se detiene si cancelas
                var data = Scanner.GetRawImage();
                if (EsHuellaValida(data)) return data!;
                System.Threading.Thread.Sleep(300);
            }
            return null; // Retorna null si se canceló
        }, token);
    }

    public static bool EsHuellaValida(byte[]? data)
    {
        if (data == null || data.Length == 0) return false;
        long suma = 0;
        for (int i = 0; i < data.Length; i += 100) suma += data[i];
        long maxSuma = 255 * (data.Length / 100);
        return suma > 1000 && suma < (maxSuma - 1000);
    }
}
