using System.Globalization;
using System.Xml.Linq;
using OpenAC.Net.DFe.Core.Tests.Serializer.Models;

namespace OpenAC.Net.DFe.Core.Tests.Serializer;

public class CustomSerializerTests
{
    [Test]
    [Arguments("pt-BR")]
    [Arguments("en-US")]
    public async Task CustomHooksRoundTripAttributesElementsEnumsAndObjectReturns(string culture)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            var date = new DateTimeOffset(2026, 10, 30, 13, 45, 0, TimeSpan.FromHours(-3));
            var model = new CustomHookDocument
            {
                Number = 7, Registration = " isento ", Flag = true, Amount = 1234.5678m,
                Date = date, Nullable = 42, Payload = "<tag>&\"ç", Token = new CustomHookToken("custom-token")
            };
            var xml = XElement.Parse(model.GetXml());
            await Assert.That(xml.Attribute("n")!.Value).IsEqualTo("N-07");
            await Assert.That(xml.Element("registration")!.Value).IsEqualTo("ISENTO");
            await Assert.That(xml.Element("flag")!.Value).IsEqualTo("1");
            await Assert.That(xml.Element("amount")!.Value).IsEqualTo("1234.5678");
            await Assert.That(xml.Element("date")!.Value).IsEqualTo("20261030134500-03:00");
            await Assert.That(xml.Element("kind")!.Value).IsEqualTo("K0");
            await Assert.That(xml.Element("payload")!.FirstNode).IsTypeOf<XCData>();
            await Assert.That(xml.Element("nullable")!.Value).IsEqualTo("42");
            await Assert.That(xml.Element("hidden")).IsNull();
            var loaded = CustomHookDocument.Load(xml.ToString());
            await Assert.That(loaded.Number).IsEqualTo(7);
            await Assert.That(loaded.Registration).IsEqualTo("isento");
            await Assert.That(loaded.Flag).IsTrue();
            await Assert.That(loaded.Amount).IsEqualTo(model.Amount);
            await Assert.That(loaded.Date).IsEqualTo(date);
            await Assert.That(loaded.Date.Offset).IsEqualTo(date.Offset);
            await Assert.That(loaded.Kind).IsEqualTo(CustomHookKind.First);
            await Assert.That(loaded.Payload).IsEqualTo(model.Payload);
            await Assert.That(loaded.Nullable).IsEqualTo(42);
            await Assert.That(loaded.Token.Value).IsEqualTo("custom-token");
            await Assert.That(model.Registration).IsEqualTo(" isento ");
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Test]
    public async Task CustomItemValueCallsHooksForTheElementText()
    {
        var model = new CustomHookText { Value = 123.456m };
        var xml = XElement.Parse(model.GetXml());
        await Assert.That(xml.Value).IsEqualTo("V=123.456");
        await Assert.That(xml.Attribute("unit")!.Value).IsEqualTo("UN");
        await Assert.That(CustomHookText.Load(xml.ToString()).Value).IsEqualTo(model.Value);
    }

    [Test]
    public async Task InheritedProtectedHooksAreCalledDirectly()
    {
        var model = new InheritedCustomHook { Code = 123 };
        var xml = model.WriteToXml();
        await Assert.That(xml.Element("code")!.Value).IsEqualTo("CODE-123");
        await Assert.That(InheritedCustomHook.ReadFromXml(xml)!.Code).IsEqualTo(123);
    }

    [Test]
    public async Task MissingHooksKeepTheExistingFallbackAndStandardFieldsIgnoreHookNames()
    {
        var model = new CustomHookFallback { Plain = 17, Write = 3, Read = 4 };
        var xml = XElement.Parse(model.GetXml());
        await Assert.That(xml.Element("plain")!.Value).IsEqualTo("17");
        await Assert.That(xml.Element("write")!.Value).IsEqualTo("W3");
        await Assert.That(xml.Attribute("read")!.Value).IsEqualTo("4");
        await Assert.That(xml.Element("standard")!.Value).IsEqualTo("original");
        var loaded = CustomHookFallback.Load("<fallback read='4'><plain>17</plain><write>3</write></fallback>");
        await Assert.That(loaded.Plain).IsEqualTo(17);
        await Assert.That(loaded.Write).IsEqualTo(3);
        await Assert.That(loaded.Read).IsEqualTo(5);
    }

    [Test]
    public async Task EmptyXmlValuesArePassedToTheDeserializerAndEmptyOptionalOutputsAreOmitted()
    {
        var loaded = CustomHookEmpty.Load("<empty a=''><e /></empty>");
        await Assert.That(loaded.Attribute).IsEqualTo("empty-attribute");
        await Assert.That(loaded.Element).IsEqualTo("empty-element");
        var xml = XElement.Parse(new CustomHookEmpty().GetXml());
        await Assert.That(xml.Attribute("a")).IsNull();
        await Assert.That(xml.Element("e")).IsNull();
    }

    [Test]
    public async Task ShouldSerializePrecedesCustomAttributesAndTextWithoutInvokingTheFormatter()
    {
        var model = new CustomHookDocument { EmitNumber = false };
        var xml = XElement.Parse(model.GetXml());
        await Assert.That(xml.Attribute("n")).IsNull();
        await Assert.That(model.NumberSerializeCalls).IsEqualTo(0);
        var text = new CustomHookText { EmitValue = false, Value = 123 };
        var textXml = XElement.Parse(text.GetXml());
        await Assert.That(textXml.Value).IsEqualTo(string.Empty);
        await Assert.That(textXml.Attribute("unit")!.Value).IsEqualTo("UN");
        await Assert.That(text.ValueSerializeCalls).IsEqualTo(0);
        var inherited = new InheritedCustomHook { EmitCode = false, Code = 123 };
        await Assert.That(inherited.WriteToXml().Element("code")).IsNull();
    }

    [Test]
    public async Task ShouldSerializeAppliesToStandardTypesCollectionsAndPrimitiveTextToo()
    {
        var xml = XElement.Parse(new ConditionalSerializationDocument().GetXml());
        await Assert.That(xml.HasAttributes).IsFalse();
        await Assert.That(xml.HasElements).IsFalse();
        var text = XElement.Parse(new ConditionalPrimitiveText().GetXml());
        await Assert.That(text.Value).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task NullOptionalValuesKeepTheOmissionRuleAndCustomDeserializersCanReturnNull()
    {
        var model = new CustomHookDocument();
        var xml = XElement.Parse(model.GetXml());
        await Assert.That(xml.Element("nullable")).IsNull();
        await Assert.That(model.NullableSerializeCalls).IsEqualTo(0);
        xml.Add(new XElement("nullable", "EMPTY"));
        await Assert.That(CustomHookDocument.Load(xml.ToString()).Nullable).IsNull();
    }

    [Test]
    public async Task HookExceptionsReachTheCallerWithoutReflectionWrappers()
    {
        await Assert.That(() => new CustomHookErrors().GetXml()).ThrowsExactly<InvalidOperationException>();
        await Assert.That(() => CustomHookErrors.Load("<errors><value>1</value></errors>")).ThrowsExactly<FormatException>();
    }
}
