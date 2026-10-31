using OpenAC.Net.DFe.Core.Common;

namespace OpenAC.Net.DFe.Core.Tests.Http;

internal sealed class HttpTestConfiguration : DFeConfigBase<HttpTestGeneral, HttpTestWebServices,
    HttpTestCertificates, HttpTestFiles>
{
    internal HttpTestConfiguration()
    {
        Geral = new();
        WebServices = new() { IntervaloTentativas = 0 };
        Certificados = new();
        Arquivos = new();
    }
}

internal sealed class HttpTestGeneral : DFeGeralConfigBase { }
internal sealed class HttpTestWebServices : DFeWebserviceConfigBase { }
internal sealed class HttpTestCertificates : DFeCertificadosConfigBase { }
internal sealed class HttpTestFiles : DFeArquivosConfigBase
{
    protected override void ArquivoServicoChange() { }
}
