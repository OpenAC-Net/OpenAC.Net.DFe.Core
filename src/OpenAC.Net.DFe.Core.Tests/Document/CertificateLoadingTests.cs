using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace OpenAC.Net.DFe.Core.Tests.Document;

public class CertificateLoadingTests
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task LoadsPasswordProtectedPfxWithPrivateKey(bool fromFile)
    {
        using var key = RSA.Create(2048);
        using var original = CreateCertificate(key);
        var pfx = original.Export(X509ContentType.Pfx, "senha-teste");
        using var loaded = LoadPfx(pfx, "senha-teste", fromFile);

        await Assert.That(loaded.Thumbprint).IsEqualTo(original.Thumbprint);
        await Assert.That(loaded.HasPrivateKey).IsTrue();
        using var loadedKey = loaded.GetRSAPrivateKey()!;
        var data = new byte[] { 1, 2, 3 };
        var signature = loadedKey.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        await Assert.That(key.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)).IsTrue();
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task RejectsIncorrectPfxPassword(bool fromFile)
    {
        using var key = RSA.Create(2048);
        using var original = CreateCertificate(key);
        var pfx = original.Export(X509ContentType.Pfx, "senha-teste");
        await Assert.That(() =>
        {
            using var loaded = LoadPfx(pfx, "senha-incorreta", fromFile);
        }).Throws<CryptographicException>();
    }

    [Test]
    public async Task ValidatesSignedXmlAndRejectsTampering()
    {
        using var key = RSA.Create(2048);
        using var certificate = CreateCertificate(key);
        var document = new XmlDocument { PreserveWhitespace = true };
        document.LoadXml("<Documento><Informacao Id='teste'><Valor>100</Valor></Informacao></Documento>");
        document.AssinarDocumento("Documento", "Informacao", "Id", certificate, digest: SignDigest.SHA256);

        await Assert.That(document.ValidarAssinatura()).IsTrue();
        document.GetElementsByTagName("Valor")[0]!.InnerText = "200";
        await Assert.That(document.ValidarAssinatura()).IsFalse();
    }

    private static X509Certificate2 CreateCertificate(RSA key)
    {
        var request = new CertificateRequest("CN=DFe Tests", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));
    }

    private static X509Certificate2 LoadPfx(byte[] pfx, string password, bool fromFile)
    {
        if (!fromFile) return CertificadoDigital.SelecionarCertificado(pfx, password);

        var path = Path.Combine(Path.GetTempPath(), $"dfe-certificate-{Guid.NewGuid():N}.pfx");
        try
        {
            File.WriteAllBytes(path, pfx);
            return CertificadoDigital.SelecionarCertificado(path, password);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
