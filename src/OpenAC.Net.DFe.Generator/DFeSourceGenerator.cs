using System.Collections.Generic;
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
/// Incremental Source Generator do Roslyn que descobre classes Raiz DFe e gera recursivamente métodos de serialização/deserialização XML para todo o grafo de classes filhas em tempo de compilação.
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
        var rootDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsCandidateRootClass(s),
                transform: static (ctx, _) => GetRootClassSymbol(ctx))
            .Where(static s => s != null);

        var compilationAndRoots = context.CompilationProvider.Combine(rootDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndRoots, static (spc, source) =>
        {
            var (_, roots) = source;
            if (roots.IsDefaultOrEmpty) return;

            var queue = new Queue<INamedTypeSymbol>();
            var visited = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            var classModels = new List<DFeClassModel>();

            foreach (var root in roots)
            {
                if (root != null && visited.Add(root))
                {
                    queue.Enqueue(root);
                }
            }

            while (queue.Count > 0)
            {
                var currentClass = queue.Dequeue();
                var classModel = DFeModelParser.ParseClass(currentClass, out var referencedTypes);
                if (classModel == null) continue;

                classModels.Add(classModel);

                foreach (var refType in referencedTypes)
                {
                    if (refType.DeclaringSyntaxReferences.Length > 0 && visited.Add(refType))
                    {
                        queue.Enqueue(refType);
                    }
                }
            }

            foreach (var classModel in classModels)
            {
                var hasErrors = false;
                foreach (var diag in classModel.Diagnostics)
                {
                    var descriptor = new DiagnosticDescriptor(
                        id: diag.Id,
                        title: diag.Title,
                        messageFormat: diag.Message,
                        category: "DFeGenerator",
                        defaultSeverity: diag.Severity,
                        isEnabledByDefault: true);

                    var location = GetLocation(diag);
                    spc.ReportDiagnostic(Diagnostic.Create(descriptor, location));

                    if (diag.Severity == DiagnosticSeverity.Error)
                    {
                        hasErrors = true;
                    }
                }

                if (hasErrors) continue;
                if (!classModel.IsPartial) continue;

                var sourceCode = DFeSerializerEmitter.Generate(classModel);
                var safeNs = string.IsNullOrEmpty(classModel.Namespace) ? "Global" : classModel.Namespace.Replace(".", "_");
                var fileName = $"{safeNs}_{classModel.ClassName}.DFe.g.cs";

                spc.AddSource(fileName, SourceText.From(sourceCode, Encoding.UTF8));
            }
        });
    }

    private static Location GetLocation(DFeDiagnosticInfo info)
    {
        if (string.IsNullOrEmpty(info.FilePath))
            return Location.None;

        return Location.Create(
            info.FilePath!,
            new TextSpan(0, 0),
            new LinePositionSpan(
                new LinePosition(info.StartLine, info.StartCharacter),
                new LinePosition(info.EndLine, info.EndCharacter)
            )
        );
    }

    private static bool IsCandidateRootClass(SyntaxNode node)
    {
        if (node is not ClassDeclarationSyntax classDeclaration)
            return false;

        if (classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword))
            return false;

        if (classDeclaration.Parent is TypeDeclarationSyntax)
            return false;

        // Check for DFeRoot attribute
        if (classDeclaration.AttributeLists.Count > 0)
        {
            foreach (var attrList in classDeclaration.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    var name = attr.Name.ToString();
                    if (name.EndsWith("DFeRoot") || name.EndsWith("DFeRootAttribute"))
                        return true;
                }
            }
        }

        // Check for DFeDocument / DFeSignDocument base class
        if (classDeclaration.BaseList != null)
        {
            foreach (var baseType in classDeclaration.BaseList.Types)
            {
                var baseName = baseType.Type.ToString();
                if (baseName.Contains("DFeDocument") || baseName.Contains("DFeSignDocument"))
                    return true;
            }
        }

        return false;
    }

    private static INamedTypeSymbol? GetRootClassSymbol(GeneratorSyntaxContext ctx)
    {
        if (ctx.Node is ClassDeclarationSyntax classDecl)
        {
            var symbol = ctx.SemanticModel.GetDeclaredSymbol(classDecl);
            if (symbol is INamedTypeSymbol classSymbol && DFeModelParser.IsRootClass(classSymbol))
            {
                return classSymbol;
            }
        }
        return null;
    }
}
