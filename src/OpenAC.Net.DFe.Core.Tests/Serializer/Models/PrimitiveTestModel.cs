using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Document;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Tests.Serializer.Models;

[DFeRoot("PrimitiveRoot")]
public partial class PrimitiveTestModel : DFeDocument<PrimitiveTestModel>
{
    [DFeElement(TipoCampo.Str, "strVal")]
    public string StrVal { get; set; } = string.Empty;

    [DFeElement(TipoCampo.StrNumber, "strNumVal")]
    public string StrNumVal { get; set; } = string.Empty;

    [DFeElement(TipoCampo.StrNumberFill, "strNumFillVal", Min = 5)]
    public string StrNumFillVal { get; set; } = string.Empty;

    [DFeElement(TipoCampo.Int, "intVal", Min = 4)]
    public int IntVal { get; set; }

    [DFeElement(TipoCampo.Long, "longVal")]
    public long LongVal { get; set; }

    [DFeElement(TipoCampo.Dat, "datVal")]
    public DateTime DatVal { get; set; }

    [DFeElement(TipoCampo.DatCFe, "datCFeVal")]
    public DateTime DatCFeVal { get; set; }

    [DFeElement(TipoCampo.Hor, "horVal")]
    public DateTime HorVal { get; set; }

    [DFeElement(TipoCampo.HorCFe, "horCFeVal")]
    public DateTime HorCFeVal { get; set; }

    [DFeElement(TipoCampo.DatHor, "datHorVal")]
    public DateTime DatHorVal { get; set; }

    [DFeElement(TipoCampo.DatHorTz, "datHorTzVal")]
    public DateTimeOffset DatHorTzVal { get; set; }

    [DFeElement(TipoCampo.De2, "de2Val")]
    public decimal De2Val { get; set; }

    [DFeElement(TipoCampo.De3, "de3Val")]
    public decimal De3Val { get; set; }

    [DFeElement(TipoCampo.De4, "de4Val")]
    public decimal De4Val { get; set; }

    [DFeElement(TipoCampo.De6, "de6Val")]
    public decimal De6Val { get; set; }

    [DFeElement(TipoCampo.De10, "de10Val")]
    public decimal De10Val { get; set; }

    [DFeElement(TipoCampo.Int, "nullableInt", Ocorrencia = Ocorrencia.NaoObrigatoria)]
    public int? NullableInt { get; set; }

    [DFeElement(TipoCampo.De2, "nullableDec", Ocorrencia = Ocorrencia.NaoObrigatoria)]
    public decimal? NullableDec { get; set; }

    [DFeElement(TipoCampo.Dat, "nullableDat", Ocorrencia = Ocorrencia.NaoObrigatoria)]
    public DateTime? NullableDat { get; set; }

    [DFeElement(TipoCampo.DatHorTz, "nullableDatHorTz", Ocorrencia = Ocorrencia.NaoObrigatoria)]
    public DateTimeOffset? NullableDatHorTz { get; set; }
}
