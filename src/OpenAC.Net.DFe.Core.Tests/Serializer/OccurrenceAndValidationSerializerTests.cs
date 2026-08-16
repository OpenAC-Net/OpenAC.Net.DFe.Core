using System.Xml.Linq;
using OpenAC.Net.DFe.Core.Tests.Serializer.Models;

namespace OpenAC.Net.DFe.Core.Tests.Serializer;

public class OccurrenceAndValidationSerializerTests
{
    [Test]
    public async Task TestOcorrenciaMaiorQueZeroOmittedWhenZero()
    {
        var model = new OccurrenceTestModel
        {
            CampoObrigatorio = "Teste",
            CampoMaiorQueZero = 0m,
            CampoOpcional = null,
            CampoCondicional = 5 // <= 10 -> ShouldSerialize returns false
        };

        var xml = model.GetXml();
        var xDoc = XDocument.Parse(xml);

        await Assert.That(xDoc.Root?.Element("campoObrigatorio")).IsNotNull();
        await Assert.That(xDoc.Root?.Element("campoOpcional")).IsNull();
        await Assert.That(xDoc.Root?.Element("campoMaiorQueZero")).IsNull();
        await Assert.That(xDoc.Root?.Element("campoCondicional")).IsNull();
    }

    [Test]
    public async Task TestOcorrenciaMaiorQueZeroIncludedWhenGreaterThanZero()
    {
        var model = new OccurrenceTestModel
        {
            CampoObrigatorio = "Teste",
            CampoMaiorQueZero = 15.50m,
            CampoOpcional = "Preenchido",
            CampoCondicional = 20 // > 10 -> ShouldSerialize returns true
        };

        var xml = model.GetXml();
        var xDoc = XDocument.Parse(xml);

        await Assert.That(xDoc.Root?.Element("campoObrigatorio")?.Value).IsEqualTo("Teste");
        await Assert.That(xDoc.Root?.Element("campoOpcional")?.Value).IsEqualTo("Preenchido");
        await Assert.That(xDoc.Root?.Element("campoMaiorQueZero")?.Value).IsEqualTo("15.50");
        await Assert.That(xDoc.Root?.Element("campoCondicional")?.Value).IsEqualTo("20");
    }
}
