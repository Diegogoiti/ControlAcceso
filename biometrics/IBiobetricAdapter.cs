namespace ControlAcceso.Biometrics
{
    public interface IBiometricAdapter
    {
        /// <summary>
        /// Genera los bytes de un template binario a partir de la imagen cruda de la huella.
        /// </summary>
        byte[] GenerarTemplateBytes(byte[] rawImageData, int width = 320, int height = 480);

        /// <summary>
        /// Calcula el puntaje de similitud (score) entre dos templates binarios.
        /// </summary>
        double CalcularSimilitud(byte[] templateBytes1, byte[] templateBytes2);

        /// <summary>
        /// Evalúa si dos templates binarios superan el umbral mínimo para considerar que es la misma huella.
        /// </summary>
        bool EsCoincidencia(byte[] templateBytes1, byte[] templateBytes2, double umbral = 50.0);
    }
}
