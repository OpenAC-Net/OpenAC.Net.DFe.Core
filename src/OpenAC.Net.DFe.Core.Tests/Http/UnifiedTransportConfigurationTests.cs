using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using OpenAC.Net.DFe.Core.Http;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace OpenAC.Net.DFe.Core.Tests.Http;

public class UnifiedTransportConfigurationTests
{
    [Test]
    public async Task OperationTimeoutAndLegacyTimeoutShareOneValue()
    {
        var config = new HttpTestConfiguration();
        config.WebServices.TimeOut = TimeSpan.FromSeconds(12);
        await Assert.That(config.WebServices.TimeoutOperacao).IsEqualTo(TimeSpan.FromSeconds(12));
        config.WebServices.TimeoutOperacao = TimeSpan.FromSeconds(23);
        await Assert.That(config.WebServices.TimeOut).IsEqualTo(TimeSpan.FromSeconds(23));
    }

    [Test]
    public async Task TransportCapturesPoliciesAndIncludesThemInPoolIdentity()
    {
        var config = new HttpTestConfiguration();
        var first = new DFeHttpTransportOptions(config.WebServices);
        config.WebServices.TimeoutTentativa = TimeSpan.FromSeconds(3);
        config.WebServices.TempoVidaHandler = TimeSpan.FromMinutes(7);
        config.WebServices.CircuitBreakerHabilitado = true;
        var second = new DFeHttpTransportOptions(config.WebServices);
        await Assert.That(first.TimeoutTentativa).IsEqualTo(TimeSpan.FromSeconds(60));
        await Assert.That(second.TimeoutTentativa).IsEqualTo(TimeSpan.FromSeconds(3));
        await Assert.That(second.TempoVidaHandler).IsEqualTo(TimeSpan.FromMinutes(7));
        var endpoint = new Uri("https://example.test/status");
        await Assert.That(first.GetPoolKey(endpoint)).IsNotEqualTo(second.GetPoolKey(endpoint));
    }

    [Test]
    public async Task SharedPipelineTimesOutOneAttemptWithoutRetrying()
    {
        var config = new HttpTestConfiguration();
        config.WebServices.TimeoutTentativa = TimeSpan.FromMilliseconds(70);
        var pipeline = DFeHttpResilience.CreatePipeline(new(config.WebServices));
        var calls = 0;
        await Assert.That(async () => await pipeline.ExecuteAsync(async token =>
        {
            calls++;
            await Task.Delay(Timeout.Infinite, token);
            return new HttpResponseMessage(HttpStatusCode.OK);
        })).ThrowsExactly<TimeoutRejectedException>();
        await Assert.That(calls).IsEqualTo(1);
    }

    [Test]
    public async Task SharedBreakerOpensAndDoesNotAffectAnotherProfile()
    {
        var config = new HttpTestConfiguration();
        config.WebServices.CircuitBreakerHabilitado = true;
        var pipeline = DFeHttpResilience.CreatePipeline(new(config.WebServices));
        for (var i = 0; i < 100; i++)
        {
            try { await pipeline.ExecuteAsync(_ => ValueTask.FromException<HttpResponseMessage>(new HttpRequestException())); }
            catch (HttpRequestException) { }
        }
        await Assert.That(async () => await pipeline.ExecuteAsync(_ => new ValueTask<HttpResponseMessage>(new HttpResponseMessage(HttpStatusCode.OK))))
            .Throws<BrokenCircuitException>();
        var isolated = DFeHttpResilience.CreatePipeline(new(config.WebServices));
        using var response = await isolated.ExecuteAsync(_ => new ValueTask<HttpResponseMessage>(new HttpResponseMessage(HttpStatusCode.OK)));
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task CertificateMarginIsAnExplicitPartOfCertificateConfiguration()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=unified-config", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(2));
        var config = new HttpTestConfiguration();
        await Assert.That(() => config.Certificados.ValidarCertificado(certificate)).ThrowsExactly<InvalidOperationException>();
        config.Certificados.MargemValidadeCertificado = TimeSpan.Zero;
        config.Certificados.ValidarCertificado(certificate);
        await Assert.That(certificate.HasPrivateKey).IsTrue();
    }
}
