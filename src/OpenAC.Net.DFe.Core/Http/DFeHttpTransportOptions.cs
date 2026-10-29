using System;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using OpenAC.Net.DFe.Core.Common;

namespace OpenAC.Net.DFe.Core.Http;

/// <summary>Configuração de uma operação HTTP, independente do modelo de documento fiscal.</summary>
public sealed class DFeHttpTransportOptions
{
    /// <summary>Captura as configurações de conexão e retentativas da operação.</summary>
    public DFeHttpTransportOptions(DFeWebserviceConfigBase configuration, X509Certificate2? certificate = null,
        bool validateServerCertificate = true) : this(configuration, certificate, validateServerCertificate, null)
    {
    }

    /// <summary>Captura as opções com um prazo específico para o componente.</summary>
    public DFeHttpTransportOptions(DFeWebserviceConfigBase configuration, X509Certificate2? certificate,
        bool validateServerCertificate, TimeSpan? timeout)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        Certificate = certificate;
        Protocols = (SslProtocols)configuration.Protocolos;
        Proxy = configuration.Proxy ?? string.Empty;
        ValidateServerCertificate = validateServerCertificate;
        Attempts = Math.Max(1, configuration.Tentativas);
        RetryDelay = TimeSpan.FromMilliseconds(Math.Max(0, configuration.IntervaloTentativas));
        Timeout = timeout ?? configuration.TimeOut ?? TimeSpan.FromSeconds(100);
    }

    /// <summary>Certificado mTLS emprestado pelo chamador durante o envio.</summary>
    public X509Certificate2? Certificate { get; }

    /// <summary>Protocolos TLS permitidos para a conexão.</summary>
    public SslProtocols Protocols { get; }

    /// <summary>Endereço do proxy; vazio para usar o proxy padrão do sistema.</summary>
    public string Proxy { get; }

    /// <summary>Indica se a cadeia de confiança do servidor deve ser validada.</summary>
    public bool ValidateServerCertificate { get; }

    /// <summary>Número máximo de envios, incluindo o inicial, apenas para métodos seguros.</summary>
    public int Attempts { get; }

    /// <summary>Intervalo entre tentativas de métodos seguros.</summary>
    public TimeSpan RetryDelay { get; }

    /// <summary>Limite da operação ou de cada tentativa, conforme TimeoutPerAttempt.</summary>
    public TimeSpan Timeout { get; }

    /// <summary>Habilita cookies no cliente reutilizado. O padrão é desabilitado.</summary>
    public bool UseCookies { get; set; }

    /// <summary>Permite seguir redirecionamentos HTTP. O padrão é habilitado.</summary>
    public bool AllowAutoRedirect { get; set; } = true;

    /// <summary>Formatos descompactados automaticamente. O padrão é GZip e Deflate.</summary>
    public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.GZip | DecompressionMethods.Deflate;

    /// <summary>Aplica Timeout a cada envio, excluindo o intervalo entre tentativas. O padrão limita a operação inteira.</summary>
    public bool TimeoutPerAttempt { get; set; }

    /// <summary>Bufferiza e clona também requisições sem retentativas, preservando Content-Length de transportes anteriores.</summary>
    public bool BufferRequestContent { get; set; }
}
