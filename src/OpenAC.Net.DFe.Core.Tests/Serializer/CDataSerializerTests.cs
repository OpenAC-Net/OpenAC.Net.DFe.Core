using OpenAC.Net.DFe.Core.Tests.Serializer.Models;

namespace OpenAC.Net.DFe.Core.Tests.Serializer;

public class CDataSerializerTests
{
    [Test]
    public async Task TestSerializeElementWithCData()
    {
        var model = new CDataTestModel
        {
            ConteudoHtml = "<div><p>Texto com tags & caracteres especiais</p></div>",
            ConteudoXml = "<dados><item id=\"1\">Valor</item></dados>",
            TextoNormal = "Texto simples sem escape"
        };

        var xml = model.GetXml();

        await Assert.That(xml).Contains("<![CDATA[<div><p>Texto com tags & caracteres especiais</p></div>]]>");
        await Assert.That(xml).Contains("<![CDATA[<dados><item id=\"1\">Valor</item></dados>]]>");
        await Assert.That(xml).Contains("<textoNormal>Texto simples sem escape</textoNormal>");
    }

    [Test]
    public async Task TestDeserializeElementWithCData()
    {
        const string xml = """
                           <CDataRoot>
                                       <conteudoHtml><![CDATA[<b>Negrito</b>]]></conteudoHtml>
                                       <conteudoXml><![CDATA[<det><prod>ABC</prod></det>]]></conteudoXml>
                                       <textoNormal>Simples</textoNormal>
                                   </CDataRoot>
                           """;

        var model = CDataTestModel.Load(xml);

        await Assert.That(model).IsNotNull();
        await Assert.That(model.ConteudoHtml).IsEqualTo("<b>Negrito</b>");
        await Assert.That(model.ConteudoXml).IsEqualTo("<det><prod>ABC</prod></det>");
        await Assert.That(model.TextoNormal).IsEqualTo("Simples");
    }
}
