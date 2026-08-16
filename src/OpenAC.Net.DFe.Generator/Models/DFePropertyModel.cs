using System;

namespace OpenAC.Net.DFe.Generator.Models;

/// <summary>
/// Modelo semântico representativo de uma propriedade mapeada para XML em uma classe DFe.
/// </summary>
public sealed record DFePropertyModel(
    string Name,
    string TypeFullName,
    DFeTypeKind TypeKind,
    DFePropertyKind PropertyKind,
    string TagName,
    string? TagNamespace,
    string Id,
    string Descricao,
    int Ordem,
    int Min,
    int Max,
    int MinSize,
    int MaxSize,
    OcorrenciaModel Ocorrencia,
    TipoCampoModel TipoCampo,
    bool UseCData,
    bool IsNullable,
    bool HasShouldSerialize,
    string? CollectionItemType,
    DFeTypeKind CollectionItemTypeKind,
    bool CollectionItemTypeIsNullable,
    string? CollectionContainerTagName,
    DFeDictionaryMemberModel? DictionaryKey,
    DFeDictionaryMemberModel? DictionaryValue,
    string? DictionaryKeyType,
    string? DictionaryValueType,
    EquatableArray<DFeItemMappingModel> ItemMappings,
    DFeEnumInfoModel? EnumInfo) : IEquatable<DFePropertyModel>;
