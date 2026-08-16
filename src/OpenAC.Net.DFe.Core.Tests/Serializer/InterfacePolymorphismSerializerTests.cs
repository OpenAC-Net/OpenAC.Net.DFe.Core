using System.Xml.Linq;
using OpenAC.Net.DFe.Core.Tests.Commom;

namespace OpenAC.Net.DFe.Core.Tests.Serializer;

public class InterfacePolymorphismSerializerTests
{
    [Test]
    public async Task TestSerializePolymorphicItemsInCollection()
    {
        var model = new TesteXml();
        model.XmlItems.Add(new TesteXml2 { Id = 1, TestDecimal = 10.00m, TestString = "ItemTipo2" });
        model.XmlItems.Add(new TesteXml3 { Id = 2, TestDecimal = 20.00m, TestString = "ItemTipo3" });

        var xml = model.GetXml();
        var xDoc = XDocument.Parse(xml);

        var itensElement = xDoc.Root?.Element("Itens");
        await Assert.That(itensElement).IsNotNull();

        var item2 = itensElement!.Element("Item2");
        var item3 = itensElement.Element("Item3");

        await Assert.That(item2).IsNotNull();
        await Assert.That(item3).IsNotNull();
        await Assert.That(item2!.Attribute("id")?.Value).IsEqualTo("01");
        await Assert.That(item3!.Attribute("id")?.Value).IsEqualTo("02");
    }

    [Test]
    public async Task TestDeserializePolymorphicItems()
    {
        const string xml = """
                           <RFTD>
                                       <Itens>
                                           <Item2 id="01"><decimal1>10.00</decimal1><testString1>Tipo 2</testString1></Item2>
                                           <Item3 id="02"><decimal1>20.00</decimal1><testString1>Tipo 3</testString1></Item3>
                                       </Itens>
                                   </RFTD>
                           """;

        var model = TesteXml.Load(xml);

        await Assert.That(model).IsNotNull();
        await Assert.That(model.XmlItems.Count).IsEqualTo(2);
        await Assert.That(model.XmlItems[0]).IsTypeOf<TesteXml2>();
        await Assert.That(model.XmlItems[1]).IsTypeOf<TesteXml3>();
        await Assert.That(((TesteXml2)model.XmlItems[0]).Id).IsEqualTo(1);
        await Assert.That(((TesteXml3)model.XmlItems[1]).Id).IsEqualTo(2);
    }
}
