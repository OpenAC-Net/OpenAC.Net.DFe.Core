namespace OpenAC.Net.DFe.Generator.Models;

/// <summary>
/// Classificação da propriedade para o gerador de código DFe.
/// </summary>
public enum DFePropertyKind
{
    /// <summary>
    /// Elemento filho XML padrão.
    /// </summary>
    Element = 0,

    /// <summary>
    /// Atributo XML.
    /// </summary>
    Attribute = 1,

    /// <summary>
    /// Lista ou coleção de elementos.
    /// </summary>
    Collection = 2,

    /// <summary>
    /// Dicionário chave-valor.
    /// </summary>
    Dictionary = 3,

    /// <summary>
    /// Elemento de valor simples com atributos.
    /// </summary>
    ValueElement = 4,

    /// <summary>
    /// Valor textual simples dentro de elemento com atributos.
    /// </summary>
    ItemValue = 5,

    /// <summary>
    /// Interface ou classe abstrata com mapeamento polimórfico.
    /// </summary>
    Interface = 6,

    /// <summary>
    /// Nenhum mapeamento especial.
    /// </summary>
    None = 7
}

/// <summary>
/// Classificação do tipo CLR da propriedade no modelo do gerador.
/// </summary>
public enum DFeTypeKind
{
    /// <summary>
    /// Tipo primitivo do .NET (string, int, decimal, DateTime, etc.).
    /// </summary>
    Primitive = 0,

    /// <summary>
    /// Tipo enumeração.
    /// </summary>
    Enum = 1,

    /// <summary>
    /// Classe complexa com propriedades.
    /// </summary>
    Class = 2,

    /// <summary>
    /// Classe raiz com atributo DFeRoot.
    /// </summary>
    RootClass = 3,

    /// <summary>
    /// Coleção ou lista genérica.
    /// </summary>
    Collection = 4,

    /// <summary>
    /// Dicionário genérico.
    /// </summary>
    Dictionary = 5,

    /// <summary>
    /// Elemento de valor simples.
    /// </summary>
    ValueElement = 6,

    /// <summary>
    /// Interface ou classe abstrata.
    /// </summary>
    InterfaceOrAbstract = 7,

    /// <summary>
    /// Stream de dados.
    /// </summary>
    Stream = 8,

    /// <summary>
    /// Outro tipo.
    /// </summary>
    Other = 9
}

/// <summary>
/// Modelo correspondente à enumeração TipoCampo para uso sem dependência direta do Core durante a análise do gerador.
/// </summary>
public enum TipoCampoModel
{
    Str = 0,
    Int = 1,
    Dat = 2,
    DatHor = 3,
    DatHorTz = 4,
    StrNumber = 5,
    StrNumberFill = 6,
    De2 = 7,
    De3 = 8,
    De4 = 9,
    De10 = 10,
    Hor = 11,
    De6 = 12,
    DatCFe = 13,
    HorCFe = 14,
    Enum = 15,
    Custom = 16,
    Long = 17
}

/// <summary>
/// Modelo correspondente à enumeração Ocorrencia para uso no gerador de código.
/// </summary>
public enum OcorrenciaModel
{
    NaoObrigatoria = 0,
    Obrigatoria = 1,
    MaiorQueZero = 2
}
