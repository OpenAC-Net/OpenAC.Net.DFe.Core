using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenAC.Net.DFe.Generator.Models;

namespace OpenAC.Net.DFe.Generator.Parser;

/// <summary>
/// Analisador sintático e semântico do Roslyn que extrai metadados de classes DFe (<see cref="DFeClassModel"/>).
/// </summary>
public static class DFeModelParser
{
    /// <summary>
    /// Verifica se o símbolo de classe é uma classe Raiz (possui [DFeRoot] ou herda de DFeDocument/DFeSignDocument).
    /// </summary>
    public static bool IsRootClass(INamedTypeSymbol classSymbol)
    {
        if (classSymbol.TypeKind != TypeKind.Class || classSymbol.IsAbstract)
            return false;

        if (IsIgnoredType(classSymbol))
            return false;

        if (classSymbol.HasAttribute("DFeRoot"))
            return true;

        if (SymbolExtensions.InheritsFrom(classSymbol, "OpenAC.Net.DFe.Core.Document.DFeDocument"))
            return true;

        return false;
    }

    /// <summary>
    /// Verifica se a classe deve ser ignorada pelo analisador DFe (System, Attributes, Exceptions, Configs, etc).
    /// </summary>
    public static bool IsIgnoredType(INamedTypeSymbol classSymbol)
    {
        if (classSymbol.ContainingType != null)
            return true;

        var nsString = classSymbol.ContainingNamespace.ToDisplayString();
        if (SymbolExtensions.InheritsFrom(classSymbol, "System.Attribute") ||
            SymbolExtensions.InheritsFrom(classSymbol, "System.Exception") ||
            SymbolExtensions.InheritsFrom(classSymbol, "System.EventArgs") ||
            SymbolExtensions.InheritsFrom(classSymbol, "OpenAC.Net.DFe.Core.Common.DFeConfigBase") ||
            SymbolExtensions.InheritsFrom(classSymbol, "OpenAC.Net.DFe.Core.Common.DFeGeralConfigBase") ||
            SymbolExtensions.InheritsFrom(classSymbol, "OpenAC.Net.DFe.Core.Common.DFeWebserviceConfigBase") ||
            SymbolExtensions.InheritsFrom(classSymbol, "OpenAC.Net.DFe.Core.Common.DFeCertificadosConfigBase") ||
            SymbolExtensions.InheritsFrom(classSymbol, "OpenAC.Net.DFe.Core.Common.DFeArquivosConfigBase") ||
            SymbolExtensions.InheritsFrom(classSymbol, "OpenAC.Net.DFe.Core.Common.DFeOptionsBase") ||
            SymbolExtensions.InheritsFrom(classSymbol, "OpenAC.Net.DFe.Core.Service.DFeServiceClientBase") ||
            nsString.StartsWith("System") ||
            nsString.StartsWith("OpenAC.Net.DFe.Core.Collection") ||
            nsString.StartsWith("OpenAC.Net.DFe.Core.Serializer"))
            return true;

        return false;
    }

    /// <summary>
    /// Analisa a sintaxe e modelo semântico a partir do contexto do gerador incremental.
    /// </summary>
    public static DFeClassModel? ParseClass(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
            return null;

        return ParseClass(classSymbol, out _);
    }

    /// <summary>
    /// Analisa os atributos e propriedades de um símbolo de classe (<see cref="INamedTypeSymbol"/>) construindo o modelo <see cref="DFeClassModel"/>.
    /// </summary>
    public static DFeClassModel? ParseClass(INamedTypeSymbol classSymbol)
    {
        return ParseClass(classSymbol, out _);
    }

    /// <summary>
    /// Analisa os atributos e propriedades de um símbolo de classe (<see cref="INamedTypeSymbol"/>) construindo o modelo <see cref="DFeClassModel"/> e retornando os tipos filhos referenciados.
    /// </summary>
    public static DFeClassModel? ParseClass(INamedTypeSymbol classSymbol, out List<INamedTypeSymbol> referencedTypes)
    {
        referencedTypes = new List<INamedTypeSymbol>();

        if (classSymbol.TypeKind != TypeKind.Class || classSymbol.IsAbstract)
            return null;

        if (IsIgnoredType(classSymbol))
            return null;

        var nsString = classSymbol.ContainingNamespace.ToDisplayString();
        var ns = classSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : nsString;
        var className = classSymbol.Name;
        var fullTypeName = classSymbol.ToCleanDisplayString();

        var isPartial = false;
        foreach (var syntaxRef in classSymbol.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax() is ClassDeclarationSyntax classDecl &&
                classDecl.Modifiers.Any(m => m.Text == "partial"))
            {
                isPartial = true;
                break;
            }
        }

