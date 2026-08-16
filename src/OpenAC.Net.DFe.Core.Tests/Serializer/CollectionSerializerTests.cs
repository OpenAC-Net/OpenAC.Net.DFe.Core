using System.Xml.Linq;
using OpenAC.Net.DFe.Core.Collection;
using OpenAC.Net.DFe.Core.Tests.Serializer.Models;

namespace OpenAC.Net.DFe.Core.Tests.Serializer;

public class CollectionSerializerTests
{
    [Test]
    public async Task TestSerializeListOfObjectsAndArrays()
    {
        var model = new CollectionTestModel
        {
            ItensList = new List<SubItemModel>
            {
                new() { Sequencia = 1, Descricao = "Item 1", Valor = 10.50m },
                new() { Sequencia = 2, Descricao = "Item 2", Valor = 20.00m }
            },
            ItensArray = new[]
            {
                new SubItemModel { Sequencia = 10, Descricao = "Array 1", Valor = 100.00m }
            },
            DFeItens = new DFeCollection<SubItemModel>
            {
                new() { Sequencia = 100, Descricao = "DFeItem 1", Valor = 500.00m }
            },
            Tags = new List<string> { "tagA", "tagB", "tagC" },
            Codigos = new List<int> { 10, 20, 30 },
            StatusList = new List<StatusOperacao> { StatusOperacao.Pendente, StatusOperacao.Aprovado }
        };

        var xml = model.GetXml();
        var xDoc = XDocument.Parse(xml);
        var root = xDoc.Root;

        await Assert.That(root).IsNotNull();
        await Assert.That(root!.Elements("itens").Count()).IsEqualTo(2);
        await Assert.That(root.Elements("itensArray").Count()).IsEqualTo(1);
        await Assert.That(root.Elements("dfeItens").Count()).IsEqualTo(1);
        await Assert.That(root.Elements("tags").Count()).IsEqualTo(3);
        await Assert.That(root.Elements("codigos").Count()).IsEqualTo(3);
        await Assert.That(root.Elements("statusList").Count()).IsEqualTo(2);
    }

    [Test]
    public async Task TestDeserializeListOfObjectsAndArrays()
    {
        const string xml = """
                           <CollectionRoot>
                                       <itens nSeq="1"><descricao>Item A</descricao><valor>15.00</valor></itens>
                                       <itens nSeq="2"><descricao>Item B</descricao><valor>25.00</valor></itens>
                                       <itensArray nSeq="3"><descricao>Array A</descricao><valor>35.00</valor></itensArray>
                                       <dfeItens nSeq="4"><descricao>DFe A</descricao><valor>45.00</valor></dfeItens>
                                       <tags>alpha</tags>
                                       <tags>beta</tags>
                                       <codigos>100</codigos>
                                       <codigos>200</codigos>
                                       <statusList>APR</statusList>
                                       <statusList>REJ</statusList>
                                   </CollectionRoot>
                           """;

        var model = CollectionTestModel.Load(xml);

        await Assert.That(model).IsNotNull();
        await Assert.That(model.ItensList.Count).IsEqualTo(2);
        await Assert.That(model.ItensList[0].Sequencia).IsEqualTo(1);
        await Assert.That(model.ItensList[0].Descricao).IsEqualTo("Item A");
        await Assert.That(model.ItensList[0].Valor).IsEqualTo(15.00m);

        await Assert.That(model.ItensArray.Length).IsEqualTo(1);
        await Assert.That(model.ItensArray[0].Sequencia).IsEqualTo(3);

        await Assert.That(model.DFeItens.Count).IsEqualTo(1);
        await Assert.That(model.DFeItens[0].Sequencia).IsEqualTo(4);

        await Assert.That(model.Tags.Count).IsEqualTo(2);
        await Assert.That(model.Tags[0]).IsEqualTo("alpha");
        await Assert.That(model.Tags[1]).IsEqualTo("beta");

        await Assert.That(model.Codigos.Count).IsEqualTo(2);
        await Assert.That(model.Codigos[0]).IsEqualTo(100);
        await Assert.That(model.Codigos[1]).IsEqualTo(200);

        await Assert.That(model.StatusList.Count).IsEqualTo(2);
        await Assert.That(model.StatusList[0]).IsEqualTo(StatusOperacao.Aprovado);
        await Assert.That(model.StatusList[1]).IsEqualTo(StatusOperacao.Rejeitado);
    }

    [Test]
    public async Task TestEmptyCollectionsOmittedOrHandled()
    {
        var model = new CollectionTestModel();
        var xml = model.GetXml();
        var xDoc = XDocument.Parse(xml);

        await Assert.That(xDoc.Root?.Elements("itens").Count()).IsEqualTo(0);
        await Assert.That(xDoc.Root?.Elements("tags").Count()).IsEqualTo(0);

        var loaded = CollectionTestModel.Load(xml);
        await Assert.That(loaded.ItensList).IsEmpty();
        await Assert.That(loaded.Tags).IsEmpty();
    }
}
