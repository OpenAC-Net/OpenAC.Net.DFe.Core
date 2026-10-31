namespace OpenAC.Net.DFe.Core.Storage;

/// <summary>Contrato de um arquivo textual pronto para persistência.</summary>
public interface IDFeDocument
{
    /// <summary>Caminho completo do arquivo, incluindo nome e extensão.</summary>
    string Caminho { get; }

    /// <summary>Conteúdo textual a persistir.</summary>
    string Conteudo { get; }

    /// <summary>Define como tratar um arquivo existente no destino.</summary>
    DFeArquivoExiste ArquivoExiste { get; }
}
