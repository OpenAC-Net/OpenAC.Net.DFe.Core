using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Document;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Tests.Serializer.Models;

[DFeRoot("AttributeRoot", Namespace = "https://www.openac.net.br/teste")]
public partial class AttributeTestModel : DFeDocument<AttributeTestModel>
{
    [DFeAttribute(TipoCampo.Str, "versao")]
    public string Versao { get; set; } = "1.00";

    [DFeAttribute(TipoCampo.Int, "id", Min = 3)]
    public int Id { get; set; } = 1;

    [DFeAttribute(TipoCampo.Enum, "status")]
    public StatusOperacao Status { get; set; } = StatusOperacao.Aprovado;

    [DFeAttribute(TipoCampo.Str, "codigoOpcional", Ocorrencia = Ocorrencia.NaoObrigatoria)]
    public string? CodigoOpcional { get; set; }

    [DFeElement(TipoCampo.Str, "conteudo")]
    public string Conteudo { get; set; } = "Texto do elemento";

    public bool ShouldSerializeId() => Id > 0;
}
