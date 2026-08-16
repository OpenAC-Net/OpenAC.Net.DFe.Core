using System;
using Microsoft.CodeAnalysis;

namespace OpenAC.Net.DFe.Generator.Models;

/// <summary>
/// Modelo equatable para representar diagnósticos emitidos pelo gerador incremental do Roslyn.
/// </summary>
public sealed record DFeDiagnosticInfo(
    string Id,
    string Title,
    string Message,
    DiagnosticSeverity Severity,
    string? FilePath = null,
    int StartLine = 0,
    int StartCharacter = 0,
    int EndLine = 0,
    int EndCharacter = 0
) : IEquatable<DFeDiagnosticInfo>;
