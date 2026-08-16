using System.Xml.Linq;
using OpenAC.Net.DFe.Core.Tests.Serializer.Models;

namespace OpenAC.Net.DFe.Core.Tests.Serializer;

public class DictionarySerializerTests
{
    [Test]
    public async Task TestSerializeDictionaryWithKeyAsAttributeAndElement()
    {
        var model = new DictionaryTestModel
        {
            EnderecosPorStatus = new Dictionary<StatusOperacao, string>
            {
                { StatusOperacao.Aprovado, "https://sefaz.gov.br/aprovado" },
                { StatusOperacao.Rejeitado, "https://sefaz.gov.br/rejeitado" }
            },
            Parametros = new Dictionary<string, string>
            {
                { "Timeout", "30000" },
                { "Tentativas", "3" }
            }
        };

        var xml = model.GetXml();
        var xDoc = XDocument.Parse(xml);
        var root = xDoc.Root;

        await Assert.That(root).IsNotNull();

        var enderecos = root!.Element("enderecos");
        await Assert.That(enderecos).IsNotNull();

        var paramsNode = root.Element("parametros");
        await Assert.That(paramsNode).IsNotNull();
    }

    [Test]
    public async Task TestDeserializeDictionary()
    {
        const string xml = """
                           <DictionaryRoot>
                                       <enderecos>
                                           <Endereco Tipo="APR">https://sefaz.gov.br/aprovado</Endereco>
                                           <Endereco Tipo="REJ">https://sefaz.gov.br/rejeitado</Endereco>
                                       </enderecos>
                                       <parametros>
                                           <Parametros>
                                               <Chave>Timeout</Chave>
                                               <Valor>30000</Valor>
                                           </Parametros>
                                       </parametros>
                                   </DictionaryRoot>
                           """;

        var model = DictionaryTestModel.Load(xml);

        await Assert.That(model).IsNotNull();
        await Assert.That(model.EnderecosPorStatus.Count).IsEqualTo(2);
        await Assert.That(model.EnderecosPorStatus[StatusOperacao.Aprovado]).IsEqualTo("https://sefaz.gov.br/aprovado");
        await Assert.That(model.EnderecosPorStatus[StatusOperacao.Rejeitado]).IsEqualTo("https://sefaz.gov.br/rejeitado");
    }
}
