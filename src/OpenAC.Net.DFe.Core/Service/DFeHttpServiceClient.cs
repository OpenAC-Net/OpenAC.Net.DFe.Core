using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenAC.Net.DFe.Core.Http;

namespace OpenAC.Net.DFe.Core.Service;

/// <summary>Base HTTP independente do modelo fiscal, compartilhada pelos clientes e provedores dos componentes.</summary>
public abstract class DFeHttpServiceClient
{
    /// <summary>Transporte desktop ou do host usado na operação.</summary>
    protected abstract IDFeHttpTransport TransporteHttp { get; }

    /// <summary>Token adicional da operação corrente.</summary>
    protected virtual CancellationToken TokenOperacao => default;

    /// <summary>Indica se o certificado capturado nas opções pertence exclusivamente a este envio.</summary>
    protected virtual bool DescartarCertificadoAposEnvio => false;

    /// <summary>Captura as opções HTTP e o certificado para o envio corrente.</summary>
    protected abstract DFeHttpTransportOptions CriarOpcoesHttp();

    /// <summary>Envia a requisição pelo transporte compartilhado, preservando respostas HTTP de erro.</summary>
    protected async Task<HttpResponseMessage> EnviarRequisicaoAsync(HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        using var operation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, TokenOperacao);
        operation.Token.ThrowIfCancellationRequested();
        var options = CriarOpcoesHttp();
        try
        {
            return await TransporteHttp.SendAsync(request, options, operation.Token).ConfigureAwait(false);
        }
        finally
        {
            if (DescartarCertificadoAposEnvio) options.Certificate?.Dispose();
        }
    }

    /// <summary>Lê texto fiscal com a política comum de charset.</summary>
    protected Task<string> LerRespostaAsync(HttpResponseMessage response, CancellationToken cancellationToken = default) =>
        DFeHttpResponse.ReadAsStringAsync(response.Content, cancellationToken);

    /// <summary>Permite especializar a política comum de decodificação de respostas.</summary>
    protected virtual string DecodificarResposta(byte[] bytes, string? headerCharset) =>
        DFeHttpResponse.Decode(bytes, headerCharset);
}
