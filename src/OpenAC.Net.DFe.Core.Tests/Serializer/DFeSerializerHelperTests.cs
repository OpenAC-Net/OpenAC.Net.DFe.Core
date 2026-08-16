using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Tests.Serializer;

public enum TesteHelperEnum
{
    ItemA = 0,
    ItemB = 1
}

public class DFeSerializerHelperTests
{
    private readonly SerializerOptions options = new();

    [Test]
    public async Task TestFormatValue_Str()
    {
        await Assert.That(DFeSerializerHelper.FormatValue_Str("  teste  ", 0, options)).IsEqualTo("teste");
        await Assert.That(DFeSerializerHelper.FormatValue_Str(null, 0, options)).IsNull();
    }

    [Test]
    public async Task TestFormatValue_StrNumber()
    {
        await Assert.That(DFeSerializerHelper.FormatValue_StrNumber("12.34-56", 0, options)).IsEqualTo("123456");
        await Assert.That(DFeSerializerHelper.FormatValue_StrNumber(null, 0, options)).IsNull();
    }

    [Test]
    public async Task TestFormatValue_StrNumberFill()
    {
        await Assert.That(DFeSerializerHelper.FormatValue_StrNumberFill("123", 5, options)).IsEqualTo("00123");
        await Assert.That(DFeSerializerHelper.FormatValue_StrNumberFill("12345", 5, options)).IsEqualTo("12345");
        await Assert.That(DFeSerializerHelper.FormatValue_StrNumberFill(null, 5, options)).IsNull();
    }

    [Test]
    public async Task TestFormatValue_Int()
    {
        await Assert.That(DFeSerializerHelper.FormatValue_Int(42, 4, options)).IsEqualTo("0042");
        await Assert.That(DFeSerializerHelper.FormatValue_Int(42, 0, options)).IsEqualTo("42");
        await Assert.That(DFeSerializerHelper.FormatValue_Int(null, 0, options)).IsNull();
    }

    [Test]
    public async Task TestFormatValue_Decimals()
    {
        await Assert.That(DFeSerializerHelper.FormatValue_De2(123.45m, 0, options)).IsEqualTo("123.45");
        await Assert.That(DFeSerializerHelper.FormatValue_De3(123.45m, 0, options)).IsEqualTo("123.450");
        await Assert.That(DFeSerializerHelper.FormatValue_De4(123.45m, 0, options)).IsEqualTo("123.4500");
        await Assert.That(DFeSerializerHelper.FormatValue_De6(123.45m, 0, options)).IsEqualTo("123.450000");
        await Assert.That(DFeSerializerHelper.FormatValue_De10(123.45m, 0, options)).IsEqualTo("123.4500000000");
    }

    [Test]
    public async Task TestFormatValue_DatesAndTimes()
    {
        var dt = new DateTime(2026, 8, 16, 14, 30, 45);
        await Assert.That(DFeSerializerHelper.FormatValue_Dat(dt, 0, options)).IsEqualTo("2026-08-16");
        await Assert.That(DFeSerializerHelper.FormatValue_DatCFe(dt, 0, options)).IsEqualTo("20260816");
        await Assert.That(DFeSerializerHelper.FormatValue_Hor(dt, 0, options)).IsEqualTo("14:30:45");
        await Assert.That(DFeSerializerHelper.FormatValue_HorCFe(dt, 0, options)).IsEqualTo("143045");
        await Assert.That(DFeSerializerHelper.FormatValue_DatHor(dt, 0, options)).IsEqualTo("2026-08-16T14:30:45");

        var dto = new DateTimeOffset(2026, 8, 16, 14, 30, 45, TimeSpan.FromHours(-3));
        await Assert.That(DFeSerializerHelper.FormatValue_DatHorTz(dto, 0, options)).IsEqualTo("2026-08-16T14:30:45-03:00");
    }

    [Test]
    public async Task TestParseValue_Primitives()
    {
        await Assert.That(DFeSerializerHelper.ParseValue_Str("<![CDATA[conteudo]]>")).IsEqualTo("conteudo");
        await Assert.That(DFeSerializerHelper.ParseValue_Str("normal")).IsEqualTo("normal");
        await Assert.That(DFeSerializerHelper.ParseValue_Int("123")).IsEqualTo(123);
        await Assert.That(DFeSerializerHelper.ParseValue_Long("123456789012")).IsEqualTo(123456789012L);
        await Assert.That(DFeSerializerHelper.ParseValue_De2("123.45")).IsEqualTo(123.45m);
        await Assert.That(DFeSerializerHelper.ParseValue_StrNumber("12.34-56")).IsEqualTo("123456");
    }

    [Test]
    public async Task TestParseValue_Dates()
    {
        var expected = new DateTime(2026, 8, 16);
        await Assert.That(DFeSerializerHelper.ParseValue_Dat("2026-08-16")).IsEqualTo(expected);
        await Assert.That(DFeSerializerHelper.ParseValue_DatCFe("20260816")).IsEqualTo(expected);

        var expectedHor = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 14, 30, 45);
        await Assert.That(DFeSerializerHelper.ParseValue_Hor("14:30:45").TimeOfDay).IsEqualTo(new TimeSpan(14, 30, 45));
        await Assert.That(DFeSerializerHelper.ParseValue_HorCFe("143045").TimeOfDay).IsEqualTo(new TimeSpan(14, 30, 45));
    }

    [Test]
    public async Task TestParseEnum_Generic()
    {
        await Assert.That(DFeSerializerHelper.ParseEnum_Generic<TesteHelperEnum>("ItemB")).IsEqualTo(TesteHelperEnum.ItemB);
        await Assert.That(DFeSerializerHelper.ParseEnum_Generic<TesteHelperEnum>("Invalido")).IsEqualTo(TesteHelperEnum.ItemA);
    }
}
