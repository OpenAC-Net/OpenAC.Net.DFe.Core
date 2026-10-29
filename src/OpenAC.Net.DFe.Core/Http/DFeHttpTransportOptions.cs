using System;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using OpenAC.Net.DFe.Core.Common;

namespace OpenAC.Net.DFe.Core.Http;

/// <summary>Configuração de uma operação HTTP, independente do modelo de documento fiscal.</summary>
public sealed class DFeHttpTransportOptions
{
    /// <summary>Captura as configurações de conexão e retentativas da operação.</summary>
    public DFeHttpTransportOptions(DFeWebserviceConfigBase configuration, X509Certificate2? certificate = null,
        bool validateServerCertificate = true)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        Certificate = certificate;
        Protocols = (SslProtocols)configuration.Protocolos;
        Proxy = configuration.Proxy ?? string.Empty;
        ValidateServerCertificate = validateServerCertificate;
        Attempts = Math.Max(1, configuration.Tentativas);
        RetryDelay = TimeSpan.FromMilliseconds(Math.Max(0, configuration.IntervaloTentativas));
        Timeout = configuration.TimeOut ?? TimeSpan.FromSeconds(100);
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

    /// <summary>Limite total da operação, incluindo retentativas e leitura da resposta.</summary>
    public TimeSpan Timeout { get; }
}