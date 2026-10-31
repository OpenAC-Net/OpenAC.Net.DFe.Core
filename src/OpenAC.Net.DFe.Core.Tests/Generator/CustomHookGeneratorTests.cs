using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Generator;

namespace OpenAC.Net.DFe.Core.Tests.Generator;

public class CustomHookGeneratorTests
{
    [Test]
    [Arguments("private int SerializeValue() => 1;")]
    [Arguments("private string SerializeValue(int value) => value.ToString();")]
    [Arguments("private static string SerializeValue() => \"1\";")]
    [Arguments("private void DeserializeValue(string value) { }")]
    [Arguments("private object DeserializeValue(int value) => value;")]
    [Arguments("private bool DeserializeValue(string value) => true;")]
    public async Task InvalidCustomHookSignaturesProduceAnActionableDiagnostic(string hook)
    {
        var (_, result) = Generate(hook);
        await Assert.That(result.Diagnostics.Any(d => d.Id == "DFE0004" && d.Severity == DiagnosticSeverity.Error)).IsTrue();
        await Assert.That(result.Diagnostics.Single(d => d.Id == "DFE0004").GetMessage()).Contains("Value");
    }

    [Test]
    public async Task PrivateLegacyHooksAndPrivateShouldSerializeProduceCompilableDirectCalls()
    {
        var (compilation, result) = Generate("""
            private string SerializeValue() => "C-" + Value;
            private object DeserializeValue(string value) => int.Parse(value.Substring(2));
            private bool ShouldSerializeValue() => Value > 0;
            """);
        await Assert.That(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)).IsEmpty();
        await Assert.That(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error)).IsEmpty();
        var source = result.GeneratedTrees.Single().GetText().ToString();
        await Assert.That(source).Contains("this.ShouldSerializeValue()");
        await Assert.That(source).Contains("this.SerializeValue()");
        await Assert.That(source).Contains("(int)this.DeserializeValue(");
        await Assert.That(source).DoesNotContain("System.Reflection");
    }

    private static (Compilation Compilation, GeneratorDriverRunResult Result) Generate(string hook)
    {
        var source = $$"""
            using OpenAC.Net.DFe.Core.Attributes;
            using OpenAC.Net.DFe.Core.Serializer;
            namespace CustomHookProbe;
            [DFeRoot("probe")]
            public partial class Probe
            {
                [DFeElement(TipoCampo.Custom, "v")]
                public int Value { get; set; }
                {{hook}}
            }
            """;
        var compilation = CSharpCompilation.Create("CustomHookProbe", [CSharpSyntaxTree.ParseText(source)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
             MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
             MetadataReference.CreateFromFile(typeof(OpenAC.Net.Core.OpenDisposable).Assembly.Location),
             MetadataReference.CreateFromFile(typeof(System.Xml.Linq.XElement).Assembly.Location),
             MetadataReference.CreateFromFile(typeof(DFeRootAttribute).Assembly.Location),
             MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("System.Runtime").Location),
             MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("System.Collections").Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new DFeSourceGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var updated, out _);
        return (updated, driver.GetRunResult());
    }
}
