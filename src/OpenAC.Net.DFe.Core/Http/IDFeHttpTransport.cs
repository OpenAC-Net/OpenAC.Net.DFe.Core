using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAC.Net.DFe.Core.Http;

/// <summary>Transporte substituível para requisições REST, SOAP e multipart dos componentes DFe.</summary>
public interface IDFeHttpTransport
{
    /// <summary>Envia e bufferiza a resposta. O chamador deve descartar a resposta retornada.</summary>
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, DFeHttpTransportOptions options,
        CancellationToken cancellationToken = default);
}