        var isGeneric = classSymbol.IsGenericType;
        var genericTypeSig = isGeneric ? "<" + string.Join(", ", classSymbol.TypeParameters.Select(tp => tp.Name)) + ">" : string.Empty;

        var constraints = new List<string>();
        if (isGeneric)
        {
            foreach (var tp in classSymbol.TypeParameters)
            {
                var tpConstraints = new List<string>();
                if (tp.HasReferenceTypeConstraint) tpConstraints.Add("class");
                if (tp.HasValueTypeConstraint) tpConstraints.Add("struct");
                if (tp.HasNotNullConstraint) tpConstraints.Add("notnull");
                if (tp.HasUnmanagedTypeConstraint) tpConstraints.Add("unmanaged");

                foreach (var constraintType in tp.ConstraintTypes)
                {
                    tpConstraints.Add($"global::{constraintType.ToCleanDisplayString()}");
                }

                if (tp.HasConstructorConstraint) tpConstraints.Add("new()");

                if (tpConstraints.Count > 0)
                {
                    constraints.Add($"where {tp.Name} : {string.Join(", ", tpConstraints)}");
                }
            }
        }

        var genericConstraintsClause = string.Join(" ", constraints);

        var isDFeDocument = SymbolExtensions.InheritsFrom(classSymbol, "OpenAC.Net.DFe.Core.Document.DFeDocument");
        var isDFeSignDocument = SymbolExtensions.InheritsFrom(classSymbol, "OpenAC.Net.DFe.Core.Document.DFeSignDocument");
        var signInfoAttr = classSymbol.GetAttribute("DFeSignInfoElement");
        var rootAttr = classSymbol.GetAttribute("DFeRoot");
        var isRoot = rootAttr != null || isDFeDocument;

        var diagnostics = new List<DFeDiagnosticInfo>();

        if (isDFeDocument && rootAttr == null)
        {
            var loc = classSymbol.Locations.FirstOrDefault();
            var lineSpan = loc?.GetLineSpan() ?? default;
            var baseTypeName = isDFeSignDocument ? "DFeSignDocument" : "DFeDocument";
            diagnostics.Add(new DFeDiagnosticInfo(
                Id: "DFE0002",
                Title: "Atributo DFeRoot obrigatório",
                Message: $"A classe '{className}' herda de {baseTypeName} e precisa obrigatoriamente possuir o atributo [DFeRoot].",
                Severity: DiagnosticSeverity.Error,
                FilePath: lineSpan.Path,
                StartLine: lineSpan.StartLinePosition.Line,
                StartCharacter: lineSpan.StartLinePosition.Character,
                EndLine: lineSpan.EndLinePosition.Line,
                EndCharacter: lineSpan.EndLinePosition.Character
            ));
        }

