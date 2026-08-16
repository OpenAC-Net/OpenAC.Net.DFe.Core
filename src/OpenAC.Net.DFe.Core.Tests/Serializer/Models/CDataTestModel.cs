using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Document;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Tests.Serializer.Models;

[DFeRoot("CDataRoot")]
public partial class CDataTestModel : DFeDocument<CDataTestModel>
{
    [DFeElement(TipoCampo.Str, "conteudoHtml", UseCData = true)]
    public string ConteudoHtml { get; set; } = string.Empty;

    [DFeElement(TipoCampo.Str, "conteudoXml", UseCData = true)]
    public string ConteudoXml { get; set; } = string.Empty;

    [DFeElement(TipoCampo.Str, "textoNormal", UseCData = false)]
    public string TextoNormal { get; set; } = string.Empty;
}
