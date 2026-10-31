using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAC.Net.DFe.Core.Http;

/// <summary>Transporte desktop com pool por certificado, TLS, proxy, perfil e host.</summary>
public class DFeHttpTransport : DFeHttpTransportBase
{
    /// <summary>Instância compartilhada para consumidores sem injeção de dependências.</summary>
    public static DFeHttpTransport Instance { get; } = new();

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendCoreAsync(HttpRequestMessage request,
        DFeHttpTransportOptions options, CancellationToken cancellationToken)
    {
        if (options.Certificate != null)
            Common.DFeCertificadosConfigBase.ValidarCertificado(options.Certificate, options.MargemValidadeCertificado);
        using var lease = DFeHttpClientPool.Instance.Acquire(options, request.RequestUri!);
        // O empréstimo permanece ativo até terminar o corpo, inclusive se a entrada expirar.
        return await lease.Pipeline.ExecuteAsync(async token =>
            await lease.Client.SendAsync(request, HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);
    }
}
