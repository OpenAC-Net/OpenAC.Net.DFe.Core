using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Collection;
using OpenAC.Net.DFe.Core.Document;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Tests.Serializer.Models;

public partial class ChildItemModel : DFeParentItem<ChildItemModel, ParentChildTestModel>
{
    [DFeAttribute(TipoCampo.Int, "id")]
    public int Id { get; set; }

    [DFeElement(TipoCampo.Str, "nome")]
    public string Nome { get; set; } = string.Empty;

    public string GetParentCodigo() => Parent?.Codigo ?? string.Empty;
}

[DFeRoot("ParentRoot")]
public partial class ParentChildTestModel : DFeDocument<ParentChildTestModel>
{
    public ParentChildTestModel()
    {
        Filhos = new DFeParentCollection<ChildItemModel, ParentChildTestModel>(this);
    }

    [DFeAttribute(TipoCampo.Str, "codigo")]
    public string Codigo { get; set; } = string.Empty;

    [DFeCollection("filhos")]
    public DFeParentCollection<ChildItemModel, ParentChildTestModel> Filhos { get; set; }
}
