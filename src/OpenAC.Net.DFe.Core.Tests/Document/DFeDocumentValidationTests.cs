using System;
using System.Xml.Linq;
using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Document;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Tests.Document;

public class DFeDocumentValidationTests
{
    [DFeRoot("DummyWithRoot")]
    private class DummyDocWithRoot : DFeDocument<DummyDocWithRoot>
    {
        public override XElement WriteToXml(string? rootName = null, string? rootNamespace = null, SerializerOptions? options = null) => new("Dummy");
        public override void ReadXml(XElement element, SerializerOptions? options = null) { }
    }

    [DFeRoot("DummyWithRoot")]
    private class DummyDocCapturaRaiz : DFeDocument<DummyDocCapturaRaiz>
    {
        public string? RaizLida { get; private set; }

        public override XElement WriteToXml(string? rootName = null, string? rootNamespace = null, SerializerOptions? options = null) => new("Dummy");
        public override void ReadXml(XElement element, SerializerOptions? options = null) => RaizLida = element.Element("valor")?.Value;
    }

    [DFeRoot("DummySign")]
    [DFeSignInfoElement("infDummy")]
    private class DummySignDocValid : DFeSignDocument<DummySignDocValid>
    {
        public override XElement WriteToXml(string? rootName = null, string? rootNamespace = null, SerializerOptions? options = null) => new("Dummy");
        public override void ReadXml(XElement element, SerializerOptions? options = null) { }

        public void TestAssinar(System.Security.Cryptography.X509Certificates.X509Certificate2 cert)
        {
            AssinarDocumento(cert, Common.DFeSaveOptions.DisableFormatting, false);
        }
    }

    [Test]
    public async Task ShouldInstantiateSuccessfullyDFeDocument()
    {
        var doc = new DummyDocWithRoot();
        await Assert.That(doc).IsNotNull();
    }

    [Test]
    public async Task ShouldInstantiateSuccessfullyDFeSignDocument()
    {
        var doc = new DummySignDocValid();
        await Assert.That(doc).IsNotNull();
    }

    [Test]
    public async Task ShouldThrowArgumentExceptionWhenLoadStringIsEmpty()
    {
        await Assert.That(() => DFeDocument<DummyDocWithRoot>.Load(string.Empty))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task ShouldThrowArgumentNullExceptionWhenLoadStreamIsNull()
    {
        await Assert.That(() => DFeDocument<DummyDocWithRoot>.Load((System.IO.Stream)null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldLoadFromRootElement()
    {
        var doc = DFeDocument<DummyDocCapturaRaiz>.Load("<DummyWithRoot><valor>raiz</valor></DummyWithRoot>");
        await Assert.That(doc.RaizLida).IsEqualTo("raiz");
    }

    [Test]
    public async Task ShouldLoadFromNestedRootElement()
    {
        const string xml = "<evento><infEvento><valor>evento</valor><DummyWithRoot><valor>aninhado</valor></DummyWithRoot></infEvento></evento>";

        var doc = DFeDocument<DummyDocCapturaRaiz>.Load(xml);
        await Assert.That(doc.RaizLida).IsEqualTo("aninhado");
        await Assert.That(doc.Xml).IsEqualTo(xml);

        using var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));
        var docStream = DFeDocument<DummyDocCapturaRaiz>.Load(stream);
        await Assert.That(docStream.RaizLida).IsEqualTo("aninhado");
    }

    [Test]
    public async Task ShouldFallbackToDocumentRootWhenRootTagNotFound()
    {
        var doc = DFeDocument<DummyDocCapturaRaiz>.Load("<outro><valor>fallback</valor></outro>");
        await Assert.That(doc.RaizLida).IsEqualTo("fallback");
    }

    [Test]
    public async Task ShouldThrowArgumentExceptionWhenSavePathIsEmpty()
    {
        var doc = new DummyDocWithRoot();
        await Assert.That(() => doc.Save(string.Empty))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task ShouldThrowArgumentNullExceptionWhenSaveStreamIsNull()
    {
        var doc = new DummyDocWithRoot();
        await Assert.That(() => doc.Save((System.IO.Stream)null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ShouldThrowArgumentNullExceptionWhenAssinarWithNullCert()
    {
        var doc = new DummySignDocValid();
        await Assert.That(() => doc.TestAssinar(null!))
            .Throws<ArgumentNullException>();
    }
}
