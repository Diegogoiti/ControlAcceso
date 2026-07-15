using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace ControlAcceso;

public static class Scanner
{
    [StructLayout(LayoutKind.Sequential)]
    private struct FTRSCAN_IMAGE_SIZE { public int nWidth, nHeight, nImageSize; }

    [DllImport("ftrScanAPI.dll")] private static extern IntPtr ftrScanOpenDevice();
    [DllImport("ftrScanAPI.dll")] private static extern bool ftrScanGetImageSize(IntPtr h, out FTRSCAN_IMAGE_SIZE s);
    [DllImport("ftrScanAPI.dll")] private static extern bool ftrScanGetFrame(IntPtr h, IntPtr p, IntPtr f);
    [DllImport("ftrScanAPI.dll")] private static extern void ftrScanCloseDevice(IntPtr h);

    public static byte[]? GetRawImage()
    {
        IntPtr h = ftrScanOpenDevice();
        if (h == IntPtr.Zero) return null;

        try
        {
            if (ftrScanGetImageSize(h, out var size))
            {
                IntPtr p = Marshal.AllocHGlobal(size.nImageSize);
                try
                {
                    if (ftrScanGetFrame(h, p, IntPtr.Zero))
                    {
                        byte[] data = new byte[size.nImageSize];
                        Marshal.Copy(p, data, 0, size.nImageSize);
                        return data;
                    }
                }
                finally { Marshal.FreeHGlobal(p); }
            }
        }
        finally { ftrScanCloseDevice(h); }
        return null;
    }
    public static void GuardarComoImagen(byte[] rawData, int width, int height)
    {
        // 1. Crear un Bitmap de 8 bits (escala de grises)
        using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed))
        {
            // 2. Configurar la paleta de colores (necesario para 8bpp)
            ColorPalette pal = bmp.Palette;
            for (int i = 0; i < 256; i++)
                pal.Entries[i] = Color.FromArgb(i, i, i);
            bmp.Palette = pal;

            // 3. Copiar los datos al Bitmap
            BitmapData bmpData = bmp.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed);

            System.Runtime.InteropServices.Marshal.Copy(rawData, 0, bmpData.Scan0, rawData.Length);
            bmp.UnlockBits(bmpData);

            // 4. Guardar como PNG (es un formato sin pérdida, ideal para huellas)
            bmp.Save("huella_test.png", ImageFormat.Png);
        }
    }
}
