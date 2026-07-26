using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ControlAcceso.Hardware
{
    public class FutronicCaptahuellasAdapter : ICaptahuellasService
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct FTRSCAN_IMAGE_SIZE
        {
            public int nWidth, nHeight, nImageSize;
        }

        [DllImport("ftrScanAPI.dll")] private static extern IntPtr ftrScanOpenDevice();
        [DllImport("ftrScanAPI.dll")] private static extern bool ftrScanGetImageSize(IntPtr h, out FTRSCAN_IMAGE_SIZE s);
        [DllImport("ftrScanAPI.dll")] private static extern bool ftrScanGetFrame(IntPtr h, IntPtr p, IntPtr f);
        [DllImport("ftrScanAPI.dll")] private static extern void ftrScanCloseDevice(IntPtr h);

        public Task<byte[]?> CapturarHuellaAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                IntPtr h = ftrScanOpenDevice();
                if (h == IntPtr.Zero) return null;

                try
                {
                    if (ftrScanGetImageSize(h, out var size))
                    {
                        IntPtr p = Marshal.AllocHGlobal(size.nImageSize);
                        try
                        {
                            // Bucle de sondeo del sensor para permitir cancelación reactiva
                            while (!cancellationToken.IsCancellationRequested)
                            {
                                if (ftrScanGetFrame(h, p, IntPtr.Zero))
                                {
                                    byte[] data = new byte[size.nImageSize];
                                    Marshal.Copy(p, data, 0, size.nImageSize);
                                    return data;
                                }

                                // Pequeño delay de cortesía para no saturar la CPU
                                Thread.Sleep(100);
                            }

                            // Lanza excepción canónica si el token fue cancelado
                            cancellationToken.ThrowIfCancellationRequested();
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(p);
                        }
                    }
                }
                finally
                {
                    ftrScanCloseDevice(h);
                }

                return null;
            }, cancellationToken);
        }
    }
}
