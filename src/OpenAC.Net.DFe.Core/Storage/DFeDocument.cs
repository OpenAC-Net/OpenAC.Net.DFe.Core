using System;

namespace OpenAC.Net.DFe.Core.Storage;

/// <summary>Conteúdo textual produzido por uma operação fiscal ou de comunicação.</summary>
public sealed class DFeDocument
{
    /// <summary>Inicializa um documento e os metadados usados para organizar sua persistência.</summary>
    public DFeDocument(DFeTipoDocumento tipo, string conteudo, string nomeArquivo,
        string? documentoContribuinte, DateTime data, bool preservarNomeArquivo = false)
    {
        if (string.IsNullOrWhiteSpace(nomeArquivo) || nomeArquivo == "." || nomeArquivo == ".." ||
            nomeArquivo.IndexOfAny(['/', '\\', ':']) >= 0)
            throw new ArgumentException("Informe apenas o nome do arquivo, sem diretórios.", nameof(nomeArquivo));
        Tipo = tipo;
        Conteudo = conteudo ?? throw new ArgumentNullException(nameof(conteudo));
        NomeArquivo = nomeArquivo;
        DocumentoContribuinte = documentoContribuinte;
        Data = data;
        PreservarNomeArquivo = preservarNomeArquivo;
    }

    /// <summary>Categoria do documento ou payload.</summary>
    public DFeTipoDocumento Tipo { get; }

    /// <summary>Conteúdo a persistir.</summary>
    public string Conteudo { get; }

    /// <summary>Nome sugerido, incluindo a extensão.</summary>
    public string NomeArquivo { get; }

    /// <summary>CPF ou CNPJ usado para organizar diretórios.</summary>
    public string? DocumentoContribuinte { get; }

    /// <summary>Data de referência do documento.</summary>
    public DateTime Data { get; }

    /// <summary>Quando verdadeiro, uma colisão gera erro em vez de acrescentar um sufixo.</summary>
    public bool PreservarNomeArquivo { get; }
}