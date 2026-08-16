using System.Xml.Linq;
using OpenAC.Net.DFe.Core.Tests.Serializer.Models;

namespace OpenAC.Net.DFe.Core.Tests.Serializer;

public class ParentCollectionSerializerTests
{
    [Test]
    public async Task TestAddNewSetsParentReference()
    {
        var parent = new ParentChildTestModel { Codigo = "PAI_001" };
        var filho1 = parent.Filhos.AddNew();
        filho1.Id = 1;
        filho1.Nome = "Filho 1";

        var filho2 = parent.Filhos.AddNew();
        filho2.Id = 2;
        filho2.Nome = "Filho 2";

        await Assert.That(filho1.Parent).IsEqualTo(parent);
        await Assert.That(filho1.GetParentCodigo()).IsEqualTo("PAI_001");
        await Assert.That(filho2.Parent).IsEqualTo(parent);
        await Assert.That(filho2.GetParentCodigo()).IsEqualTo("PAI_001");
    }

    [Test]
    public async Task TestSerializeAndDeserializeParentChildCollection()
    {
        var parent = new ParentChildTestModel { Codigo = "EMP_123" };
        var f1 = parent.Filhos.AddNew();
        f1.Id = 10;
        f1.Nome = "Item Filho";

        var xml = parent.GetXml();
        var xDoc = XDocument.Parse(xml);

        await Assert.That(xDoc.Root?.Attribute("codigo")?.Value).IsEqualTo("EMP_123");
        await Assert.That(xDoc.Root?.Element("filhos")?.Attribute("id")?.Value).IsEqualTo("10");

        var loaded = ParentChildTestModel.Load(xml);
        await Assert.That(loaded.Codigo).IsEqualTo("EMP_123");
        await Assert.That(loaded.Filhos.Count).IsEqualTo(1);
        await Assert.That(loaded.Filhos[0].Id).IsEqualTo(10);
        await Assert.That(loaded.Filhos[0].Nome).IsEqualTo("Item Filho");
    }
}
