using System.Globalization;
using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Document;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Tests.Serializer.Models;

[DFeRoot("custom")]
public partial class CustomHookDocument : DFeDocument<CustomHookDocument>
{
    [DFeAttribute(TipoCampo.Custom, "n", Ocorrencia = Ocorrencia.Obrigatoria)]
    public int Number { get; set; }
    [DFeIgnore] public bool EmitNumber { get; set; } = true;
    [DFeIgnore] public int NumberSerializeCalls { get; set; }
    private bool ShouldSerializeNumber() => EmitNumber;
    private string SerializeNumber()
    {
        NumberSerializeCalls++;
        return "N-" + Number.ToString("D2", CultureInfo.InvariantCulture);
    }
    private object DeserializeNumber(string value) => int.Parse(value.Substring(2), CultureInfo.InvariantCulture);

    [DFeElement(TipoCampo.Custom, "registration", Ordem = 10, Ocorrencia = Ocorrencia.Obrigatoria)]
    public string Registration { get; set; } = "";
    private string SerializeRegistration() => Registration.Trim().ToUpperInvariant();
    private string DeserializeRegistration(string value) => value.ToLowerInvariant();

    [DFeElement(TipoCampo.Custom, "flag", Ordem = 20, Ocorrencia = Ocorrencia.Obrigatoria)]
    public bool Flag { get; set; }
    private string SerializeFlag() => Flag ? "1" : "0";
    private object DeserializeFlag(string value) => value == "1";

    [DFeElement(TipoCampo.Custom, "amount", Ordem = 30, Ocorrencia = Ocorrencia.Obrigatoria)]
    public decimal Amount { get; set; }
    private string SerializeAmount() => Amount.ToString("F4", CultureInfo.InvariantCulture);
    private decimal DeserializeAmount(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);

    [DFeElement(TipoCampo.Custom, "date", Ordem = 40, Ocorrencia = Ocorrencia.Obrigatoria)]
    public DateTimeOffset Date { get; set; }
    private string SerializeDate() => Date.ToString("yyyyMMddHHmmsszzz", CultureInfo.InvariantCulture);
    private object DeserializeDate(string value) => DateTimeOffset.ParseExact(value, "yyyyMMddHHmmsszzz", CultureInfo.InvariantCulture);

    [DFeElement(TipoCampo.Custom, "kind", Ordem = 50, Ocorrencia = Ocorrencia.Obrigatoria)]
    public CustomHookKind Kind { get; set; }
    private string SerializeKind() => "K" + (int)Kind;
    private CustomHookKind DeserializeKind(string value) => (CustomHookKind)int.Parse(value.Substring(1), CultureInfo.InvariantCulture);

    [DFeElement(TipoCampo.Custom, "payload", Ordem = 60, Ocorrencia = Ocorrencia.Obrigatoria, UseCData = true)]
    public string Payload { get; set; } = "";
    private string SerializePayload() => Payload;
    private string DeserializePayload(string value) => value;

    [DFeElement(TipoCampo.Custom, "nullable", Ordem = 70)]
    public int? Nullable { get; set; }
    [DFeIgnore] public int NullableSerializeCalls { get; set; }
    private string SerializeNullable()
    {
        NullableSerializeCalls++;
        return Nullable?.ToString(CultureInfo.InvariantCulture) ?? "EMPTY";
    }
    private int? DeserializeNullable(string value) => value == "EMPTY" ? null : int.Parse(value, CultureInfo.InvariantCulture);

    [DFeElement(TipoCampo.Custom, "hidden", Ordem = 80)]
    public string Hidden { get; set; } = "hidden";
    private bool ShouldSerializeHidden() => false;
    private string SerializeHidden() => throw new InvalidOperationException("ShouldSerialize não foi respeitado.");

    [DFeElement(TipoCampo.Custom, "token", Ordem = 90)]
    public CustomHookToken Token { get; set; } = new("token");
    private string SerializeToken() => Token.Value;
    private CustomHookToken DeserializeToken(string value) => new(value);
}

public enum CustomHookKind
{
    [DFeEnum("standard-code")]
    First
}

public sealed record CustomHookToken(string Value);

[DFeRoot("customText")]
public partial class CustomHookText : DFeDocument<CustomHookText>
{
    [DFeAttribute(TipoCampo.Str, "unit")]
    public string Unit { get; set; } = "UN";
    [DFeItemValue(TipoCampo.Custom)]
    public decimal Value { get; set; }
    [DFeIgnore] public bool EmitValue { get; set; } = true;
    [DFeIgnore] public int ValueSerializeCalls { get; set; }
    private bool ShouldSerializeValue() => EmitValue;
    private string SerializeValue()
    {
        ValueSerializeCalls++;
        return "V=" + Value.ToString("F3", CultureInfo.InvariantCulture);
    }
    private object DeserializeValue(string value) => decimal.Parse(value.Substring(2), CultureInfo.InvariantCulture);
}

