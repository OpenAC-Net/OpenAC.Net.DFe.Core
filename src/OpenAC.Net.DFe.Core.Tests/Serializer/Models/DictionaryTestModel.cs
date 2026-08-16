using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Document;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Tests.Serializer.Models;

[DFeRoot("DictionaryRoot")]
public partial class DictionaryTestModel : DFeDocument<DictionaryTestModel>
{
    [DFeDictionary("enderecos")]
    [DFeDictionaryKey(TipoCampo.Enum, "Tipo", AsAttribute = true)]
    [DFeDictionaryValue(TipoCampo.Str, "Endereco")]
    public Dictionary<StatusOperacao, string> EnderecosPorStatus { get; set; } = new();

    [DFeDictionary("parametros")]
    [DFeDictionaryKey(TipoCampo.Str, "Chave", AsAttribute = false)]
    [DFeDictionaryValue(TipoCampo.Str, "Valor")]
    public Dictionary<string, string> Parametros { get; set; } = new();
}
