using System.Xml.Linq;
using OpenAC.Net.DFe.Core.Tests.Commom;

namespace OpenAC.Net.DFe.Core.Tests.Serializer;

public class ComplexDocumentSerializerTests
{
    private static TesteXml GenerateXml()
    {
        var xml = new TesteXml
        {
            Id = 1,
            TestDate = DateTime.Now,
            TestDecimal = 100000.00M,
            TesteEnum = TesteEnum.Value3,
            TesteEnum1 = TesteEnum.Value1,
            TesteEnum2 = null,
            TestNullInt = 999,
            TestDateTz = new DateTimeOffset(DateTime.Now, TimeSpan.FromHours(-4))
        };

        var cdata = File.ReadAllText("cdata_teste.xml");

        for (var i = 0; i < 3; i++)
        {
            var item = new TesteXml2
            {
                Id = i + 1,
                TestDecimal = xml.TestDecimal + i + 1.000M,
                TestString = $"<![CDATA[{cdata}]]>"
            };
            xml.XmlItems.Add(item);
        }

        xml.XmlItems2 = xml.XmlItems.AsEnumerable();
        xml.XmlItems3 = [.. xml.XmlItems];

        for (var i = 0; i < 3; i++)
        {
            xml.TesteListEnum.Add((TesteEnum)i);
        }

        xml.TesteDateTime.Add(DateTime.Now);
        xml.TesteDateTime.Add(DateTime.MinValue);
        xml.TesteDateTime.Add(DateTime.MaxValue);

        var collection = new Xml3Collection();

        for (var i = 0; i < 3; i++)
        {
            var item = new TesteXml3
            {
                Id = i + 1,
                TestDecimal = xml.TestDecimal + i + 1.000M,
                TestString = $"XmlItem3 {i + 1}"
            };
            xml.XmlItems.Add(item);
            collection.Add(item);
        }

        xml.TestInterface3 = collection;

        for (var i = 0; i < 5; i++)
        {
            var prod = xml.XmlProd.AddNew();
            prod.Id = i;
            prod.TestDecimal = xml.TestDecimal + 1;
            prod.TestString = "XmlItem4  1";

            var prod2 = xml.XmlProd2.AddNew();
            prod2.Id = i;
            prod2.TestDecimal = xml.TestDecimal + 1;
            prod2.TestString = "XmlItem4  2";
        }

        xml.XmlProd3 = xml.XmlProd2.ToArray();

        xml.TestInterface1 = xml.XmlItems[0];
        xml.TestInterface2 = xml.XmlItems[1];
        xml.Xml5.Id = 10;
        xml.Xml5.TestDecimal = 5.0000000000M;

        return xml;
    }

    [Test]
    public async Task TestSerializeComplexDocument()
    {
        var xml = GenerateXml();
        xml.Save("teste_complexo.xml");

        await Assert.That(File.Exists("teste_complexo.xml")).IsTrue();

        var xmlDocument = XDocument.Load("teste_complexo.xml");
        await Assert.That(xmlDocument).IsNotNull();
        await Assert.That(xmlDocument.Root).IsNotNull();
        await Assert.That(xmlDocument.Root?.Name.LocalName).IsEqualTo("RFTD");
        await Assert.That(xmlDocument.Root!.HasAttributes).IsTrue();
        await Assert.That(xmlDocument.Root.Attributes().Count()).IsEqualTo(1);
        await Assert.That(xmlDocument.Root.FirstAttribute?.Name.LocalName).IsEqualTo("id");
        await Assert.That(xmlDocument.Root.FirstAttribute?.Value).IsEqualTo("01");

        var nodes = xmlDocument.Root.Nodes();
        await Assert.That(nodes.Count()).IsEqualTo(39);

        File.Delete("teste_complexo.xml");
    }

    [Test]
    public async Task TestDeserializeComplexDocument()
    {
        var xml = GenerateXml();
        xml.Save("teste_complexo_deser.xml");

        var item = TesteXml.Load("teste_complexo_deser.xml");

        await Assert.That(item).IsNotEqualTo(xml);
        await Assert.That(item.Signature).IsNotNull();
        await Assert.That(item.Xml5).IsNotNull();
        await Assert.That(item.Xml6).IsNull();
        await Assert.That(item.TesteEnum2).IsNull();

        File.Delete("teste_complexo_deser.xml");
    }
}