        if (isDFeSignDocument && signInfoAttr == null)
        {
            var loc = classSymbol.Locations.FirstOrDefault();
            var lineSpan = loc?.GetLineSpan() ?? default;
            diagnostics.Add(new DFeDiagnosticInfo(
                Id: "DFE0001",
                Title: "Atributo DFeSignInfoElement obrigatório",
                Message: $"A classe '{className}' herda de DFeSignDocument e precisa obrigatoriamente possuir o atributo [DFeSignInfoElement].",
                Severity: DiagnosticSeverity.Error,
                FilePath: lineSpan.Path,
                StartLine: lineSpan.StartLinePosition.Line,
                StartCharacter: lineSpan.StartLinePosition.Character,
                EndLine: lineSpan.EndLinePosition.Line,
                EndCharacter: lineSpan.EndLinePosition.Character
            ));
        }
        else if (isDFeSignDocument && signInfoAttr != null)
        {
            var signElement = signInfoAttr.GetConstructorArgument<string>(0) ?? signInfoAttr.GetNamedArgument<string>("SignElement");
            if (string.IsNullOrWhiteSpace(signElement))
            {
                var loc = classSymbol.Locations.FirstOrDefault();
                var lineSpan = loc?.GetLineSpan() ?? default;
                diagnostics.Add(new DFeDiagnosticInfo(
                    Id: "DFE0003",
                    Title: "Propriedade SignElement obrigatória em DFeSignInfoElement",
                    Message: $"A classe '{className}' herda de DFeSignDocument e o atributo [DFeSignInfoElement] precisa ter o 'SignElement' informado.",
                    Severity: DiagnosticSeverity.Error,
                    FilePath: lineSpan.Path,
                    StartLine: lineSpan.StartLinePosition.Line,
                    StartCharacter: lineSpan.StartLinePosition.Character,
                    EndLine: lineSpan.EndLinePosition.Line,
                    EndCharacter: lineSpan.EndLinePosition.Character
                ));
            }
        }

        string? rootTag = null;
        string? rootNs = null;

        if (rootAttr != null)
        {
            rootTag = rootAttr.GetConstructorArgument<string>(0) ?? rootAttr.GetNamedArgument<string>("Name");
            rootNs = rootAttr.GetNamedArgument<string>("Namespace");
        }

        var isValueElement = classSymbol.IsValueElement();
        var isCollectionClass = false;
        string? collectionItemType = null;
        if (!isGeneric)
        {
            var itemTypeSymbol = classSymbol.GetCollectionItemType();
            if (itemTypeSymbol != null && itemTypeSymbol.TypeKind != TypeKind.TypeParameter)
            {
                isCollectionClass = true;
                collectionItemType = itemTypeSymbol.ToCleanDisplayString();
            }
        }

        var allProperties = classSymbol.GetAllProperties();
        var propertyModels = new List<DFePropertyModel>();
        DFePropertyModel? valueElementPropModel = null;

        foreach (var prop in allProperties)
        {
            if (prop.HasAttribute("DFeIgnore") || prop.IsReadOnly || prop.IsWriteOnly || prop.IsStatic)
                continue;

            var propModel = ParseProperty(prop, classSymbol);
            if (propModel == null) continue;

            if (propModel.PropertyKind == DFePropertyKind.ItemValue)
            {
                valueElementPropModel = propModel;
            }

            propertyModels.Add(propModel);
            CollectReferencedTypes(prop, referencedTypes);
        }

        // If it has no root tag and no DFe properties and is not a ValueElement and not a collection class, it's not a DFe class
        if (!isRoot && !isValueElement && !isCollectionClass && propertyModels.Count == 0 && diagnostics.Count == 0)
            return null;

        var sortedProperties = propertyModels.OrderBy(p => p.Ordem).ToImmutableArray();

