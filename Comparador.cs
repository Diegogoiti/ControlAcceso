using System;
using SourceAFIS;

namespace ControlAcceso
{
    public static class Comparador
    {
        // 1. Genera la "firma" de la huella
        public static FingerprintTemplate GenerarTemplate(byte[] rawData, int width, int height)
        {
            // Creamos la imagen pasando los datos crudos y las dimensiones
            var image = new FingerprintImage(width, height, rawData);
            return new FingerprintTemplate(image);
        }

        // 2. Compara dos templates
        public static double CalcularSimilitud(FingerprintTemplate t1, FingerprintTemplate t2)
        {
            var matcher = new FingerprintMatcher(t1);
            return matcher.Match(t2);
        }
    }
}
