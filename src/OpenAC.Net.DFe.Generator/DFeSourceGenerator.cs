using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using OpenAC.Net.DFe.Generator.Emitter;
using OpenAC.Net.DFe.Generator.Models;
using OpenAC.Net.DFe.Generator.Parser;

namespace OpenAC.Net.DFe.Generator;

/// <summary>
/// Incremental Source Generator do Roslyn que gera métodos de serialização e deserialização XML para classes DFe em tempo de compilação.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class DFeSourceGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Inicializa o pipeline de geração incremental do Roslyn.
    /// </summary>
    /// <param name="context">Contexto de inicialização do gerador incremental.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsCandidateClass(s),
                transform: static (ctx, _) => Transform(ctx))
            .Where(static m => m != null);

        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses, static (spc, source) =>
        {
            var (_, classes) = source;
            if (classes.IsDefaultOrEmpty) return;

            var distinctClasses = classes.Where(c => c != null).Distinct().ToImmutableArray();

            foreach (var classModel in distinctClasses)
            {
                if (classModel == null) continue;

                var sourceCode = DFeSerializerEmitter.Generate(classModel);
                var safeNs = string.IsNullOrEmpty(classModel.Namespace) ? "Global" : classModel.Namespace.Replace(".", "_");
                var fileName = $"{safeNs}_{classModel.ClassName}.DFe.g.cs";

                spc.AddSource(fileName, SourceText.From(sourceCode, Encoding.UTF8));
            }
        });
    }

    private static bool IsCandidateClass(SyntaxNode node)
    {
        if (node is not ClassDeclarationSyntax classDeclaration)
            return false;

        return !classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword);
    }

    private static DFeClassModel? Transform(GeneratorSyntaxContext ctx)
    {
        if (ctx.Node is ClassDeclarationSyntax classDecl)
        {
            var symbol = ctx.SemanticModel.GetDeclaredSymbol(classDecl);
            if (symbol is INamedTypeSymbol classSymbol)
            {
                return DFeModelParser.ParseClass(classSymbol);
            }
        }
        return null;
    }
}
