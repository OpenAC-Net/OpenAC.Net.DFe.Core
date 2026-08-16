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
