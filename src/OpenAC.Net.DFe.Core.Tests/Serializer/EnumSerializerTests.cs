using System.Xml.Linq;
using OpenAC.Net.DFe.Core.Tests.Serializer.Models;

namespace OpenAC.Net.DFe.Core.Tests.Serializer;

public class EnumSerializerTests
{
    [Test]
    public async Task TestSerializeEnumWithDFeEnumAttribute()
    {
        var model = new EnumTestModel
        {
            Status = StatusOperacao.Aprovado,
            StatusNullable = StatusOperacao.Rejeitado,
            Modalidade = ModalidadeFrete.ContaEmitente,
            ModalidadeNullable = ModalidadeFrete.ContaDestinatario
        };

        var xml = model.GetXml();
        var xDoc = XDocument.Parse(xml);
        var root = xDoc.Root;

        await Assert.That(root).IsNotNull();
        await Assert.That(root!.Element("status")?.Value).IsEqualTo("APR");
        await Assert.That(root.Element("statusNullable")?.Value).IsEqualTo("REJ");
        await Assert.That(root.Element("modalidade")?.Value).IsEqualTo("ContaEmitente");
        await Assert.That(root.Element("modalidadeNullable")?.Value).IsEqualTo("ContaDestinatario");
    }

    [Test]
    public async Task TestDeserializeEnumWithDFeEnumAttribute()
    {
        const string xml = """
                           <EnumRoot>
                                       <status>REJ</status>
                                       <statusNullable>PEN</statusNullable>
                                       <modalidade>ContaDestinatario</modalidade>
                                       <modalidadeNullable>SemFrete</modalidadeNullable>
                                   </EnumRoot>
                           """;

        var model = EnumTestModel.Load(xml);
        await Assert.That(model).IsNotNull();
        await Assert.That(model.Status).IsEqualTo(StatusOperacao.Rejeitado);
        await Assert.That(model.StatusNullable).IsEqualTo(StatusOperacao.Pendente);
        await Assert.That(model.Modalidade).IsEqualTo(ModalidadeFrete.ContaDestinatario);
        await Assert.That(model.ModalidadeNullable).IsEqualTo(ModalidadeFrete.SemFrete);
    }

    [Test]
    public async Task TestNullableEnumOmittedWhenNull()
    {
        var model = new EnumTestModel
        {
            Status = StatusOperacao.Pendente,
            StatusNullable = null,
            Modalidade = ModalidadeFrete.SemFrete,
            ModalidadeNullable = null
        };

        var xml = model.GetXml();
        var xDoc = XDocument.Parse(xml);

        await Assert.That(xDoc.Root?.Element("statusNullable")).IsNull();
        await Assert.That(xDoc.Root?.Element("modalidadeNullable")).IsNull();
    }
}
