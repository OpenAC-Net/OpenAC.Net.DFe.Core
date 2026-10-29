using System.Collections.Concurrent;
using System.Net;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;
using OpenAC.Net.DFe.Core.Http;

namespace OpenAC.Net.DFe.Core.Tests.Http;

public class DFeHttpTransportTests
{
    [Test]
    public async Task SafeRequestRetriesAndPreservesContentAndHeaders()
    {
        using var server = new LocalServer(index => index == 1 ? 503 : 200);
        var config = new HttpTestConfiguration();
        using var request = new HttpRequestMessage(HttpMethod.Get, server.Url) { Content = new StringContent("consulta") };
        request.Headers.Add("X-Test", "preservado");
        request.Content.Headers.Add("X-Content", "preservado");
        using var response = await DFeHttpTransport.Instance.SendAsync(request, new(config.WebServices));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(server.Requests.Count).IsEqualTo(2);
        foreach (var raw in server.Requests)
        {
            await Assert.That(raw.Contains("X-Test: preservado")).IsTrue();
            await Assert.That(raw.Contains("X-Content: preservado")).IsTrue();
            await Assert.That(raw.EndsWith("consulta")).IsTrue();
        }
    }

    [Test]
    [Arguments("POST")]
    [Arguments("PUT")]
    [Arguments("PATCH")]
    [Arguments("DELETE")]
    public async Task UnsafeMethodsAreSentOnlyOnce(string method)
    {
        using var server = new LocalServer(_ => 503);
        var config = new HttpTestConfiguration();
        using var request = new HttpRequestMessage(new HttpMethod(method), server.Url) { Content = new StringContent("emissao") };
        using var response = await DFeHttpTransport.Instance.SendAsync(request, new(config.WebServices));
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.ServiceUnavailable);
        await Assert.That(server.Requests.Count).IsEqualTo(1);
        await Assert.That(await response.Content.ReadAsStringAsync()).IsEqualTo("retorno");
    }

    [Test]
    public async Task CancellationInterruptsRetryDelay()
    {
        using var server = new LocalServer(_ => 503);
        var config = new HttpTestConfiguration();
        config.WebServices.IntervaloTentativas = 10000;
        using var cancellation = new CancellationTokenSource();
        using var request = new HttpRequestMessage(HttpMethod.Get, server.Url);
        var operation = DFeHttpTransport.Instance.SendAsync(request, new(config.WebServices), cancellation.Token);
        await server.FirstRequest.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();
        await Assert.That(async () => await operation).Throws<OperationCanceledException>();
        await Assert.That(server.Requests.Count).IsEqualTo(1);
    }

    [Test]
    public async Task CookiesAreNotSharedBetweenOperations()
    {
        using var server = new LocalServer(_ => 200);
        var config = new HttpTestConfiguration();
        for (var index = 0; index < 2; index++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, server.Url);
            using var response = await DFeHttpTransport.Instance.SendAsync(request, new(config.WebServices));
        }
        await Assert.That(server.Requests.Count).IsEqualTo(2);
        await Assert.That(server.Requests.Any(raw => raw.Contains("Cookie:", StringComparison.OrdinalIgnoreCase))).IsFalse();
    }

    [Test]
    public async Task TimeoutUsesTheWholeTimeSpanAndDoesNotChangeGlobalTls()
    {
        using var server = new LocalServer(_ => 200, TimeSpan.FromMilliseconds(1100));
        var config = new HttpTestConfiguration();
        config.WebServices.TimeOut = TimeSpan.FromMilliseconds(1800);
        var originalProtocols = ServicePointManager.SecurityProtocol;
        using var request = new HttpRequestMessage(HttpMethod.Get, server.Url);
        using var response = await DFeHttpTransport.Instance.SendAsync(request, new(config.WebServices));
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(ServicePointManager.SecurityProtocol).IsEqualTo(originalProtocols);
    }

    [Test]
    public async Task TimeoutCancelsResponseRead()
    {
        using var server = new LocalServer(_ => 200, TimeSpan.FromSeconds(2));
        var config = new HttpTestConfiguration();
        config.WebServices.TimeOut = TimeSpan.FromMilliseconds(150);
        using var request = new HttpRequestMessage(HttpMethod.Get, server.Url);
        await Assert.That(async () => await DFeHttpTransport.Instance.SendAsync(request, new(config.WebServices)))
            .Throws<OperationCanceledException>();
    }

    [Test]
    public async Task CompatibilityOptionsPreserveCookiesWithoutSharingThemWithDefaultProfile()
    {
        using var server = new LocalServer(_ => 200);
        var config = new HttpTestConfiguration();
        var options = new DFeHttpTransportOptions(config.WebServices) { UseCookies = true };
        for (var index = 0; index < 2; index++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, server.Url);
            using var response = await DFeHttpTransport.Instance.SendAsync(request, options);
        }
        using var isolatedRequest = new HttpRequestMessage(HttpMethod.Get, server.Url);
        using var isolatedResponse = await DFeHttpTransport.Instance.SendAsync(isolatedRequest, new(config.WebServices));
        var requests = server.Requests.ToArray();
        await Assert.That(requests[1]).Contains("Cookie: tenant=anterior");
        await Assert.That(requests[2].Contains("Cookie:", StringComparison.OrdinalIgnoreCase)).IsFalse();
    }

    [Test]
    public async Task PerAttemptTimeoutDoesNotLimitRetryDelayOrWholeOperation()
    {
        using var server = new LocalServer(index => index < 3 ? 503 : 200);
        var config = new HttpTestConfiguration();
        config.WebServices.TimeOut = TimeSpan.FromMilliseconds(500);
        config.WebServices.IntervaloTentativas = 600;
        var options = new DFeHttpTransportOptions(config.WebServices) { TimeoutPerAttempt = true };
        using var request = new HttpRequestMessage(HttpMethod.Get, server.Url);
        using var response = await DFeHttpTransport.Instance.SendAsync(request, options);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(server.Requests.Count).IsEqualTo(3);
    }

    [Test]
    public async Task BufferedPostPreservesLengthAndIsNeverRetried()
    {
        using var server = new LocalServer(_ => 503);
        var config = new HttpTestConfiguration();
        using var request = new HttpRequestMessage(HttpMethod.Post, server.Url) { Content = new UnknownLengthContent() };
        var options = new DFeHttpTransportOptions(config.WebServices) { BufferRequestContent = true };
        using var response = await DFeHttpTransport.Instance.SendAsync(request, options);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.ServiceUnavailable);
        await Assert.That(server.Requests.Count).IsEqualTo(1);
        await Assert.That(server.Requests.Single()).Contains("Content-Length: 7");
        await Assert.That(server.Requests.Single().EndsWith("emissao")).IsTrue();
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task DecompressionCanPreserveOriginalCompressedResponse(bool decompress)
    {
        using var server = new LocalServer(_ => 200, gzip: true);
        var config = new HttpTestConfiguration();
        var options = new DFeHttpTransportOptions(config.WebServices)
        {
            AutomaticDecompression = decompress ? DecompressionMethods.GZip : DecompressionMethods.None
        };
        using var request = new HttpRequestMessage(HttpMethod.Get, server.Url);
        using var response = await DFeHttpTransport.Instance.SendAsync(request, options);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        if (decompress)
        {
            await Assert.That(Encoding.ASCII.GetString(bytes)).IsEqualTo("retorno");
            await Assert.That(response.Content.Headers.ContentEncoding.Count).IsEqualTo(0);
        }
        else
        {
            await Assert.That(bytes[0]).IsEqualTo((byte)0x1f);
            await Assert.That(bytes[1]).IsEqualTo((byte)0x8b);
            await Assert.That(response.Content.Headers.ContentEncoding.Single()).IsEqualTo("gzip");
        }
    }

    [Test]
    public async Task ComponentCanKeepSingleAttemptForSafeMethods()
    {
        using var server = new LocalServer(_ => 503);
        var config = new HttpTestConfiguration();
        using var request = new HttpRequestMessage(HttpMethod.Get, server.Url);
        using var response = await new SingleAttemptTransport().SendAsync(request, new(config.WebServices));
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.ServiceUnavailable);
        await Assert.That(server.Requests.Count).IsEqualTo(1);
    }

    private sealed class SingleAttemptTransport : DFeHttpTransport
    {
        protected override bool ManageRetries => false;
    }

    private sealed class UnknownLengthContent : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            stream.WriteAsync(Encoding.ASCII.GetBytes("emissao")).AsTask();
        protected override bool TryComputeLength(out long length) { length = 0; return false; }
    }

    private sealed class LocalServer : IDisposable
    {
        private readonly TcpListener listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(15));
        private readonly Task worker;
        internal ConcurrentQueue<string> Requests { get; } = new();
        internal TaskCompletionSource FirstRequest { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal Uri Url { get; }

        internal LocalServer(Func<int, int> status, TimeSpan delay = default, bool gzip = false)
        {
            listener.Start();
            Url = new Uri($"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/");
            worker = Serve(status, delay, gzip);
        }

        private async Task Serve(Func<int, int> status, TimeSpan delay, bool gzip)
        {
            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    using var client = await listener.AcceptTcpClientAsync(cancellation.Token);
                    using var stream = client.GetStream();
                    using var reader = new StreamReader(stream, Encoding.UTF8, false, leaveOpen: true);
                    var raw = new StringBuilder();
                    var length = 0;
                    while (await reader.ReadLineAsync(cancellation.Token) is { Length: > 0 } line)
                    {
                        raw.AppendLine(line);
                        if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                            length = int.Parse(line.Substring("Content-Length:".Length));
                    }
                    if (length > 0)
                    {
                        var body = new char[length];
                        await reader.ReadBlockAsync(body, cancellation.Token);
                        raw.Append(body);
                    }
                    Requests.Enqueue(raw.ToString());
                    FirstRequest.TrySetResult();
                    await Task.Delay(delay, cancellation.Token);
                    var payload = Encoding.ASCII.GetBytes("retorno");
                    if (gzip)
                    {
                        using var compressed = new MemoryStream();
                        using (var compressor = new GZipStream(compressed, CompressionMode.Compress, true))
                            compressor.Write(payload);
                        payload = compressed.ToArray();
                    }
                    var encodingHeader = gzip ? "Content-Encoding: gzip\r\n" : string.Empty;
                    var headers = Encoding.ASCII.GetBytes($"HTTP/1.1 {status(Requests.Count)} Result\r\nContent-Length: {payload.Length}\r\n{encodingHeader}Connection: close\r\nSet-Cookie: tenant=anterior\r\n\r\n");
                    await stream.WriteAsync(headers, cancellation.Token);
                    await stream.WriteAsync(payload, cancellation.Token);
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
            catch (IOException) when (cancellation.IsCancellationRequested) { }
            catch (SocketException) when (cancellation.IsCancellationRequested) { }
        }

        public void Dispose()
        {
            cancellation.Cancel();
            listener.Stop();
            worker.GetAwaiter().GetResult();
            cancellation.Dispose();
        }
    }
}
