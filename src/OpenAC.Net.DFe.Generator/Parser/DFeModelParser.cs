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
    /// Analisa a sintaxe e modelo semântico a partir do contexto do gerador incremental.
    /// </summary>
    public static DFeClassModel? ParseClass(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
            return null;

        return ParseClass(classSymbol);
    }

    /// <summary>
    /// Analisa os atributos e propriedades de um símbolo de classe (<see cref="INamedTypeSymbol"/>) construindo o modelo <see cref="DFeClassModel"/>.
    /// </summary>
    public static DFeClassModel? ParseClass(INamedTypeSymbol classSymbol)
    {
        if (classSymbol.TypeKind != TypeKind.Class || classSymbol.IsAbstract)
            return null;

        // Skip System types, Attributes, Exceptions, and core infrastructure types
        var nsString = classSymbol.ContainingNamespace.ToDisplayString();
        if (SymbolExtensions.InheritsFrom(classSymbol, "System.Attribute") ||
            SymbolExtensions.InheritsFrom(classSymbol, "System.Exception") ||
            SymbolExtensions.InheritsFrom(classSymbol, "System.EventArgs") ||
            nsString.StartsWith("System") ||
            nsString.StartsWith("OpenAC.Net.DFe.Core.Collection") ||
            nsString.StartsWith("OpenAC.Net.DFe.Core.Serializer"))
            return null;

        var ns = classSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : nsString;
        var className = classSymbol.Name;
        var fullTypeName = classSymbol.ToDisplayString();

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
                    tpConstraints.Add($"global::{constraintType.ToDisplayString()}");
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
        var rootAttr = classSymbol.GetAttribute("DFeRoot");
        var isRoot = rootAttr != null || isDFeDocument;

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
                collectionItemType = itemTypeSymbol.ToDisplayString();
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
        }

        // If it has no root tag and no DFe properties and is not a ValueElement and not a collection class, it's not a DFe class
        if (!isRoot && !isValueElement && !isCollectionClass && propertyModels.Count == 0)
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
            Properties: new EquatableArray<DFePropertyModel>(sortedProperties)
        );
    }

    private static DFePropertyModel? ParseProperty(IPropertySymbol prop, INamedTypeSymbol ownerClass)
    {
        var propName = prop.Name;
        var propType = prop.Type;
        var typeFullName = propType.ToDisplayString();
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
                collectionItemType = itemTypeSymbol.ToDisplayString();
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
                dictKeyType = dictTypes.Value.KeyType.ToDisplayString();
                dictValType = dictTypes.Value.ValueType.ToDisplayString();
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
                    collectionItemType = itemTypeSymbol.ToDisplayString();
                    collectionItemTypeKind = itemTypeSymbol.GetDFeTypeKind();
                    collectionItemTypeIsNullable = itemTypeSymbol.IsNullable();
                }
            }
        }
        else if (typeKind is DFeTypeKind.Class or DFeTypeKind.RootClass or DFeTypeKind.ValueElement)
        {
            // Class property without explicit [DFeElement] (e.g. Signature, Xml5)
            propertyKind = DFePropertyKind.Element;
            tagName = propName;
            ocorrencia = OcorrenciaModel.NaoObrigatoria;
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

            var mapIsCollection = false;
            string? mapCollectionItemType = null;
            var mapItemTypeSymbol = itemTypeArg.GetCollectionItemType();
            if (mapItemTypeSymbol != null)
            {
                mapIsCollection = true;
                mapCollectionItemType = mapItemTypeSymbol.ToDisplayString();
            }

            itemMappings.Add(new DFeItemMappingModel(
                TypeFullName: itemTypeArg.ToDisplayString(),
                TagName: mapName,
                TagNamespace: mapNs,
                Descricao: mapDesc,
                Min: mapMin,
                Max: mapMax,
                Ocorrencia: mapOcorrencia,
                IsCollection: mapIsCollection,
                CollectionItemType: mapCollectionItemType
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
            enumInfo = new DFeEnumInfoModel(targetEnumType.ToDisplayString(), new EquatableArray<DFeEnumMemberModel>(members));
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
