namespace OpenAC.Net.DFe.Core.Storage;

/// <summary>Comportamentos de persistência quando o arquivo de destino já existe.</summary>
public enum DFeArquivoExiste
{
    /// <summary>Acrescenta sufixos numéricos ao nome até encontrar um destino disponível.</summary>
    AdicionarSufixo,

    /// <summary>Lança um erro sem alterar o arquivo existente.</summary>
    LancarErro,

    /// <summary>Substitui o conteúdo do arquivo existente.</summary>
    Sobrescrever
}
