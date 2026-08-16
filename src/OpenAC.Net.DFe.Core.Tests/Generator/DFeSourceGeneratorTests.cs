using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Common;
using OpenAC.Net.DFe.Core.Document;
using OpenAC.Net.DFe.Core.Serializer;
using OpenAC.Net.DFe.Generator;

namespace OpenAC.Net.DFe.Core.Tests.Generator;

public class DFeSourceGeneratorTests
{
    private const string SampleDFeClass = """

                                          using System;
                                          using System.Collections.Generic;
                                          using OpenAC.Net.DFe.Core.Attributes;
                                          using OpenAC.Net.DFe.Core.Serializer;

                                          namespace MyTestNamespace;

                                          public enum TipoOperacao
                                          {
                                              [DFeEnum("0")]
                                              Entrada = 0,

                                              [DFeEnum("1")]
                                              Saida = 1
                                          }

                                          [DFeRoot("NFe", Namespace = "http://www.portalfiscal.inf.br/nfe")]
                                          public partial class DocumentoFiscal
                                          {
                                              [DFeAttribute(TipoCampo.Int, "Id", Min = 1, Max = 44)]
                                              public int Id { get; set; }

                                              [DFeElement(TipoCampo.Str, "xNome", Min = 2, Max = 60, Ocorrencia = Ocorrencia.Obrigatoria)]
                                              public string Nome { get; set; } = string.Empty;

                                              [DFeElement(TipoCampo.De2, "vTotal", Ocorrencia = Ocorrencia.Obrigatoria)]
                                              public decimal ValorTotal { get; set; }

                                              [DFeElement(TipoCampo.DatHor, "dhEmi", Ocorrencia = Ocorrencia.Obrigatoria)]
                                              public DateTime DataEmissao { get; set; }

                                              [DFeElement(TipoCampo.Enum, "tpNF", Ocorrencia = Ocorrencia.Obrigatoria)]
                                              public TipoOperacao Tipo { get; set; }

                                              [DFeCollection("det")]
                                              public List<ItemDetalhe> Itens { get; set; } = new();

                                              public bool ShouldSerializeId() => Id > 0;
                                          }

                                          public partial class ItemDetalhe
                                          {
                                              [DFeAttribute(TipoCampo.Int, "nItem")]
                                              public int Numero { get; set; }

                                              [DFeElement(TipoCampo.Str, "xProd")]
                                              public string Produto { get; set; } = string.Empty;

                                              [DFeElement(TipoCampo.De4, "vUnCom")]
                                              public decimal ValorUnitario { get; set; }
                                          }

                                          """;