        return new DFeClassModel(
            Namespace: ns,
            ClassName: className,
            TypeFullName: fullTypeName,
            IsPartial: isPartial,
            IsRoot: isRoot,
            IsDFeDocument: isDFeDocument,
            RootTag: rootTag,
            RootNamespace: rootNs,
            IsValueElement: isValueElement,
            ValueElementProperty: valueElementPropModel,
            IsCollectionClass: isCollectionClass,
            CollectionItemType: collectionItemType,
            IsGeneric: isGeneric,
            GenericTypeSignature: genericTypeSig,
            GenericTypeConstraintsClause: genericConstraintsClause,
            Properties: new EquatableArray<DFePropertyModel>(sortedProperties),
            Diagnostics: new EquatableArray<DFeDiagnosticInfo>(diagnostics.ToImmutableArray())
        );
    }

    private static DFePropertyModel? ParseProperty(IPropertySymbol prop, INamedTypeSymbol ownerClass)
    {
        var propName = prop.Name;
        var propType = prop.Type;
        var typeFullName = propType.ToCleanDisplayString();
        var isNullable = propType.IsNullable();
        var typeKind = propType.GetDFeTypeKind();

        var shouldSerializeMethod = ownerClass.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(m => m.Name == $"ShouldSerialize{propName}");
        var hasShouldSerialize = shouldSerializeMethod != null && shouldSerializeMethod.DeclaredAccessibility != Accessibility.Private;

        var attrAttribute = prop.GetAttribute("DFeAttribute");
        var elemAttribute = prop.GetAttribute("DFeElement");
        var colAttribute = prop.GetAttribute("DFeCollection");
        var dictAttribute = prop.GetAttribute("DFeDictionary");
        var itemValueAttribute = prop.GetAttribute("DFeItemValue");
        var itemAttributes = prop.GetAttributes("DFeItem").ToList();

        DFePropertyKind propertyKind = DFePropertyKind.None;
        string tagName = propName;
        string? tagNamespace = null;
        string id = string.Empty;
        string descricao = string.Empty;
        int ordem = 0;
        int min = 0;
        int max = 0;
        int minSize = 0;
        int maxSize = 0;
        OcorrenciaModel ocorrencia = OcorrenciaModel.NaoObrigatoria;
        TipoCampoModel tipoCampo = TipoCampoModel.Str;
        bool useCData = false;

        string? collectionItemType = null;
        DFeTypeKind collectionItemTypeKind = DFeTypeKind.Other;
        bool collectionItemTypeIsNullable = false;
        string? collectionContainerTag = null;
        DFeDictionaryMemberModel? dictKey = null;
        DFeDictionaryMemberModel? dictValue = null;
        string? dictKeyType = null;
        string? dictValType = null;

        if (attrAttribute != null)
        {
            propertyKind = DFePropertyKind.Attribute;
            tipoCampo = ExtractTipoCampo(attrAttribute);
            tagName = attrAttribute.GetConstructorArgument<string>(1) ?? attrAttribute.GetConstructorArgument<string>(0) ?? attrAttribute.GetNamedArgument<string>("Name") ?? propName;
            id = attrAttribute.GetNamedArgument<string>("Id") ?? string.Empty;
            descricao = attrAttribute.GetNamedArgument<string>("Descricao") ?? string.Empty;
            ordem = attrAttribute.GetNamedArgument<int>("Ordem", 0);
            min = attrAttribute.GetNamedArgument<int>("Min", 0);
            max = attrAttribute.GetNamedArgument<int>("Max", 0);
            ocorrencia = (OcorrenciaModel)attrAttribute.GetNamedArgument<int>("Ocorrencia", 0);
        }
        else if (colAttribute != null)
        {
            propertyKind = DFePropertyKind.Collection;
            tipoCampo = ExtractTipoCampo(colAttribute);
            tagName = colAttribute.GetConstructorArgument<string>(1) ?? colAttribute.GetConstructorArgument<string>(0) ?? colAttribute.GetNamedArgument<string>("Name") ?? propName;
            collectionContainerTag = tagName;
            tagNamespace = colAttribute.GetNamedArgument<string>("Namespace");
            id = colAttribute.GetNamedArgument<string>("Id") ?? string.Empty;
            descricao = colAttribute.GetNamedArgument<string>("Descricao") ?? string.Empty;
            ordem = colAttribute.GetNamedArgument<int>("Ordem", 0);
            min = colAttribute.GetNamedArgument<int>("Min", 0);
            max = colAttribute.GetNamedArgument<int>("Max", 0);
            minSize = colAttribute.GetNamedArgument<int>("MinSize", 0);
            maxSize = colAttribute.GetNamedArgument<int>("MaxSize", 0);
            ocorrencia = (OcorrenciaModel)colAttribute.GetNamedArgument<int>("Ocorrencia", 0);

            var itemTypeSymbol = propType.GetCollectionItemType();
            if (itemTypeSymbol != null)
            {
                collectionItemType = itemTypeSymbol.ToCleanDisplayString();
                collectionItemTypeKind = itemTypeSymbol.GetDFeTypeKind();
                collectionItemTypeIsNullable = itemTypeSymbol.IsNullable();
            }
        }
        else if (dictAttribute != null)
        {
            propertyKind = DFePropertyKind.Dictionary;
            tagName = dictAttribute.GetConstructorArgument<string>(0) ?? dictAttribute.GetNamedArgument<string>("Name") ?? propName;
            tagNamespace = dictAttribute.GetNamedArgument<string>("Namespace");
            id = dictAttribute.GetNamedArgument<string>("Id") ?? string.Empty;
            descricao = dictAttribute.GetNamedArgument<string>("Descricao") ?? string.Empty;
            ordem = dictAttribute.GetNamedArgument<int>("Ordem", 0);
            minSize = dictAttribute.GetNamedArgument<int>("MinSize", 0);
            maxSize = dictAttribute.GetNamedArgument<int>("MaxSize", 0);
            ocorrencia = (OcorrenciaModel)dictAttribute.GetNamedArgument<int>("Ocorrencia", 0);

            var dictTypes = propType.GetDictionaryTypes();
            if (dictTypes != null)
            {
                dictKeyType = dictTypes.Value.KeyType.ToCleanDisplayString();
                dictValType = dictTypes.Value.ValueType.ToCleanDisplayString();
            }

            var keyAttr = prop.GetAttribute("DFeDictionaryKey");
            var valAttr = prop.GetAttribute("DFeDictionaryValue");

            if (keyAttr != null)
            {
                dictKey = new DFeDictionaryMemberModel(
                    Tipo: ExtractTipoCampo(keyAttr),
                    Name: keyAttr.GetConstructorArgument<string>(1) ?? keyAttr.GetConstructorArgument<string>(0) ?? keyAttr.GetNamedArgument<string>("Name") ?? "Key",
                    AsAttribute: keyAttr.GetNamedArgument<bool>("AsAttribute", false),
                    Min: keyAttr.GetNamedArgument<int>("Min", 0),
                    Max: keyAttr.GetNamedArgument<int>("Max", 0),
                    Ocorrencia: (OcorrenciaModel)keyAttr.GetNamedArgument<int>("Ocorrencia", 0)
                );
            }

            if (valAttr != null)
            {
                dictValue = new DFeDictionaryMemberModel(
                    Tipo: ExtractTipoCampo(valAttr),
                    Name: valAttr.GetConstructorArgument<string>(1) ?? valAttr.GetConstructorArgument<string>(0) ?? valAttr.GetNamedArgument<string>("Name") ?? "Value",
                    AsAttribute: valAttr.GetNamedArgument<bool>("AsAttribute", false),
                    Min: valAttr.GetNamedArgument<int>("Min", 0),
                    Max: valAttr.GetNamedArgument<int>("Max", 0),
                    Ocorrencia: (OcorrenciaModel)valAttr.GetNamedArgument<int>("Ocorrencia", 0)
                );
            }
        }
        else if (itemValueAttribute != null)
        {
            propertyKind = DFePropertyKind.ItemValue;
            tipoCampo = ExtractTipoCampo(itemValueAttribute);
            min = itemValueAttribute.GetNamedArgument<int>("Min", 0);
            max = itemValueAttribute.GetNamedArgument<int>("Max", 0);
            ocorrencia = (OcorrenciaModel)itemValueAttribute.GetNamedArgument<int>("Ocorrencia", 0);
        }
        else if (elemAttribute != null)
        {
            propertyKind = DFePropertyKind.Element;
            tipoCampo = ExtractTipoCampo(elemAttribute);
            tagName = elemAttribute.GetConstructorArgument<string>(1) ?? elemAttribute.GetConstructorArgument<string>(0) ?? elemAttribute.GetNamedArgument<string>("Name") ?? propName;
            tagNamespace = elemAttribute.GetNamedArgument<string>("Namespace");
            id = elemAttribute.GetNamedArgument<string>("Id") ?? string.Empty;
            descricao = elemAttribute.GetNamedArgument<string>("Descricao") ?? string.Empty;
            ordem = elemAttribute.GetNamedArgument<int>("Ordem", 0);
            min = elemAttribute.GetNamedArgument<int>("Min", 0);
            max = elemAttribute.GetNamedArgument<int>("Max", 0);
            ocorrencia = (OcorrenciaModel)elemAttribute.GetNamedArgument<int>("Ocorrencia", 0);
            useCData = elemAttribute.GetNamedArgument<bool>("UseCData", false);
        }
        else if (itemAttributes.Count > 0)
        {
            propertyKind = typeKind == DFeTypeKind.Collection ? DFePropertyKind.Collection : DFePropertyKind.Interface;
            tagName = propName;

            if (typeKind == DFeTypeKind.Collection)
            {
                var itemTypeSymbol = propType.GetCollectionItemType();
                if (itemTypeSymbol != null)
                {
                    collectionItemType = itemTypeSymbol.ToCleanDisplayString();
                    collectionItemTypeKind = itemTypeSymbol.GetDFeTypeKind();
                    collectionItemTypeIsNullable = itemTypeSymbol.IsNullable();
                }
            }
        }
        else if (typeKind is DFeTypeKind.Class or DFeTypeKind.RootClass or DFeTypeKind.ValueElement)
        {
            // Propriedade cujo tipo é uma classe sem [DFeElement] explícito (e.g. Signature, Xml5, Ide, Emit)
            propertyKind = DFePropertyKind.Element;
            tagName = propName;
            ocorrencia = OcorrenciaModel.NaoObrigatoria;
        }
        else if (typeKind == DFeTypeKind.Collection)
        {
            var itemTypeSymbol = propType.GetCollectionItemType();
            if (itemTypeSymbol != null && (itemTypeSymbol.TypeKind == TypeKind.Class || itemTypeSymbol.TypeKind == TypeKind.Interface))
            {
                propertyKind = DFePropertyKind.Collection;
                tagName = propName;
                collectionContainerTag = propName;
                collectionItemType = itemTypeSymbol.ToCleanDisplayString();
                collectionItemTypeKind = itemTypeSymbol.GetDFeTypeKind();
                collectionItemTypeIsNullable = itemTypeSymbol.IsNullable();
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }

        // Process polymorphic items mappings ([DFeItem(typeof(T), "name")])
        var itemMappings = new List<DFeItemMappingModel>();
        foreach (var itemAttr in itemAttributes)
        {
            var itemTypeArg = itemAttr.ConstructorArguments.Length > 0 ? itemAttr.ConstructorArguments[0].Value as ITypeSymbol : null;
            var itemNameArg = itemAttr.ConstructorArguments.Length > 1 ? itemAttr.ConstructorArguments[1].Value as string : null;
            if (itemTypeArg == null) continue;

            var mapName = itemNameArg ?? itemAttr.GetNamedArgument<string>("Name") ?? itemTypeArg.Name;
            var mapNs = itemAttr.GetNamedArgument<string>("Namespace");
            var mapDesc = itemAttr.GetNamedArgument<string>("Descricao");
            var mapMin = itemAttr.GetNamedArgument<int>("Min", 0);
            var mapMax = itemAttr.GetNamedArgument<int>("Max", 0);
            var mapOcorrencia = (OcorrenciaModel)itemAttr.GetNamedArgument<int>("Ocorrencia", 0);
            var mapTipoCampo = ExtractTipoCampo(itemAttr);

            var mapIsCollection = false;
            string? mapCollectionItemType = null;
            var mapItemTypeSymbol = itemTypeArg.GetCollectionItemType();
            if (mapItemTypeSymbol != null)
            {
                mapIsCollection = true;
                mapCollectionItemType = mapItemTypeSymbol.ToCleanDisplayString();
            }

            var mapTypeKind = itemTypeArg.GetDFeTypeKind();

            itemMappings.Add(new DFeItemMappingModel(
                TypeFullName: itemTypeArg.ToCleanDisplayString(),
                TagName: mapName,
                TagNamespace: mapNs,
                Descricao: mapDesc,
                Min: mapMin,
                Max: mapMax,
                Ocorrencia: mapOcorrencia,
                IsCollection: mapIsCollection,
                CollectionItemType: mapCollectionItemType,
                TypeKind: mapTypeKind,
                TipoCampo: mapTipoCampo
            ));
        }

        // Process Enum info if applicable
        DFeEnumInfoModel? enumInfo = null;
        var targetEnumType = propType.UnwrapNullable();
        if (typeKind == DFeTypeKind.Collection && collectionItemType != null)
        {
            var itemType = propType.GetCollectionItemType()?.UnwrapNullable();
            if (itemType?.TypeKind == TypeKind.Enum)
            {
                targetEnumType = itemType;
            }
        }
        else if (typeKind == DFeTypeKind.Dictionary && dictKeyType != null)
        {
            var dictTypes = propType.GetDictionaryTypes();
            if (dictTypes?.KeyType.TypeKind == TypeKind.Enum)
            {
                targetEnumType = dictTypes.Value.KeyType;
            }
        }

        if (targetEnumType != null && targetEnumType.TypeKind == TypeKind.Enum && targetEnumType is INamedTypeSymbol enumSymbol)
        {
            var members = new List<DFeEnumMemberModel>();
            foreach (var member in enumSymbol.GetMembers().OfType<IFieldSymbol>())
            {
                if (member.IsStatic && member.ConstantValue != null)
                {
                    var enumAttr = member.GetAttribute("DFeEnum");
                    var xmlVal = enumAttr?.GetConstructorArgument<string>(0) ?? enumAttr?.GetNamedArgument<string>("Value") ?? member.Name;
                    members.Add(new DFeEnumMemberModel(member.Name, xmlVal));
                }
            }
            enumInfo = new DFeEnumInfoModel(targetEnumType.ToCleanDisplayString(), new EquatableArray<DFeEnumMemberModel>(members));
        }

        return new DFePropertyModel(
            Name: propName,
            TypeFullName: typeFullName,
            TypeKind: typeKind,
            PropertyKind: propertyKind,
            TagName: tagName,
            TagNamespace: tagNamespace,
            Id: id,
            Descricao: descricao,
            Ordem: ordem,
            Min: min,
            Max: max,
            MinSize: minSize,
            MaxSize: maxSize,
            Ocorrencia: ocorrencia,
            TipoCampo: tipoCampo,
            UseCData: useCData,
            IsNullable: isNullable,
            HasShouldSerialize: hasShouldSerialize,
            CollectionItemType: collectionItemType,
            CollectionItemTypeKind: collectionItemTypeKind,
            CollectionItemTypeIsNullable: collectionItemTypeIsNullable,
            CollectionContainerTagName: collectionContainerTag,
            DictionaryKey: dictKey,
            DictionaryValue: dictValue,
            DictionaryKeyType: dictKeyType,
            DictionaryValueType: dictValType,
            ItemMappings: new EquatableArray<DFeItemMappingModel>(itemMappings),
            EnumInfo: enumInfo
        );
    }

    private static void CollectReferencedTypes(IPropertySymbol prop, List<INamedTypeSymbol> referencedTypes)
    {
        void AddIfClass(ITypeSymbol? type)
        {
            if (type == null) return;
            var unwrapped = type.UnwrapNullable();
            if (unwrapped is INamedTypeSymbol named &&
                named.TypeKind == TypeKind.Class &&
                !named.IsAbstract &&
                !IsIgnoredType(named) &&
                named.SpecialType == SpecialType.None)
            {
                referencedTypes.Add(named);
            }
        }

        var propType = prop.Type.UnwrapNullable();

        // 1. Direct property type
        AddIfClass(propType);

        // 2. Collection item type
        var colItemType = propType.GetCollectionItemType();
        AddIfClass(colItemType);

        // 3. Dictionary types
        var dictTypes = propType.GetDictionaryTypes();
        if (dictTypes != null)
        {
            AddIfClass(dictTypes.Value.KeyType);
            AddIfClass(dictTypes.Value.ValueType);
        }

        // 4. DFeItem polymorphic types
        foreach (var itemAttr in prop.GetAttributes("DFeItem"))
        {
            if (itemAttr.ConstructorArguments.Length > 0 && itemAttr.ConstructorArguments[0].Value is ITypeSymbol itemType)
            {
                AddIfClass(itemType);
                AddIfClass(itemType.GetCollectionItemType());
            }
        }
    }

    private static TipoCampoModel ExtractTipoCampo(AttributeData attr)
    {
        if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is int intVal)
        {
            return (TipoCampoModel)intVal;
        }
        var named = attr.GetNamedArgument<int>("Tipo", 0);
        return (TipoCampoModel)named;
    }
}
