using System;
using SourceAFIS;

namespace ControlAcceso.Biometrics
{
    public class SourceAFISAdapter : IBiometricAdapter
    {
        // FS88H: sensor óptico CMOS, 500 DPI nativos (confirmado en la hoja de
        // especificaciones de Futronic). SourceAFIS ignora cualquier DPI embebido
        // en la imagen misma, así que hay que pasarlo explícito o asume 500 por
        // defecto de todas formas — esto solo blinda el código para no depender
        // de esa coincidencia si algún día cambian de modelo de lector.
        private static readonly FingerprintImageOptions _opcionesImagen = new FingerprintImageOptions
        {
            Dpi = 500
        };

        public byte[] GenerarTemplateBytes(byte[] rawImageData, int width = 320, int height = 480)
        {
            if (rawImageData == null || rawImageData.Length == 0)
                throw new ArgumentException("Los datos crudos de la imagen no pueden estar vacíos.");

            var image = new FingerprintImage(width, height, rawImageData, _opcionesImagen);
            var template = new FingerprintTemplate(image);

            return template.ToByteArray();
        }

        public double CalcularSimilitud(byte[] templateBytes1, byte[] templateBytes2)
        {
            if (templateBytes1 == null || templateBytes2 == null)
                return 0.0;

            try
            {
                var t1 = new FingerprintTemplate(templateBytes1);
                var t2 = new FingerprintTemplate(templateBytes2);

                var matcher = new FingerprintMatcher(t1);
                return matcher.Match(t2);
            }
            catch
            {
                return 0.0;
            }
        }

        public bool EsCoincidencia(byte[] templateBytes1, byte[] templateBytes2, double umbral = 35.0)
        {
            double score = CalcularSimilitud(templateBytes1, templateBytes2);
            return score > umbral;
        }
    }
}
