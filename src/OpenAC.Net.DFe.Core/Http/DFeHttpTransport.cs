using System;
#if NETFRAMEWORK
using System.Net;
#endif
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAC.Net.DFe.Core.Http;

/// <summary>Transporte desktop com pool por certificado, TLS, proxy, perfil e host.</summary>
public class DFeHttpTransport : DFeHttpTransportBase
{
#if NETFRAMEWORK
    private static readonly SemaphoreSlim tlsLock = new(1, 1);
#endif

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
        {
#if NETFRAMEWORK
            // A configuração é global. Evita que envios do transporte sobrescrevam o TLS uns dos outros.
            await tlsLock.WaitAsync(token).ConfigureAwait(false);
            var previousProtocols = ServicePointManager.SecurityProtocol;
            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)options.Protocols;
#endif
                return await lease.Client.SendAsync(request, HttpCompletionOption.ResponseContentRead, token)
                    .ConfigureAwait(false);
#if NETFRAMEWORK
            }
            finally
            {
                ServicePointManager.SecurityProtocol = previousProtocols;
                tlsLock.Release();
            }
#endif
        }, cancellationToken).ConfigureAwait(false);
    }
}
