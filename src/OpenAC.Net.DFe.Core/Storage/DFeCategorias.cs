namespace OpenAC.Net.DFe.Core.Storage;

/// <summary>Categorias padrão das mensagens de comunicação; componentes podem definir suas próprias constantes.</summary>
public static class DFeCategorias
{
    /// <summary>Payload enviado ao serviço.</summary>
    public const string SolicitacaoTecnica = "SolicitacaoTecnica";

    /// <summary>Payload recebido do serviço.</summary>
    public const string RespostaTecnica = "RespostaTecnica";

    /// <summary>Envelope SOAP enviado.</summary>
    public const string Solicitacao = "Solicitacao";

    /// <summary>Envelope SOAP recebido.</summary>
    public const string Resposta = "Resposta";
}
