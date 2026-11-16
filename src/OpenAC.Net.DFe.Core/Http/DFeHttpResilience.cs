using System;
using System.Net.Http;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace OpenAC.Net.DFe.Core.Http;

/// <summary>Política compartilhada pelos transportes padrão e IHttpClientFactory. Não adiciona retentativas.</summary>
public static class DFeHttpResilience
{
    /// <summary>Cria um pipeline por perfil de conexão, com timeout por tentativa e circuit breaker opcional.</summary>
    public static ResiliencePipeline<HttpResponseMessage> CreatePipeline(DFeHttpTransportOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>();
        if (options.CircuitBreakerHabilitado)
            pipeline.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>().Handle<TimeoutRejectedException>()
                    .HandleResult(response => (int)response.StatusCode == 408 || (int)response.StatusCode == 429
                        || (int)response.StatusCode >= 500),
                FailureRatio = 0.1,
                MinimumThroughput = 100,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(5)
            });
        pipeline.AddTimeout(options.TimeoutPerAttempt ? options.Timeout : options.TimeoutTentativa);
        return pipeline.Build();
    }
}
