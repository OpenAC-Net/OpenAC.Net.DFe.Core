using System.Net;
using System.Text;
using OpenAC.Net.DFe.Core.Http;

namespace OpenAC.Net.DFe.Core.Tests.Http;

public class DFeHttpResponseTests
{
    [Test]
    public async Task HostResponseBodyRemainsWithinTheTransportTimeout()
    {
        var content = new SlowContent();
        var transport = new HostTransport(content);
        var config = new HttpTestConfiguration();
        config.WebServices.TimeOut = TimeSpan.FromMilliseconds(100);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://servico.test/");
        await Assert.That(async () => await transport.SendAsync(request, new(config.WebServices)))
            .Throws<OperationCanceledException>();
        await Assert.That(content.Disposed).IsTrue();
    }

    [Test]
    public async Task BufferPreservesStatusContentHeadersAndCanBeReadRepeatedly()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("rejeição", Encoding.UTF8, "application/xml")
        };
        response.Content.Headers.Add("X-Original", "mantido");
        await DFeHttpResponse.BufferAsync(response);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(response.Content.Headers.GetValues("X-Original").Single()).IsEqualTo("mantido");
        await Assert.That(response.Content.Headers.ContentType!.MediaType).IsEqualTo("application/xml");
        await Assert.That(await DFeHttpResponse.ReadAsStringAsync(response.Content)).IsEqualTo("rejeição");
        await Assert.That(await DFeHttpResponse.ReadAsStringAsync(response.Content)).IsEqualTo("rejeição");
    }

    [Test]
    public async Task Latin1AndInvalidCharsetAreHandledWithoutLosingAccents()
    {
        var bytes = Encoding.Latin1.GetBytes("ação e emissão");
        await Assert.That(DFeHttpResponse.Decode(bytes, "charset-inexistente")).IsEqualTo("ação e emissão");
        await Assert.That(DFeHttpResponse.Decode(Encoding.UTF8.GetBytes("ação"), "ISO-8859-1")).IsEqualTo("ação");
    }

    [Test]
    public async Task BufferedRequestPreservesHostTenantContext()
    {
        var config = new HttpTestConfiguration();
        var transport = new HostTransport(new StringContent("ok"));
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://servico.test/")
        {
            Content = new StringContent("xml")
        };
        request.Options.Set(new HttpRequestOptionsKey<string>("tenant"), "empresa-123");
        using var response = await transport.SendAsync(request,
            new DFeHttpTransportOptions(config.WebServices) { BufferRequestContent = true });
        await Assert.That(transport.CapturedTenant).IsEqualTo("empresa-123");
        await Assert.That(response.RequestMessage).IsSameReferenceAs(request);
    }

    private sealed class HostTransport(HttpContent content) : DFeHttpTransportBase
    {
        internal string? CapturedTenant { get; private set; }
        protected override bool ManageRetries => false;
        protected override Task<HttpResponseMessage> SendCoreAsync(HttpRequestMessage request,
            DFeHttpTransportOptions options, CancellationToken cancellationToken)
        {
            CapturedTenant = request.Options.TryGetValue(new HttpRequestOptionsKey<string>("tenant"), out var tenant) ? tenant : null;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        }
    }

    private sealed class SlowContent : HttpContent
    {
        internal bool Disposed { get; private set; }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            SerializeToStreamAsync(stream, context, CancellationToken.None);
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken) =>
            Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        protected override bool TryComputeLength(out long length) { length = 0; return false; }
        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
    }
}
