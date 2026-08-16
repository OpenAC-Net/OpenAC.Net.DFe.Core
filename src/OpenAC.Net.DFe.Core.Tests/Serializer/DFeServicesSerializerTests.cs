using OpenAC.Net.DFe.Core.Collection;
using OpenAC.Net.DFe.Core.Common;
using OpenAC.Net.DFe.Core.Service;
using OpenAC.Net.DFe.Core.Tests.Commom;

namespace OpenAC.Net.DFe.Core.Tests.Serializer;

public class DFeServicesSerializerTests
{
    [Test]
    public async Task TestSerializeAndDeserializeDFeServicesDocument()
    {
        var services = new DFeServices<DFeTipo>();
        services.Webservices.Add(new DFeServiceInfo<DFeTipo>
        {
            Tipo = DFeTipoServico.NFe,
            TipoEmissao = DFeTipoEmissao.Normal,
            Ambientes = new DFeCollection<DFeServiceEnvironment<DFeTipo>>
            {
                new()
                {
                    Ambiente = DFeTipoAmbiente.Homologacao,
                    UF = DFeSiglaUF.SP,
                    Enderecos = new Dictionary<DFeTipo, string>
                    {
                        { DFeTipo.Envio, "https://homologacao.nfe.fazenda.sp.gov.br/ws/nfeautorizacao4.asmx" },
                        { DFeTipo.Consulta, "https://homologacao.nfe.fazenda.sp.gov.br/ws/nferetautorizacao4.asmx" }
                    }
                },
                new()
                {
                    Ambiente = DFeTipoAmbiente.Producao,
                    UF = DFeSiglaUF.SP,
                    Enderecos = new Dictionary<DFeTipo, string>
                    {
                        { DFeTipo.Envio, "https://nfe.fazenda.sp.gov.br/ws/nfeautorizacao4.asmx" },
                        { DFeTipo.Consulta, "https://nfe.fazenda.sp.gov.br/ws/nferetautorizacao4.asmx" }
                    }
                }
            }
        });

        var xml = services.GetXml();
        await Assert.That(xml).IsNotNull().And.IsNotEqualTo(string.Empty);

        var loaded = DFeServices<DFeTipo>.Load(xml);
        var loadedXml = loaded.GetXml();

        await Assert.That(loadedXml).IsEqualTo(xml);
        await Assert.That(loaded[DFeTipoEmissao.Normal]).IsNotNull();
        await Assert.That(loaded[DFeTipoEmissao.Normal][DFeTipoAmbiente.Producao, DFeSiglaUF.SP]).IsNotNull();
        await Assert.That(loaded[DFeTipoEmissao.Normal][DFeTipoAmbiente.Producao, DFeSiglaUF.SP][DFeTipo.Envio])
            .IsEqualTo("https://nfe.fazenda.sp.gov.br/ws/nfeautorizacao4.asmx");
    }
}
