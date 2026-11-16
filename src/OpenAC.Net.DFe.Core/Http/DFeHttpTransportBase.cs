using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAC.Net.DFe.Core.Http;

/// <summary>Fluxo HTTP compartilhado com retentativas seguras e timeout abrangendo a leitura do corpo.</summary>
public abstract class DFeHttpTransportBase : IDFeHttpTransport
{
    /// <summary>Indica se esta camada gerencia retentativas. Hosts com política própria devem retornar falso.</summary>
    protected virtual bool ManageRetries => true;

    /// <summary>Envia uma tentativa HTTP. A leitura final do corpo é controlada pela base.</summary>
    protected abstract Task<HttpResponseMessage> SendCoreAsync(HttpRequestMessage request,
        DFeHttpTransportOptions options, CancellationToken cancellationToken);

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
        if (!options.TimeoutPerAttempt) timeout.CancelAfter(options.Timeout);
        var token = timeout.Token;
        token.ThrowIfCancellationRequested();

        var attempts = ManageRetries && IsSafe(request.Method) ? options.Attempts : 1;
        // Bufferizar uma requisição fiscal não habilita retentativas de operações não seguras.
        byte[]? content = null;
        if ((attempts > 1 || options.BufferRequestContent) && request.Content != null)
            content = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        token.ThrowIfCancellationRequested();

        for (var attempt = 1;; attempt++)
        {
            using var clone = attempts > 1 || options.BufferRequestContent ? Clone(request, content) : null;
            using var attemptTimeout = options.TimeoutPerAttempt
                ? CancellationTokenSource.CreateLinkedTokenSource(token) : null;
            attemptTimeout?.CancelAfter(options.Timeout);
            var sendToken = attemptTimeout?.Token ?? token;
            try
            {
                var response = await SendCoreAsync(clone ?? request, options, sendToken).ConfigureAwait(false);
                try
                {
                    await DFeHttpResponse.BufferAsync(response, sendToken).ConfigureAwait(false);
                    response.RequestMessage = request;
                }
                catch
                {
                    response.Dispose();
                    throw;
                }
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
        foreach (var option in source.Options)
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
#else
        foreach (var property in source.Properties)
            clone.Properties[property.Key] = property.Value;
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