    [Test]
    public async Task GenerateDFeSerializerForDocument()
    {
        var generator = new DFeSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            "TestCompilation",
            [CSharpSyntaxTree.ParseText(SampleDFeClass)],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Xml.Linq.XElement).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DFeRootAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(TipoCampo).Assembly.Location),
                MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("System.Runtime").Location),
                MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("System.Collections").Location)
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        await Assert.That(runResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)).IsEmpty();

        var generatedFiles = runResult.GeneratedTrees.Select(t => Path.GetFileName(t.FilePath)).ToList();
        await Assert.That(generatedFiles).Contains("MyTestNamespace_DocumentoFiscal.DFe.g.cs");
        await Assert.That(generatedFiles).Contains("MyTestNamespace_ItemDetalhe.DFe.g.cs");

        var docFiscalTree = runResult.GeneratedTrees.First(t => t.FilePath.EndsWith("MyTestNamespace_DocumentoFiscal.DFe.g.cs"));
        var docFiscalCode = docFiscalTree.GetText().ToString();

        // Verify generated self-serializing methods
        await Assert.That(docFiscalCode).Contains("partial class DocumentoFiscal");
        await Assert.That(docFiscalCode).Contains("public XElement WriteToXml");
        await Assert.That(docFiscalCode).Contains("public static global::MyTestNamespace.DocumentoFiscal? ReadFromXml");
        await Assert.That(docFiscalCode).Contains("public void ReadXml");
        await Assert.That(docFiscalCode).Contains("this.ShouldSerializeId()");
        await Assert.That(docFiscalCode).Contains("colItem.WriteToXml(\"det\", null, options)");
    }

    [Test]
    public async Task ShouldNotGenerateSerializerForNonDFeClasses()
    {
        const string NonDFeSource = """
                                    using System;
                                    using OpenAC.Net.DFe.Core.Common;

                                    namespace MyTestNamespace;

                                    public class MinhaConfigGeral : DFeGeralConfigBase { }
                                    public class MinhaConfigWebservice : DFeWebserviceConfigBase { }
                                    public class MinhaConfigCertificados : DFeCertificadosConfigBase { }
                                    public class MinhaConfigArquivos : DFeArquivosConfigBase { }

                                    public class MinhaConfig : DFeConfigBase<MinhaConfigGeral, MinhaConfigWebservice, MinhaConfigCertificados, MinhaConfigArquivos>
                                    {
                                    }

                                    public class RegularClass
                                    {
                                        public string Nome { get; set; } = string.Empty;
                                        public MinhaConfigGeral Geral { get; set; } = new();
                                    }
                                    """;

        var generator = new DFeSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            "TestCompilationNonDFe",
            [CSharpSyntaxTree.ParseText(NonDFeSource)],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DFeConfigBase<,,,>).Assembly.Location),
                MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("System.Runtime").Location)
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        await Assert.That(runResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)).IsEmpty();

        var generatedFiles = runResult.GeneratedTrees.Select(t => System.IO.Path.GetFileName(t.FilePath)).ToList();
        await Assert.That(generatedFiles).IsEmpty();
    }

    [Test]
    public async Task ShouldReportErrorWhenDFeSignDocumentMissingDFeSignInfoElement()
    {
        const string Source = """
                              using OpenAC.Net.DFe.Core.Attributes;
                              using OpenAC.Net.DFe.Core.Document;

                              namespace MyTestNamespace;

                              [DFeRoot("NFe")]
                              public partial class DocumentoSemSignInfo : DFeSignDocument<DocumentoSemSignInfo>
                              {
                                  [DFeElement(OpenAC.Net.DFe.Core.Serializer.TipoCampo.Str, "xNome")]
                                  public string Nome { get; set; } = string.Empty;
                              }
                              """;

        var generator = new DFeSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            "TestCompilationMissingSignInfo",
            [CSharpSyntaxTree.ParseText(Source)],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DFeRootAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DFeSignDocument<>).Assembly.Location),
                MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("System.Runtime").Location)
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var errors = runResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        await Assert.That(errors).IsNotEmpty();
        await Assert.That(errors.Any(e => e.Id == "DFE0001")).IsTrue();

        var generatedFiles = runResult.GeneratedTrees.Select(t => System.IO.Path.GetFileName(t.FilePath)).ToList();
        await Assert.That(generatedFiles).IsEmpty();
    }

    [Test]
    public async Task ShouldReportErrorWhenDFeDocumentMissingDFeRoot()
    {
        const string Source = """
                              using OpenAC.Net.DFe.Core.Attributes;
                              using OpenAC.Net.DFe.Core.Document;

                              namespace MyTestNamespace;

                              public partial class DocumentoSemRoot : DFeDocument<DocumentoSemRoot>
                              {
                                  [DFeElement(OpenAC.Net.DFe.Core.Serializer.TipoCampo.Str, "xNome")]
                                  public string Nome { get; set; } = string.Empty;
                              }
                              """;

        var generator = new DFeSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            "TestCompilationMissingRoot",
            [CSharpSyntaxTree.ParseText(Source)],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DFeRootAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DFeDocument<>).Assembly.Location),
                MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("System.Runtime").Location)
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var errors = runResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        await Assert.That(errors).IsNotEmpty();
        await Assert.That(errors.Any(e => e.Id == "DFE0002")).IsTrue();

        var generatedFiles = runResult.GeneratedTrees.Select(t => System.IO.Path.GetFileName(t.FilePath)).ToList();
        await Assert.That(generatedFiles).IsEmpty();
    }

    [Test]
    public async Task ShouldReportErrorWhenDFeSignDocumentMissingDFeRoot()
    {
        const string Source = """
                              using OpenAC.Net.DFe.Core.Attributes;
                              using OpenAC.Net.DFe.Core.Document;

                              namespace MyTestNamespace;

                              [DFeSignInfoElement("infNFe")]
                              public partial class DocumentoSignSemRoot : DFeSignDocument<DocumentoSignSemRoot>
                              {
                                  [DFeElement(OpenAC.Net.DFe.Core.Serializer.TipoCampo.Str, "xNome")]
                                  public string Nome { get; set; } = string.Empty;
                              }
                              """;

        var generator = new DFeSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            "TestCompilationSignMissingRoot",
            [CSharpSyntaxTree.ParseText(Source)],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DFeRootAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DFeSignDocument<>).Assembly.Location),
                MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("System.Runtime").Location)
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var errors = runResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        await Assert.That(errors).IsNotEmpty();
        await Assert.That(errors.Any(e => e.Id == "DFE0002")).IsTrue();

        var generatedFiles = runResult.GeneratedTrees.Select(t => System.IO.Path.GetFileName(t.FilePath)).ToList();
        await Assert.That(generatedFiles).IsEmpty();
    }

    [Test]
    public async Task ShouldReportBothErrorsWhenDFeSignDocumentMissingBothRootAndSignInfo()
    {
        const string Source = """
                              using OpenAC.Net.DFe.Core.Attributes;
                              using OpenAC.Net.DFe.Core.Document;

                              namespace MyTestNamespace;

                              public partial class DocumentoSemNada : DFeSignDocument<DocumentoSemNada>
                              {
                                  [DFeElement(OpenAC.Net.DFe.Core.Serializer.TipoCampo.Str, "xNome")]
                                  public string Nome { get; set; } = string.Empty;
                              }
                              """;

        var generator = new DFeSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            "TestCompilationMissingBoth",
            [CSharpSyntaxTree.ParseText(Source)],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DFeRootAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DFeSignDocument<>).Assembly.Location),
                MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("System.Runtime").Location)
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var errors = runResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        await Assert.That(errors.Count).IsEqualTo(2);
        await Assert.That(errors.Any(e => e.Id == "DFE0001")).IsTrue();
        await Assert.That(errors.Any(e => e.Id == "DFE0002")).IsTrue();

        var generatedFiles = runResult.GeneratedTrees.Select(t => System.IO.Path.GetFileName(t.FilePath)).ToList();
        await Assert.That(generatedFiles).IsEmpty();
    }

    [Test]
    public async Task ShouldReportErrorWhenDFeSignDocumentHasEmptySignElement()
    {
        const string Source = """
                              using OpenAC.Net.DFe.Core.Attributes;
                              using OpenAC.Net.DFe.Core.Document;

                              namespace MyTestNamespace;

                              [DFeRoot("NFe")]
                              [DFeSignInfoElement("")]
                              public partial class DocumentoSignElementVazio : DFeSignDocument<DocumentoSignElementVazio>
                              {
                                  [DFeElement(OpenAC.Net.DFe.Core.Serializer.TipoCampo.Str, "xNome")]
                                  public string Nome { get; set; } = string.Empty;
                              }
                              """;

        var generator = new DFeSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            "TestCompilationSignEmptyElement",
            [CSharpSyntaxTree.ParseText(Source)],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DFeRootAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DFeSignDocument<>).Assembly.Location),
                MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("System.Runtime").Location)
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var errors = runResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        await Assert.That(errors).IsNotEmpty();
        await Assert.That(errors.Any(e => e.Id == "DFE0003")).IsTrue();

        var generatedFiles = runResult.GeneratedTrees.Select(t => System.IO.Path.GetFileName(t.FilePath)).ToList();
        await Assert.That(generatedFiles).IsEmpty();
    }

    [Test]
    public async Task ShouldGenerateSerializerWhenDFeSignDocumentHasDFeSignInfoElement()
    {
        const string Source = """
                              using OpenAC.Net.DFe.Core.Attributes;
                              using OpenAC.Net.DFe.Core.Document;
                              using OpenAC.Net.DFe.Core.Serializer;

                              namespace MyTestNamespace;

                              [DFeRoot("NFe")]
                              [DFeSignInfoElement("infNFe")]
                              public partial class DocumentoComSignInfo : DFeSignDocument<DocumentoComSignInfo>
                              {
                                  [DFeElement(TipoCampo.Str, "xNome")]
                                  public string Nome { get; set; } = string.Empty;
                              }
                              """;

        var generator = new DFeSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            "TestCompilationWithSignInfo",
            [CSharpSyntaxTree.ParseText(Source)],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DFeRootAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DFeSignDocument<>).Assembly.Location),
                MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("System.Runtime").Location)
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        await Assert.That(runResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)).IsEmpty();

        var generatedFiles = runResult.GeneratedTrees.Select(t => System.IO.Path.GetFileName(t.FilePath)).ToList();
        await Assert.That(generatedFiles).Contains("MyTestNamespace_DocumentoComSignInfo.DFe.g.cs");
    }

    [Test]
    public async Task ShouldTransitvelyDiscoverChildClassWithoutDFeElement()
    {
        const string Source = """
                              using OpenAC.Net.DFe.Core.Attributes;
                              using OpenAC.Net.DFe.Core.Serializer;

                              namespace MyTestNamespace;

                              [DFeRoot("NFe")]
                              public partial class DocumentoRoot
                              {
                                  // Propriedade de tipo classe SEM [DFeElement] explícito
                                  public Identificacao Ide { get; set; } = new();
                              }

                              public partial class Identificacao
                              {
                                  [DFeElement(TipoCampo.Int, "cNF")]
                                  public int Codigo { get; set; }
                              }
                              """;

        var generator = new DFeSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            "TestCompilationTransitive",
            [CSharpSyntaxTree.ParseText(Source)],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DFeRootAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("System.Runtime").Location)
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        await Assert.That(runResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)).IsEmpty();

        var generatedFiles = runResult.GeneratedTrees.Select(t => System.IO.Path.GetFileName(t.FilePath)).ToList();
        await Assert.That(generatedFiles).Contains("MyTestNamespace_DocumentoRoot.DFe.g.cs");
        await Assert.That(generatedFiles).Contains("MyTestNamespace_Identificacao.DFe.g.cs");

        var docRootTree = runResult.GeneratedTrees.First(t => t.FilePath.EndsWith("MyTestNamespace_DocumentoRoot.DFe.g.cs"));
        var docRootCode = docRootTree.GetText().ToString();
        await Assert.That(docRootCode).Contains("this.Ide.WriteToXml(\"Ide\", null, options)");
    }
}

