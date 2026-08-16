using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using OpenAC.Net.DFe.Generator.Models;

namespace OpenAC.Net.DFe.Generator.Parser;

/// <summary>
/// Métodos de extensão utilitários para inspeção de símbolos do Roslyn (<see cref="ISymbol"/>, <see cref="ITypeSymbol"/>, <see cref="AttributeData"/>).
/// </summary>
public static class SymbolExtensions
{
    /// <summary>
    /// Obtém o primeiro atributo com o nome especificado.
    /// </summary>
    public static AttributeData? GetAttribute(this ISymbol symbol, string attributeName)
    {
        return symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass != null &&
            (a.AttributeClass.Name == attributeName || a.AttributeClass.Name == $"{attributeName}Attribute"));
    }

    /// <summary>
    /// Obtém todos os atributos com o nome especificado.
    /// </summary>
    public static IEnumerable<AttributeData> GetAttributes(this ISymbol symbol, string attributeName)
    {
        return symbol.GetAttributes().Where(a => a.AttributeClass != null &&
            (a.AttributeClass.Name == attributeName || a.AttributeClass.Name == $"{attributeName}Attribute"));
    }

    public static bool HasAttribute(this ISymbol symbol, string attributeName)
    {
        return symbol.GetAttribute(attributeName) != null;
    }

    public static T? GetConstructorArgument<T>(this AttributeData attributeData, int index)
    {
        if (attributeData.ConstructorArguments.Length > index)
        {
            var arg = attributeData.ConstructorArguments[index];
            if (arg.Value is T val) return val;
            if (arg.Value != null)
            {
                try
                {
                    return (T)Convert.ChangeType(arg.Value, typeof(T));
                }
                catch
                {
                    return default;
                }
            }
        }
        return default;
    }

    public static T? GetNamedArgument<T>(this AttributeData attributeData, string name, T? defaultValue = default)
    {
        var named = attributeData.NamedArguments.FirstOrDefault(kvp => kvp.Key == name);
        if (!named.Equals(default(KeyValuePair<string, TypedConstant>)))
        {
            if (named.Value.Value is T val) return val;
            if (named.Value.Value != null)
            {
                try
                {
                    return (T)Convert.ChangeType(named.Value.Value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
        }
        return defaultValue;
    }

    public static bool IsNullable(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated) return true;
        if (typeSymbol is INamedTypeSymbol named && named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T) return true;
        return false;
    }

    public static ITypeSymbol UnwrapNullable(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol named && named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return named.TypeArguments[0];
        }
        return typeSymbol;
    }

    public static DFeTypeKind GetDFeTypeKind(this ITypeSymbol typeSymbol)
    {
        var unwrapped = typeSymbol.UnwrapNullable();

        if (IsPrimitive(unwrapped)) return DFeTypeKind.Primitive;
        if (unwrapped.TypeKind == TypeKind.Enum) return DFeTypeKind.Enum;
        if (IsStream(unwrapped)) return DFeTypeKind.Stream;
        if (IsDictionary(unwrapped)) return DFeTypeKind.Dictionary;
        if (IsCollection(unwrapped)) return DFeTypeKind.Collection;
        if (IsValueElement(unwrapped)) return DFeTypeKind.ValueElement;
        if (unwrapped.TypeKind == TypeKind.Interface || unwrapped.IsAbstract) return DFeTypeKind.InterfaceOrAbstract;
        if (unwrapped.HasAttribute("DFeRoot")) return DFeTypeKind.RootClass;
        if (unwrapped.TypeKind == TypeKind.Class) return DFeTypeKind.Class;

        return DFeTypeKind.Other;
    }

    public static bool IsPrimitive(this ITypeSymbol typeSymbol)
    {
        var unwrapped = typeSymbol.UnwrapNullable();

        if (unwrapped.SpecialType is
            SpecialType.System_String or
            SpecialType.System_Char or
            SpecialType.System_SByte or
            SpecialType.System_Int16 or
            SpecialType.System_Int32 or
            SpecialType.System_Int64 or
            SpecialType.System_Byte or
            SpecialType.System_UInt16 or
            SpecialType.System_UInt32 or
            SpecialType.System_UInt64 or
            SpecialType.System_Double or
            SpecialType.System_Single or
            SpecialType.System_Decimal or
            SpecialType.System_Boolean or
            SpecialType.System_DateTime)
        {
            return true;
        }

        var fullDisplay = unwrapped.ToDisplayString();
        return fullDisplay is "System.DateTimeOffset" or "System.Guid" or "System.TimeSpan";
    }

    public static bool IsStream(this ITypeSymbol typeSymbol)
    {
        var unwrapped = typeSymbol.UnwrapNullable();
        return unwrapped.ToDisplayString() is "System.IO.Stream" ||
               InheritsFrom(unwrapped, "System.IO.Stream");
    }

    public static bool IsCollection(this ITypeSymbol typeSymbol)
    {
        var unwrapped = typeSymbol.UnwrapNullable();
        if (unwrapped.SpecialType == SpecialType.System_String) return false;
        if (unwrapped is IArrayTypeSymbol) return true;

        if (unwrapped is INamedTypeSymbol named)
        {
            var full = named.OriginalDefinition.ToDisplayString();
            if (full is "System.Collections.Generic.List<T>" or
                "System.Collections.Generic.IList<T>" or
                "System.Collections.Generic.ICollection<T>" or
                "System.Collections.Generic.IEnumerable<T>" or
                "OpenAC.Net.DFe.Core.Collection.DFeCollection<T>")
            {
                return true;
            }

            return named.AllInterfaces.Any(i =>
                i.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>");
        }

        return false;
    }

    public static ITypeSymbol? GetCollectionItemType(this ITypeSymbol typeSymbol)
    {
        var unwrapped = typeSymbol.UnwrapNullable();
        if (unwrapped.SpecialType == SpecialType.System_String) return null;
        if (unwrapped is IArrayTypeSymbol arrayType) return arrayType.ElementType;

        if (unwrapped is INamedTypeSymbol named)
        {
            if (named.OriginalDefinition.ToDisplayString() is
                "System.Collections.Generic.List<T>" or
                "System.Collections.Generic.IList<T>" or
                "System.Collections.Generic.ICollection<T>" or
                "System.Collections.Generic.IEnumerable<T>" or
                "OpenAC.Net.DFe.Core.Collection.DFeCollection<T>")
            {
                return named.TypeArguments.Length > 0 ? named.TypeArguments[0] : null;
            }

            foreach (var iface in named.AllInterfaces)
            {
                if (iface.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>")
                {
                    return iface.TypeArguments[0];
                }
            }
        }

        return null;
    }

    public static bool IsDictionary(this ITypeSymbol typeSymbol)
    {
        var unwrapped = typeSymbol.UnwrapNullable();
        if (unwrapped is INamedTypeSymbol named)
        {
            var full = named.OriginalDefinition.ToDisplayString();
            if (full is "System.Collections.Generic.Dictionary<TKey, TValue>" or
                "System.Collections.Generic.IDictionary<TKey, TValue>")
            {
                return true;
            }

            return named.AllInterfaces.Any(i =>
                i.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IDictionary<TKey, TValue>");
        }
        return false;
    }

    public static (ITypeSymbol KeyType, ITypeSymbol ValueType)? GetDictionaryTypes(this ITypeSymbol typeSymbol)
    {
        var unwrapped = typeSymbol.UnwrapNullable();
        if (unwrapped is INamedTypeSymbol named)
        {
            if (named.TypeArguments.Length == 2)
            {
                return (named.TypeArguments[0], named.TypeArguments[1]);
            }

            foreach (var iface in named.AllInterfaces)
            {
                if (iface.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IDictionary<TKey, TValue>")
                {
                    return (iface.TypeArguments[0], iface.TypeArguments[1]);
                }
            }
        }
        return null;
    }

    public static bool IsValueElement(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol named) return false;
        return named.GetMembers().OfType<IPropertySymbol>().Any(p => p.HasAttribute("DFeItemValue"));
    }

    public static bool InheritsFrom(ITypeSymbol? typeSymbol, string baseTypeFullName)
    {
        var current = typeSymbol;
        while (current != null)
        {
            if (current.ToDisplayString().StartsWith(baseTypeFullName) ||
                current.OriginalDefinition.ToDisplayString().StartsWith(baseTypeFullName))
            {
                return true;
            }
            current = current.BaseType;
        }
        return false;
    }

    public static IEnumerable<IPropertySymbol> GetAllProperties(this INamedTypeSymbol classSymbol)
    {
        var properties = new List<IPropertySymbol>();
        var current = classSymbol;

        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (!properties.Any(p => p.Name == member.Name))
                {
                    properties.Add(member);
                }
            }
            current = current.BaseType;
        }

        return properties;
    }
}
