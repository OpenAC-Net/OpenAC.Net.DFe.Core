using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAC.Net.DFe.Core.Http;

/// <summary>Transporte padrão com pool por certificado/TLS/proxy/host e retentativas de métodos seguros.</summary>
public sealed class DFeHttpTransport : IDFeHttpTransport
{
    /// <summary>Instância compartilhada para consumidores sem injeção de dependências.</summary>
    public static DFeHttpTransport Instance { get; } = new();

    /// <inheritdoc />
    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, DFeHttpTransportOptions options,
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (options == null) throw new ArgumentNullException(nameof(options));
        var endpoint = request.RequestUri;
        if (endpoint == null || !endpoint.IsAbsoluteUri || (endpoint.Scheme != "http" && endpoint.Scheme != "https"))
            throw new ArgumentException("A requisição deve possuir uma URL HTTP absoluta.", nameof(request));

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(options.Timeout);
        var token = timeout.Token;
        token.ThrowIfCancellationRequested();

        var attempts = IsSafe(request.Method) ? options.Attempts : 1;
        // Uma requisição com efeito fiscal nunca é clonada nem repetida.
        byte[]? content = null;
        if (attempts > 1 && request.Content != null)
            content = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        token.ThrowIfCancellationRequested();

        using var lease = DFeHttpClientPool.Instance.Acquire(options, endpoint);
        for (var attempt = 1;; attempt++)
        {
            using var clone = attempts > 1 ? Clone(request, content) : null;
            try
            {
                var response = await lease.Client.SendAsync(clone ?? request,
                    HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(false);
                response.RequestMessage = request;
                if (attempt == attempts || !IsTransient(response.StatusCode)) return response;
                response.Dispose();
            }
            catch (HttpRequestException) when (attempt < attempts && !token.IsCancellationRequested)
            {
                // Apenas GET, HEAD, OPTIONS e TRACE podem repetir falhas de transporte.
            }

            if (options.RetryDelay > TimeSpan.Zero)
                await Task.Delay(options.RetryDelay, token).ConfigureAwait(false);
        }
    }

    private static HttpRequestMessage Clone(HttpRequestMessage source, byte[]? content)
    {
        var clone = new HttpRequestMessage(source.Method, source.RequestUri) { Version = source.Version };
#if NET5_0_OR_GREATER
        clone.VersionPolicy = source.VersionPolicy;
#endif
        foreach (var header in source.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        if (content != null)
        {
            clone.Content = new ByteArrayContent(content);
            foreach (var header in source.Content!.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }

    private static bool IsSafe(HttpMethod method) =>
        string.Equals(method.Method, "GET", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(method.Method, "HEAD", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(method.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(method.Method, "TRACE", StringComparison.OrdinalIgnoreCase);

    private static bool IsTransient(HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.RequestTimeout || (int)statusCode == 429 ||
        ((int)statusCode >= 500 && (int)statusCode <= 599);
}
