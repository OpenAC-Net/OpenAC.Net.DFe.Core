using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using OpenAC.Net.DFe.Core.Http;
using OpenAC.Net.DFe.Core.Service;
using OpenAC.Net.DFe.Core.Storage;

namespace OpenAC.Net.DFe.Core.Tests.Http;

public class DFeSoapServiceClientTests
{
    [Test]
    [Arguments(false, false)]
    [Arguments(true, false)]
    [Arguments(false, true)]
    [Arguments(true, true)]
    public async Task SyncAndAsyncUseTheSameSoapContractAndPersistence(bool soap12, bool sync)
    {
        var config = new HttpTestConfiguration();
        var transport = new SoapTransport(soap12);
        var store = new MemoryStore();
        config.HttpTransport = transport;
        config.DocumentStore = store;
        config.WebServices.Salvar = true;
        using var client = new SoapClient(config, soap12);
        var result = sync ? client.SendSync() : await client.SendAsync();
        await Assert.That(XElement.Parse(result).Value).IsEqualTo("ação autorizada");
        var envelope = XElement.Parse(transport.Envelope);
        await Assert.That(envelope.Name.NamespaceName).IsEqualTo(soap12
            ? "http://www.w3.org/2003/05/soap-envelope" : "http://schemas.xmlsoap.org/soap/envelope/");
        await Assert.That(envelope.Element(envelope.Name.Namespace + "Header")!.Value).IsEqualTo("empresa");
        await Assert.That(transport.ContentType).Contains(soap12 ? "application/soap+xml" : "text/xml");
        await Assert.That(soap12 ? transport.ContentType : transport.Action).Contains("urn:consulta");
        await Assert.That(store.Documents.Count).IsEqualTo(2);
        await Assert.That(store.Documents[0].Conteudo).IsEqualTo(client.EnvelopeEnvio);
        await Assert.That(store.Documents[1].Conteudo).IsEqualTo(client.EnvelopeRetorno);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task SoapFaultOnHttp500KeepsItsReasonAndPersistsBothEnvelopes(bool soap12)
    {
        var config = new HttpTestConfiguration();
        var store = new MemoryStore();
        config.WebServices.Salvar = true;
        config.DocumentStore = store;
        config.HttpTransport = new SoapTransport(soap12) { Fault = true };
        using var client = new SoapClient(config, soap12);
        var exception = await Assert.ThrowsExactlyAsync<OpenDFeCommunicationException>(() => client.SendAsync());
        await Assert.That(exception!.Message).Contains("Rejeição de teste");
        await Assert.That(client.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        await Assert.That(store.Documents.Count).IsEqualTo(2);
        await Assert.That(store.Documents[1].Conteudo).Contains("Rejeição de teste");
    }

    [Test]
    public async Task AsyncSoapDoesNotBlockAndAcceptsCancellation()
    {
        var config = new HttpTestConfiguration();
        var transport = new SoapTransport(true) { Wait = true };
        config.HttpTransport = transport;
        using var client = new SoapClient(config, true);
        using var cancellation = new CancellationTokenSource();
        var pending = client.SendAsync(cancellation.Token);
        await transport.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(pending.IsCompleted).IsFalse();
        cancellation.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => pending);
    }

    private sealed class SoapClient(HttpTestConfiguration config, bool soap12) :
        DFeSoapServiceClient<HttpTestConfiguration, HttpTestGeneral, HttpTestWebServices,
            HttpTestCertificates, HttpTestFiles>(config, "https://servico.test/", soap12 ? SoapVersion.Soap12 : SoapVersion.Soap11)
    {
        protected override X509Certificate2? Certificado => null;
        internal Task<string> SendAsync(CancellationToken token = default) =>
            ExecuteAsync("urn:consulta", "<consulta/>", "<empresa>empresa</empresa>", token, "xmlns:dfe=\"urn:dfe\"");
        internal string SendSync() => Execute("urn:consulta", "<consulta/>", "<empresa>empresa</empresa>", "xmlns:dfe=\"urn:dfe\"");
        protected override string TratarRetorno(XElement body) => body.Elements().Single().ToString();
    }

    private sealed class SoapTransport(bool soap12) : IDFeHttpTransport
    {
        internal bool Fault, Wait;
        internal string Envelope = "", ContentType = "", Action = "";
        internal readonly TaskCompletionSource<bool> Started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, DFeHttpTransportOptions options,
            CancellationToken cancellationToken = default)
        {
            Envelope = await request.Content!.ReadAsStringAsync(cancellationToken);
            ContentType = request.Content.Headers.ContentType!.ToString();
            Action = request.Headers.TryGetValues("SOAPAction", out var values) ? values.Single() : "";
            Started.TrySetResult(true);
            if (Wait) await Task.Delay(Timeout.Infinite, cancellationToken);
            var soap = soap12 ? "http://www.w3.org/2003/05/soap-envelope" : "http://schemas.xmlsoap.org/soap/envelope/";
            var body = Fault ? "<s:Fault><faultstring>Rejeição de teste</faultstring></s:Fault>" : "<resultado>ação autorizada</resultado>";
            return new HttpResponseMessage(Fault ? HttpStatusCode.InternalServerError : HttpStatusCode.OK)
            { Content = new StringContent($"<s:Envelope xmlns:s=\"{soap}\"><s:Body>{body}</s:Body></s:Envelope>") };
        }
    }

    private sealed class MemoryStore : IDFeDocumentStore
    {
        internal List<IDFeDocument> Documents { get; } = [];
        public Task SaveAsync(IDFeDocument document, CancellationToken cancellationToken = default)
        { cancellationToken.ThrowIfCancellationRequested(); Documents.Add(document); return Task.CompletedTask; }
    }
}
