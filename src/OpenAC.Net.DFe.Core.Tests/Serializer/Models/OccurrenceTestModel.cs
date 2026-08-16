using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Document;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Tests.Serializer.Models;

[DFeRoot("OccurrenceRoot")]
public partial class OccurrenceTestModel : DFeDocument<OccurrenceTestModel>
{
    [DFeElement(TipoCampo.Str, "campoObrigatorio", Ocorrencia = Ocorrencia.Obrigatoria)]
    public string CampoObrigatorio { get; set; } = string.Empty;

    [DFeElement(TipoCampo.Str, "campoOpcional", Ocorrencia = Ocorrencia.NaoObrigatoria)]
    public string? CampoOpcional { get; set; }

    [DFeElement(TipoCampo.De2, "campoMaiorQueZero", Ocorrencia = Ocorrencia.MaiorQueZero)]
    public decimal CampoMaiorQueZero { get; set; }

    [DFeElement(TipoCampo.Int, "campoCondicional")]
    public int CampoCondicional { get; set; }

    public bool ShouldSerializeCampoCondicional() => CampoCondicional > 10;
}
