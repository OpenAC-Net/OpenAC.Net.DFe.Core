namespace OpenAC.Net.DFe.Core.Storage;

/// <summary>Classifica documentos fiscais e mensagens técnicas dos componentes DFe.</summary>
public enum DFeTipoDocumento
{
    /// <summary>Payload enviado ao serviço.</summary>
    SolicitacaoTecnica,

    /// <summary>Payload recebido do serviço.</summary>
    RespostaTecnica,

    /// <summary>Envelope SOAP enviado.</summary>
    Solicitacao,

    /// <summary>Envelope SOAP recebido.</summary>
    Resposta,

    /// <summary>Documento fiscal eletrônico de outro modelo.</summary>
    DocumentoFiscal
}
