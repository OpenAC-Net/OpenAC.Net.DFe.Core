using System;

namespace OpenAC.Net.DFe.Generator.Models;

/// <summary>
/// Modelo semântico representativo de uma classe declarada para geração do serializador DFe.
/// </summary>
public sealed record DFeClassModel(
    string Namespace,
    string ClassName,
    string TypeFullName,
    bool IsPartial,
    bool IsRoot,
    bool IsDFeDocument,
    string? RootTag,
    string? RootNamespace,
    bool IsValueElement,
    DFePropertyModel? ValueElementProperty,
    bool IsCollectionClass,
    string? CollectionItemType,
    bool IsGeneric,
    string GenericTypeSignature,
    string GenericTypeConstraintsClause,
    EquatableArray<DFePropertyModel> Properties) : IEquatable<DFeClassModel>;
