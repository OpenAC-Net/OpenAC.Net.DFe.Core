using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Document;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Tests.Serializer.Models;

public enum StatusOperacao
{
    [DFeEnum("PEN")]
    Pendente = 0,

    [DFeEnum("APR")]
    Aprovado = 1,

    [DFeEnum("REJ")]
    Rejeitado = 2
}

public enum ModalidadeFrete
{
    SemFrete = 0,
    ContaEmitente = 1,
    ContaDestinatario = 2
}

[DFeRoot("EnumRoot")]
public partial class EnumTestModel : DFeDocument<EnumTestModel>
{
    [DFeElement(TipoCampo.Enum, "status")]
    public StatusOperacao Status { get; set; } = StatusOperacao.Pendente;

    [DFeElement(TipoCampo.Enum, "statusNullable", Ocorrencia = Ocorrencia.NaoObrigatoria)]
    public StatusOperacao? StatusNullable { get; set; }

    [DFeElement(TipoCampo.Enum, "modalidade")]
    public ModalidadeFrete Modalidade { get; set; } = ModalidadeFrete.SemFrete;

    [DFeElement(TipoCampo.Enum, "modalidadeNullable", Ocorrencia = Ocorrencia.NaoObrigatoria)]
    public ModalidadeFrete? ModalidadeNullable { get; set; }
}
