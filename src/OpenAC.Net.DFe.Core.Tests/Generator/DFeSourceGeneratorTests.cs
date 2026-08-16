using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OpenAC.Net.DFe.Core.Attributes;
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
}
