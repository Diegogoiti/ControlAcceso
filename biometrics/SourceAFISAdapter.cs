using System;
using SourceAFIS;

namespace ControlAcceso.Biometrics
{
    public class SourceAFISAdapter : IBiometricAdapter
    {
        public byte[] GenerarTemplateBytes(byte[] rawImageData, int width = 320, int height = 480)
        {
            if (rawImageData == null || rawImageData.Length == 0)
                throw new ArgumentException("Los datos crudos de la imagen no pueden estar vacíos.");

            // Reutiliza la lógica de tu 'Comparador.GenerarTemplate'
            var image = new FingerprintImage(width, height, rawImageData);
            var template = new FingerprintTemplate(image);

            // Exportamos los bytes para mantener el desacoplamiento fuera de esta clase
            return template.ToByteArray();
        }

        public double CalcularSimilitud(byte[] templateBytes1, byte[] templateBytes2)
        {
            if (templateBytes1 == null || templateBytes2 == null)
                return 0.0;

            try
            {
                // Reutiliza la deserialización que tenías en 'CargarDesdeBytes'
                var t1 = new FingerprintTemplate(templateBytes1);
                var t2 = new FingerprintTemplate(templateBytes2);

                // Reutiliza tu 'FingerprintMatcher'
                var matcher = new FingerprintMatcher(t1);
                return matcher.Match(t2);
            }
            catch
            {
                // Si ocurre una falla en el parseo binario del template
                return 0.0;
            }
        }

        public bool EsCoincidencia(byte[] templateBytes1, byte[] templateBytes2, double umbral = 50.0)
        {
            double score = CalcularSimilitud(templateBytes1, templateBytes2);
            return score > umbral;
        }
    }
}
