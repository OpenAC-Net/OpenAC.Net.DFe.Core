using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Collection;
using OpenAC.Net.DFe.Core.Document;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Tests.Serializer.Models;

public partial class SubItemModel
{
    [DFeAttribute(TipoCampo.Int, "nSeq")]
    public int Sequencia { get; set; }

    [DFeElement(TipoCampo.Str, "descricao")]
    public string Descricao { get; set; } = string.Empty;

    [DFeElement(TipoCampo.De2, "valor")]
    public decimal Valor { get; set; }
}

[DFeRoot("CollectionRoot")]
public partial class CollectionTestModel : DFeDocument<CollectionTestModel>
{
    [DFeCollection("itens")]
    public List<SubItemModel> ItensList { get; set; } = new();

    [DFeCollection("itensArray")]
    public SubItemModel[] ItensArray { get; set; } = Array.Empty<SubItemModel>();

    [DFeCollection("dfeItens")]
    public DFeCollection<SubItemModel> DFeItens { get; set; } = new();

    [DFeCollection(TipoCampo.Str, "tags")]
    public List<string> Tags { get; set; } = new();

    [DFeCollection(TipoCampo.Int, "codigos")]
    public List<int> Codigos { get; set; } = new();

    [DFeCollection(TipoCampo.Enum, "statusList")]
    public List<StatusOperacao> StatusList { get; set; } = new();
}
