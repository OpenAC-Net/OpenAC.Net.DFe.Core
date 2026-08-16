using System.Xml.Linq;
using OpenAC.Net.DFe.Core.Tests.Serializer.Models;

namespace OpenAC.Net.DFe.Core.Tests.Serializer;

public class PrimitiveSerializerTests
{
    [Test]
    public async Task TestSerializeAllPrimitiveTypes()
    {
        var model = new PrimitiveTestModel
        {
            StrVal = "Teste String",
            StrNumVal = "12.34-56",
            StrNumFillVal = "42",
            IntVal = 7,
            LongVal = 9876543210123L,
            DatVal = new DateTime(2026, 8, 16),
            DatCFeVal = new DateTime(2026, 8, 16),
            HorVal = new DateTime(2026, 8, 16, 15, 30, 45),
            HorCFeVal = new DateTime(2026, 8, 16, 15, 30, 45),
            DatHorVal = new DateTime(2026, 8, 16, 15, 30, 45),
            DatHorTzVal = new DateTimeOffset(2026, 8, 16, 15, 30, 45, TimeSpan.FromHours(-3)),
            De2Val = 123.45m,
            De3Val = 123.456m,
            De4Val = 123.4567m,
            De6Val = 123.456789m,
            De10Val = 123.4567890123m,
            NullableInt = 999,
            NullableDec = 50.25m,
            NullableDat = new DateTime(2026, 1, 1),
            NullableDatHorTz = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.FromHours(-3))
        };

        var xml = model.GetXml();
        await Assert.That(xml).IsNotNull().And.IsNotEqualTo(string.Empty);

        var xDoc = XDocument.Parse(xml);
        var root = xDoc.Root;
        await Assert.That(root).IsNotNull();
        await Assert.That(root!.Name.LocalName).IsEqualTo("PrimitiveRoot");

        await Assert.That(root.Element("strVal")?.Value).IsEqualTo("Teste String");
        await Assert.That(root.Element("strNumVal")?.Value).IsEqualTo("123456");
        await Assert.That(root.Element("strNumFillVal")?.Value).IsEqualTo("00042");
        await Assert.That(root.Element("intVal")?.Value).IsEqualTo("0007");
        await Assert.That(root.Element("longVal")?.Value).IsEqualTo("9876543210123");
        await Assert.That(root.Element("datVal")?.Value).IsEqualTo("2026-08-16");
        await Assert.That(root.Element("datCFeVal")?.Value).IsEqualTo("20260816");
        await Assert.That(root.Element("horVal")?.Value).IsEqualTo("15:30:45");
        await Assert.That(root.Element("horCFeVal")?.Value).IsEqualTo("153045");
        await Assert.That(root.Element("datHorVal")?.Value).IsEqualTo("2026-08-16T15:30:45");
        await Assert.That(root.Element("datHorTzVal")?.Value).IsEqualTo("2026-08-16T15:30:45-03:00");
        await Assert.That(root.Element("de2Val")?.Value).IsEqualTo("123.45");
        await Assert.That(root.Element("de3Val")?.Value).IsEqualTo("123.456");
        await Assert.That(root.Element("de4Val")?.Value).IsEqualTo("123.4567");
        await Assert.That(root.Element("de6Val")?.Value).IsEqualTo("123.456789");
        await Assert.That(root.Element("de10Val")?.Value).IsEqualTo("123.4567890123");
        await Assert.That(root.Element("nullableInt")?.Value).IsEqualTo("999");
        await Assert.That(root.Element("nullableDec")?.Value).IsEqualTo("50.25");
        await Assert.That(root.Element("nullableDat")?.Value).IsEqualTo("2026-01-01");
    }

    [Test]
    public async Task TestDeserializeAllPrimitiveTypes()
    {
        const string xml = """
                           <PrimitiveRoot>
                                       <strVal>Texto Exemplo</strVal>
                                       <strNumVal>998877</strNumVal>
                                       <strNumFillVal>00123</strNumFillVal>
                                       <intVal>0042</intVal>
                                       <longVal>12345678901234</longVal>
                                       <datVal>2026-08-16</datVal>
                                       <datCFeVal>20260816</datCFeVal>
                                       <horVal>10:20:30</horVal>
                                       <horCFeVal>102030</horCFeVal>
                                       <datHorVal>2026-08-16T10:20:30</datHorVal>
                                       <datHorTzVal>2026-08-16T10:20:30-03:00</datHorTzVal>
                                       <de2Val>99.90</de2Val>
                                       <de3Val>99.900</de3Val>
                                       <de4Val>99.9000</de4Val>
                                       <de6Val>99.900000</de6Val>
                                       <de10Val>99.9000000000</de10Val>
                                       <nullableInt>555</nullableInt>
                                       <nullableDec>77.88</nullableDec>
                                       <nullableDat>2026-05-20</nullableDat>
                                   </PrimitiveRoot>
                           """;

        var model = PrimitiveTestModel.Load(xml);
        await Assert.That(model).IsNotNull();

        await Assert.That(model.StrVal).IsEqualTo("Texto Exemplo");
        await Assert.That(model.StrNumVal).IsEqualTo("998877");
        await Assert.That(model.StrNumFillVal).IsEqualTo("00123");
        await Assert.That(model.IntVal).IsEqualTo(42);
        await Assert.That(model.LongVal).IsEqualTo(12345678901234L);
        await Assert.That(model.DatVal).IsEqualTo(new DateTime(2026, 8, 16));
        await Assert.That(model.DatCFeVal).IsEqualTo(new DateTime(2026, 8, 16));
        await Assert.That(model.HorVal.TimeOfDay).IsEqualTo(new TimeSpan(10, 20, 30));
        await Assert.That(model.HorCFeVal.TimeOfDay).IsEqualTo(new TimeSpan(10, 20, 30));
        await Assert.That(model.DatHorVal).IsEqualTo(new DateTime(2026, 8, 16, 10, 20, 30));
        await Assert.That(model.DatHorTzVal.Offset).IsEqualTo(TimeSpan.FromHours(-3));
        await Assert.That(model.De2Val).IsEqualTo(99.90m);
        await Assert.That(model.De3Val).IsEqualTo(99.900m);
        await Assert.That(model.De4Val).IsEqualTo(99.9000m);
        await Assert.That(model.De6Val).IsEqualTo(99.900000m);
        await Assert.That(model.De10Val).IsEqualTo(99.9000000000m);
        await Assert.That(model.NullableInt).IsEqualTo(555);
        await Assert.That(model.NullableDec).IsEqualTo(77.88m);
        await Assert.That(model.NullableDat).IsEqualTo(new DateTime(2026, 5, 20));
    }

    [Test]
    public async Task TestNullablePrimitivesOmittedWhenNull()
    {
        var model = new PrimitiveTestModel
        {
            StrVal = "Obrigatorio",
            NullableInt = null,
            NullableDec = null,
            NullableDat = null,
            NullableDatHorTz = null
        };

        var xml = model.GetXml();
        var xDoc = XDocument.Parse(xml);

        await Assert.That(xDoc.Root?.Element("nullableInt")).IsNull();
        await Assert.That(xDoc.Root?.Element("nullableDec")).IsNull();
        await Assert.That(xDoc.Root?.Element("nullableDat")).IsNull();
        await Assert.That(xDoc.Root?.Element("nullableDatHorTz")).IsNull();
    }
}