public class CustomHookBase
{
    [DFeElement(TipoCampo.Custom, "code", Ocorrencia = Ocorrencia.Obrigatoria)]
    public int Code { get; set; }
    [DFeIgnore] public bool EmitCode { get; set; } = true;
    protected bool ShouldSerializeCode() => EmitCode;
    protected string SerializeCode() => "CODE-" + Code.ToString(CultureInfo.InvariantCulture);
    protected object DeserializeCode(string value) => int.Parse(value.Substring(5), CultureInfo.InvariantCulture);
}

[DFeRoot("inherited")]
public partial class InheritedCustomHook : CustomHookBase { }

[DFeRoot("fallback")]
public partial class CustomHookFallback : DFeDocument<CustomHookFallback>
{
    [DFeElement(TipoCampo.Custom, "plain")]
    public int Plain { get; set; }

    [DFeElement(TipoCampo.Custom, "write")]
    public int Write { get; set; }
    private string SerializeWrite() => "W" + Write.ToString(CultureInfo.InvariantCulture);

    [DFeAttribute(TipoCampo.Custom, "read")]
    public int Read { get; set; }
    private object DeserializeRead(string value) => int.Parse(value, CultureInfo.InvariantCulture) + 1;

    [DFeElement(TipoCampo.Str, "standard")]
    public string Standard { get; set; } = "original";
    private string SerializeStandard() => throw new InvalidOperationException("Hook deve aplicar-se somente a Custom.");
}

[DFeRoot("empty")]
public partial class CustomHookEmpty : DFeDocument<CustomHookEmpty>
{
    [DFeAttribute(TipoCampo.Custom, "a")]
    public string Attribute { get; set; } = "";
    private string SerializeAttribute() => Attribute;
    private string DeserializeAttribute(string value) => value.Length == 0 ? "empty-attribute" : value;

    [DFeElement(TipoCampo.Custom, "e")]
    public string Element { get; set; } = "";
    private string SerializeElement() => Element;
    private string DeserializeElement(string value) => value.Length == 0 ? "empty-element" : value;
}

[DFeRoot("errors")]
public partial class CustomHookErrors : DFeDocument<CustomHookErrors>
{
    [DFeElement(TipoCampo.Custom, "value")]
    public int Value { get; set; }
    private string SerializeValue() => throw new InvalidOperationException("serialize-hook");
    private object DeserializeValue(string value) => throw new FormatException("deserialize-hook");
}

[DFeRoot("conditional")]
public partial class ConditionalSerializationDocument : DFeDocument<ConditionalSerializationDocument>
{
    [DFeAttribute(TipoCampo.Str, "version")] public string Version { get; set; } = "1";
    [DFeElement(TipoCampo.Str, "text")] public string Text { get; set; } = "text";
    [DFeElement(TipoCampo.Int, "number")] public int Number { get; set; } = 123;
    [DFeElement(TipoCampo.Custom, "custom")] public bool Custom { get; set; } = true;
    [DFeCollection(TipoCampo.Str, "item")] public List<string> Items { get; set; } = ["item"];
    [DFeElement(TipoCampo.Enum, "kind")] public CustomHookKind Kind { get; set; }
    [DFeElement("child")] public CustomHookErrors Child { get; set; } = new();
    [DFeDictionary("map")]
    [DFeDictionaryKey(TipoCampo.Enum, "key", AsAttribute = true)]
    [DFeDictionaryValue(TipoCampo.Str, "value")]
    public Dictionary<CustomHookKind, string> Map { get; set; } = new() { [CustomHookKind.First] = "value" };
    private bool ShouldSerializeKind() => false;
    private bool ShouldSerializeChild() => false;
    private bool ShouldSerializeMap() => false;
    private bool ShouldSerializeVersion() => false;
    private bool ShouldSerializeText() => false;
    private bool ShouldSerializeNumber() => false;
    private bool ShouldSerializeCustom() => false;
    private bool ShouldSerializeItems() => false;
    private string SerializeCustom() => throw new InvalidOperationException("Custom não deveria ser chamado.");
}

[DFeRoot("conditionalText")]
public partial class ConditionalPrimitiveText : DFeDocument<ConditionalPrimitiveText>
{
    [DFeItemValue(TipoCampo.Int)] public int Value { get; set; } = 123;
    private bool ShouldSerializeValue() => false;
}
