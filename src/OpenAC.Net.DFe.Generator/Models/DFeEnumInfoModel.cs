using System;

namespace OpenAC.Net.DFe.Generator.Models;

/// <summary>
/// Modelo de um membro individual de enumeração com seu valor de serialização XML correspondente.
/// </summary>
public sealed record DFeEnumMemberModel(string MemberName, string XmlValue) : IEquatable<DFeEnumMemberModel>;

/// <summary>
/// Modelo de informações de uma enumeração mapeada para serialização XML no DFe.
/// </summary>
public sealed record DFeEnumInfoModel(
    string EnumFullName,
    EquatableArray<DFeEnumMemberModel> Members) : IEquatable<DFeEnumInfoModel>;
