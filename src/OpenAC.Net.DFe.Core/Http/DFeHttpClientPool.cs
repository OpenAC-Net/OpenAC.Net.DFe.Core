using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace OpenAC.Net.DFe.Core.Http;

// Cada entrada possui o próprio certificado e permanece viva enquanto houver envios em andamento.
internal sealed class DFeHttpClientPool
{
    private readonly object sync = new();
    private readonly Dictionary<string, Entry> clients = new();
    internal static DFeHttpClientPool Instance { get; } = new();

    internal Lease Acquire(DFeHttpTransportOptions options, Uri endpoint)
    {
        var key =
            $"{options.Certificate?.Thumbprint}|{(int)options.Protocols}|{options.ValidateServerCertificate}|{options.Proxy}|{options.UseCookies}|{(int)options.AutomaticDecompression}|{endpoint.GetLeftPart(UriPartial.Authority)}";
        lock (sync)
        {
            var expired = new List<string>();
            foreach (var pair in clients)
            {
                if (pair.Value.Expires > DateTime.UtcNow) continue;
                pair.Value.Retired = true;
                if (pair.Value.Active == 0) pair.Value.Dispose();
                expired.Add(pair.Key);
            }

            foreach (var name in expired) clients.Remove(name);

            if (!clients.TryGetValue(key, out var entry))
            {
                entry = CreateEntry(options);
                clients.Add(key, entry);
            }

            entry.Active++;
            return new Lease(this, entry);
        }
    }

    private static Entry CreateEntry(DFeHttpTransportOptions options)
    {
        var certificate = options.Certificate == null ? null : new X509Certificate2(options.Certificate);
        var handler = new HttpClientHandler
        {
            UseCookies = options.UseCookies,
            AutomaticDecompression = options.AutomaticDecompression
        };
        try
        {
#if NETFRAMEWORK
            // As referências do .NET 4.6.2 não expõem o setter presente em runtimes mais recentes.
            typeof(HttpClientHandler).GetProperty("SslProtocols")?.SetValue(handler, options.Protocols);
#else
            handler.SslProtocols = options.Protocols;
#endif
            if (!options.ValidateServerCertificate)
                handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            if (certificate != null) handler.ClientCertificates.Add(certificate);
            if (!string.IsNullOrWhiteSpace(options.Proxy)) handler.Proxy = new WebProxy(options.Proxy, true);
            return new Entry(new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan }, certificate);
        }
        catch
        {
            handler.Dispose();
            certificate?.Dispose();
            throw;
        }
    }

    internal sealed class Entry : IDisposable
    {
        internal Entry(HttpClient client, X509Certificate2? certificate)
        {
            Client = client;
            Certificate = certificate;
            Expires = DateTime.UtcNow.AddMinutes(10);
            if (certificate != null && certificate.NotAfter.ToUniversalTime() < Expires)
                Expires = certificate.NotAfter.ToUniversalTime();
        }

        internal HttpClient Client { get; }
        private X509Certificate2? Certificate { get; }
        internal DateTime Expires { get; }
        internal int Active { get; set; }
        internal bool Retired { get; set; }

        public void Dispose()
        {
            Client.Dispose();
            Certificate?.Dispose();
        }
    }

    internal sealed class Lease : IDisposable
    {
        private readonly DFeHttpClientPool pool;
        private readonly Entry entry;
        private bool returned;

        internal Lease(DFeHttpClientPool pool, Entry entry)
        {
            this.pool = pool;
            this.entry = entry;
        }

        internal HttpClient Client => entry.Client;

        public void Dispose()
        {
            lock (pool.sync)
            {
                if (returned) return;
                returned = true;
                entry.Active--;
                if (entry.Retired && entry.Active == 0) entry.Dispose();
            }
        }
    }
}