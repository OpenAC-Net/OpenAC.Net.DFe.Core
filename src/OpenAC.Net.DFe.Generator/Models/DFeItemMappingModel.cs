using System;

namespace OpenAC.Net.DFe.Generator.Models;

/// <summary>
/// Modelo de mapeamento de tipos concretos em coleções ou propriedades polimórficas (<see cref="DFeItemMappingModel"/>).
/// </summary>
public sealed record DFeItemMappingModel(
    string TypeFullName,
    string TagName,
    string? TagNamespace,
    string? Descricao,
    int Min,
    int Max,
    OcorrenciaModel Ocorrencia,
    bool IsCollection = false,
    string? CollectionItemType = null,
    DFeTypeKind TypeKind = DFeTypeKind.Other,
    TipoCampoModel TipoCampo = TipoCampoModel.Str) : IEquatable<DFeItemMappingModel>;

/// <summary>
/// Modelo representativo dos membros de chave ou valor de um dicionário serializado em XML.
/// </summary>
public sealed record DFeDictionaryMemberModel(
    TipoCampoModel Tipo,
    string Name,
    bool AsAttribute,
    int Min,
    int Max,
    OcorrenciaModel Ocorrencia) : IEquatable<DFeDictionaryMemberModel>;
