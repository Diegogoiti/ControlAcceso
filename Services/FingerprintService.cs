using System.IO;
using SourceAFIS;

namespace ControlAcceso.Services;

public class FingerprintService
{
    public FingerprintTemplate CrearTemplate(byte[] rawData) =>
        Comparador.GenerarTemplate(rawData, 320, 480);

    public bool Comparar(FingerprintTemplate t1, FingerprintTemplate t2) =>
        (Comparador.CalcularSimilitud(t1, t2) > 50);

    public void Guardar(FingerprintTemplate template, string path) =>
        File.WriteAllBytes(path, template.ToByteArray());

    public FingerprintTemplate Cargar(string path) =>
        new FingerprintTemplate(File.ReadAllBytes(path));

    public static FingerprintTemplate CargarDesdeBytes(byte[] templateBytes) =>
        new FingerprintTemplate(templateBytes);
}
