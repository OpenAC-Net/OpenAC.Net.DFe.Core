using System;
using System.IO;

namespace OpenAC.Net.DFe.Core.Storage;

/// <summary>Implementação padrão de um arquivo textual pronto para persistência.</summary>
public sealed class DFeDocument : IDFeDocument
{
    /// <summary>Inicializa o documento com seu caminho completo, conteúdo e comportamento de gravação.</summary>
    public DFeDocument(string caminho, string conteudo,
        DFeArquivoExiste arquivoExiste = DFeArquivoExiste.AdicionarSufixo)
    {
        if (string.IsNullOrWhiteSpace(caminho) || !Path.IsPathRooted(caminho) ||
            string.IsNullOrWhiteSpace(Path.GetFileName(caminho)) ||
            Path.GetFileName(caminho) == "." || Path.GetFileName(caminho) == "..")
            throw new ArgumentException("Informe o caminho completo do arquivo, incluindo nome e extensão.",
                nameof(caminho));
        
        if (!Enum.IsDefined(typeof(DFeArquivoExiste), arquivoExiste))
            throw new ArgumentOutOfRangeException(nameof(arquivoExiste));

        Caminho = caminho;
        Conteudo = conteudo ?? throw new ArgumentNullException(nameof(conteudo));
        ArquivoExiste = arquivoExiste;
    }

    /// <inheritdoc />
    public string Caminho { get; }

    /// <inheritdoc />
    public string Conteudo { get; }

    /// <inheritdoc />
    public DFeArquivoExiste ArquivoExiste { get; }
}