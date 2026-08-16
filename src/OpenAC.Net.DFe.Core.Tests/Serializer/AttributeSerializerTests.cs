using System.Xml.Linq;
using OpenAC.Net.DFe.Core.Tests.Serializer.Models;

namespace OpenAC.Net.DFe.Core.Tests.Serializer;

public class AttributeSerializerTests
{
    [Test]
    public async Task TestSerializeAttributesOnRoot()
    {
        var model = new AttributeTestModel
        {
            Versao = "2.00",
            Id = 5,
            Status = StatusOperacao.Aprovado,
            CodigoOpcional = "ABC",
            Conteudo = "Dados"
        };

        var xml = model.GetXml();
        var xDoc = XDocument.Parse(xml);
        var root = xDoc.Root;

        await Assert.That(root).IsNotNull();
        XNamespace ns = "https://www.openac.net.br/teste";
        await Assert.That(root!.Attribute("versao")?.Value).IsEqualTo("2.00");
        await Assert.That(root.Attribute("id")?.Value).IsEqualTo("005");
        await Assert.That(root.Attribute("status")?.Value).IsEqualTo("APR");
        await Assert.That(root.Attribute("codigoOpcional")?.Value).IsEqualTo("ABC");
        await Assert.That(root.Element(ns + "conteudo")?.Value).IsEqualTo("Dados");
    }

    [Test]
    public async Task TestDeserializeAttributesOnRoot()
    {
        const string xml = """
                           <AttributeRoot xmlns="https://www.openac.net.br/teste" versao="3.10" id="042" status="REJ" codigoOpcional="XYZ">
                                       <conteudo>Corpo da mensagem</conteudo>
                                   </AttributeRoot>
                           """;

        var model = AttributeTestModel.Load(xml);

        await Assert.That(model).IsNotNull();
        await Assert.That(model.Versao).IsEqualTo("3.10");
        await Assert.That(model.Id).IsEqualTo(42);
        await Assert.That(model.Status).IsEqualTo(StatusOperacao.Rejeitado);
        await Assert.That(model.CodigoOpcional).IsEqualTo("XYZ");
        await Assert.That(model.Conteudo).IsEqualTo("Corpo da mensagem");
    }

    [Test]
    public async Task TestAttributeShouldSerializeCondition()
    {
        var model = new AttributeTestModel
        {
            Versao = "1.00",
            Id = 0, // ShouldSerializeId() returns false when <= 0
            Status = StatusOperacao.Pendente,
            CodigoOpcional = null
        };

        var xml = model.GetXml();
        var xDoc = XDocument.Parse(xml);

        await Assert.That(xDoc.Root?.Attribute("id")).IsNull();
        await Assert.That(xDoc.Root?.Attribute("codigoOpcional")).IsNull();
        await Assert.That(xDoc.Root?.Attribute("versao")).IsNotNull();
    }
}
