using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using OpenAC.Net.DFe.Core.Http;
using OpenAC.Net.DFe.Core.Service;
using OpenAC.Net.DFe.Core.Storage;

namespace OpenAC.Net.DFe.Core.Tests.Http;

public class DFeServiceClientTests
{
    [Test]
    public async Task HttpErrorsKeepStatusBodyAndPersistThroughConfiguredStore()
    {
        var config = new HttpTestConfiguration();
        var store = new MemoryStore();
        config.DocumentStore = store;
        config.WebServices.Salvar = true;
        config.Arquivos.AdicionarLiteral = true;
        config.HttpTransport = new TestTransport((_, _, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("rejeição fiscal")
        }));
        using var client = new TestClient(config);
        await Assert.That(async () => await client.Send("<envio/>" )).Throws<OpenDFeCommunicationException>();
        await Assert.That(client.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(client.EnvelopeRetorno).IsEqualTo("rejeição fiscal");
        await Assert.That(store.Documents.Count).IsEqualTo(2);
        await Assert.That(Path.GetFileName(Path.GetDirectoryName(store.Documents[0].Caminho))).IsEqualTo("SolicitacaoTecnica");
        await Assert.That(Path.GetFileName(Path.GetDirectoryName(store.Documents[1].Caminho))).IsEqualTo("RespostaTecnica");
        await Assert.That(store.Documents[1].Conteudo).IsEqualTo("rejeição fiscal");
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task DecodesAccentsEvenWhenCharsetIsWrongOrMissing(bool utf8)
    {
        var config = new HttpTestConfiguration();
        var bytes = (utf8 ? Encoding.UTF8 : Encoding.Latin1).GetBytes("ação e emissão");
        config.HttpTransport = new TestTransport((_, _, _) =>
        {
            var content = new ByteArrayContent(bytes);
            content.Headers.TryAddWithoutValidation("Content-Type", utf8 ? "text/xml; charset=iso-8859-1" : "text/xml");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        });
        using var client = new TestClient(config);
        await client.Send("consulta");
        await Assert.That(client.EnvelopeRetorno).IsEqualTo("ação e emissão");
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task RecognizesUnicodeBomBeforeUnreliableCharset(bool utf32)
    {
        var config = new HttpTestConfiguration();
        var encoding = utf32 ? Encoding.UTF32 : Encoding.Unicode;
        var bytes = encoding.GetPreamble().Concat(encoding.GetBytes("ação e emissão")).ToArray();
        config.HttpTransport = new TestTransport((_, _, _) =>
        {
            var content = new ByteArrayContent(bytes);
            content.Headers.TryAddWithoutValidation("Content-Type", "text/xml; charset=iso-8859-1");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        });
        using var client = new TestClient(config);
        await client.Send("consulta");
        await Assert.That(client.EnvelopeRetorno).IsEqualTo("ação e emissão");
    }

    [Test]
    public async Task CancellationFlowsThroughTransportWithoutCommunicationWrapper()
    {
        var config = new HttpTestConfiguration();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        config.HttpTransport = new TestTransport(async (_, _, token) =>
        {
            started.SetResult();
            await Task.Delay(Timeout.Infinite, token);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var client = new TestClient(config);
        using var cancellation = new CancellationTokenSource();
        var operation = client.Send("consulta", cancellation.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();
        await Assert.That(async () => await operation).Throws<OperationCanceledException>();
    }

    [Test]
    public async Task OperationTokenCancelsSynchronousCompatibilityEntryPoint()
    {
        var config = new HttpTestConfiguration();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        config.OperationToken.Value = cancellation.Token;
        using var client = new TestClient(config);
        await Assert.That(() => client.SendSync()).Throws<OperationCanceledException>();
    }

    private sealed class TestClient(HttpTestConfiguration config) : DFeServiceClientBase<HttpTestConfiguration,
        HttpTestGeneral, HttpTestWebServices, HttpTestCertificates, HttpTestFiles>(config, "https://servico.test/")
    {
        protected override X509Certificate2? Certificado => null;
        internal Task Send(string body, CancellationToken token = default)
        {
            EnvelopeEnvio = body;
            return ExecuteAsync("application/xml; charset=utf-8", "POST", cancellationToken: token);
        }
        internal void SendSync() => Execute("application/xml", "GET");
    }

    private sealed class TestTransport(Func<HttpRequestMessage, DFeHttpTransportOptions, CancellationToken,
        Task<HttpResponseMessage>> send) : IDFeHttpTransport
    {
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, DFeHttpTransportOptions options,
            CancellationToken cancellationToken = default) => send(request, options, cancellationToken);
    }

    private sealed class MemoryStore : IDFeDocumentStore
    {
        internal List<IDFeDocument> Documents { get; } = new();
        public Task SaveAsync(IDFeDocument document,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Documents.Add(document);
            return Task.CompletedTask;
        }
    }
}
