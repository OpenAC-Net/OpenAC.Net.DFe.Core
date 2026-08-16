// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 08-16-2026
//
// Last Modified By : RFTD
// Last Modified On : 08-16-2026
// ***********************************************************************
// <copyright file="DFeSerializerHelper.cs" company="OpenAC .Net">
//		        		   The MIT License (MIT)
//	     		    Copyright (c) 2014-2026 Grupo OpenAC.Net
//
//	 Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//	 The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//	 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// </copyright>
// <summary></summary>
// ***********************************************************************

#nullable enable
using System;
using System.Globalization;
using OpenAC.Net.Core.Extensions;

namespace OpenAC.Net.DFe.Core.Serializer;

/// <summary>
/// Métodos auxiliares de formatação e parsing para serialização e deserialização DFe.
/// </summary>
public static class DFeSerializerHelper
{
    #region Enum Helpers

    /// <summary>
    /// Realiza a conversão de uma string para o valor do enum genérico correspondente.
    /// </summary>
    /// <typeparam name="TEnum">O tipo da enumeração.</typeparam>
    /// <param name="val">A string a ser convertida.</param>
    /// <returns>O valor correspondente na enumeração, ou valor padrão se nulo/inválido.</returns>
    public static TEnum ParseEnum_Generic<TEnum>(string? val) where TEnum : Enum
    {
        if (string.IsNullOrEmpty(val)) return default!;
        try
        {
            return (TEnum)Enum.Parse(typeof(TEnum), val, true);
        }
        catch
        {
            return default!;
        }
    }

    #endregion Enum Helpers

    #region Value Formatters

    /// <summary>
    /// Formata um valor de texto simples (string) para gravação no XML.
    /// </summary>
    public static string? FormatValue_Str(object? value, int min, SerializerOptions options) => value?.ToString()?.Trim();

    /// <summary>
    /// Formata um valor numérico textual contendo apenas dígitos (sem pontuação) para gravação no XML.
    /// </summary>
    public static string? FormatValue_StrNumber(object? value, int min, SerializerOptions options) => value?.ToString()?.OnlyNumbers();

    /// <summary>
    /// Formata um texto numérico preenchendo com zeros à esquerda até o tamanho mínimo exigido.
    /// </summary>
    public static string? FormatValue_StrNumberFill(object? value, int min, SerializerOptions options)
    {
        var str = value?.ToString();
        return str == null ? null : (str.Length < min ? str.ZeroFill(min) : str);
    }

    /// <summary>
    /// Formata um número inteiro para gravação no XML, com preenchimento de zeros à esquerda se <paramref name="min"/> for maior que zero.
    /// </summary>
    public static string? FormatValue_Int(object? value, int min, SerializerOptions options)
    {
        if (value == null) return null;
        var str = value.ToString();
        return min > 0 && str != null && str.Length < min ? str.ZeroFill(min) : str;
    }

    /// <summary>
    /// Formata um número inteiro longo (64 bits) para gravação no XML.
    /// </summary>
    public static string? FormatValue_Long(object? value, int min, SerializerOptions options) => FormatValue_Int(value, min, options);

    /// <summary>
    /// Formata uma data no formato padrão AAAA-MM-DD (yyyy-MM-dd).
    /// </summary>
    public static string? FormatValue_Dat(object? value, int min, SerializerOptions options) =>
        value is DateTime dt ? dt.ToString("yyyy-MM-dd") : null;

    /// <summary>
    /// Formata uma data no formato do CF-e-SAT (yyyyMMdd).
    /// </summary>
    public static string? FormatValue_DatCFe(object? value, int min, SerializerOptions options) =>
        value is DateTime dt ? dt.ToString("yyyyMMdd") : null;

    /// <summary>
    /// Formata um horário no formato padrão HH:mm:ss.
    /// </summary>
    public static string? FormatValue_Hor(object? value, int min, SerializerOptions options) =>
        value is DateTime dt ? dt.ToString("HH:mm:ss") : null;

    /// <summary>
    /// Formata um horário no formato do CF-e-SAT (HHmmss).
    /// </summary>
    public static string? FormatValue_HorCFe(object? value, int min, SerializerOptions options) =>
        value is DateTime dt ? dt.ToString("HHmmss") : null;

    /// <summary>
    /// Formata data e hora no formato ISO 8601 ordenável (yyyy-MM-ddTHH:mm:ss).
    /// </summary>
    public static string? FormatValue_DatHor(object? value, int min, SerializerOptions options) =>
        value is DateTime dt ? dt.ToString("s") : null;

    /// <summary>
    /// Formata data e hora com fuso horário / timezone (yyyy-MM-ddTHH:mm:sszzz).
    /// </summary>
    public static string? FormatValue_DatHorTz(object? value, int min, SerializerOptions options)
    {
        return value switch
        {
            DateTimeOffset dto => dto.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszzz"),
            DateTime dt => dt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszzz"),
            _ => null
        };
    }

    /// <summary>
    /// Formata um valor decimal com 2 casas decimais (0.00).
    /// </summary>
    public static string? FormatValue_De2(object? value, int min, SerializerOptions options) =>
        value is decimal d ? string.Format(CultureInfo.InvariantCulture, "{0:0.00}", d) : null;

    /// <summary>
    /// Formata um valor decimal com 3 casas decimais (0.000).
    /// </summary>
    public static string? FormatValue_De3(object? value, int min, SerializerOptions options) =>
        value is decimal d ? string.Format(CultureInfo.InvariantCulture, "{0:0.000}", d) : null;

    /// <summary>
    /// Formata um valor decimal com 4 casas decimais (0.0000).
    /// </summary>
    public static string? FormatValue_De4(object? value, int min, SerializerOptions options) =>
        value is decimal d ? string.Format(CultureInfo.InvariantCulture, "{0:0.0000}", d) : null;

    /// <summary>
    /// Formata um valor decimal com 6 casas decimais (0.000000).
    /// </summary>
    public static string? FormatValue_De6(object? value, int min, SerializerOptions options) =>
        value is decimal d ? string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", d) : null;

    /// <summary>
    /// Formata um valor decimal com 10 casas decimais (0.0000000000).
    /// </summary>
    public static string? FormatValue_De10(object? value, int min, SerializerOptions options) =>
        value is decimal d ? string.Format(CultureInfo.InvariantCulture, "{0:0.0000000000}", d) : null;

    /// <summary>
    /// Formata um valor de enumeração para gravação no XML.
    /// </summary>
    public static string? FormatValue_Enum(object? value, int min, SerializerOptions options) => value?.ToString();

    /// <summary>
    /// Formata um tipo customizado para gravação no XML.
    /// </summary>
    public static string? FormatValue_Custom(object? value, int min, SerializerOptions options) => value?.ToString();

    #endregion Value Formatters

    #region Value Parsers

    /// <summary>
    /// Faz o parsing de string removendo delimitadores CDATA se presentes.
    /// </summary>
    public static string ParseValue_Str(string? val)
    {
        if (string.IsNullOrEmpty(val)) return string.Empty;
        if (val!.StartsWith("<![CDATA[") && val.EndsWith("]]>"))
            return val.Substring(9, val.Length - 12);
        return val;
    }

    /// <summary>
    /// Faz o parsing de uma string para inteiro de 32 bits.
    /// </summary>
    public static int ParseValue_Int(string? val) => int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0;

    /// <summary>
    /// Faz o parsing de uma string para inteiro longo de 64 bits.
    /// </summary>
    public static long ParseValue_Long(string? val) => long.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0L;

    /// <summary>
    /// Faz o parsing de uma string para valor decimal com 2 casas.
    /// </summary>
    public static decimal ParseValue_De2(string? val) => decimal.TryParse(val, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    /// <summary>
    /// Faz o parsing de uma string para valor decimal com 3 casas.
    /// </summary>
    public static decimal ParseValue_De3(string? val) => ParseValue_De2(val);

    /// <summary>
    /// Faz o parsing de uma string para valor decimal com 4 casas.
    /// </summary>
    public static decimal ParseValue_De4(string? val) => ParseValue_De2(val);

    /// <summary>
    /// Faz o parsing de uma string para valor decimal com 6 casas.
    /// </summary>
    public static decimal ParseValue_De6(string? val) => ParseValue_De2(val);

    /// <summary>
    /// Faz o parsing de uma string para valor decimal com 10 casas.
    /// </summary>
    public static decimal ParseValue_De10(string? val) => ParseValue_De2(val);

    /// <summary>
    /// Faz o parsing de uma string no formato AAAA-MM-DD para <see cref="DateTime"/>.
    /// </summary>
    public static DateTime ParseValue_Dat(string? val) => DateTime.TryParseExact(val, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ? dt : default;

    /// <summary>
    /// Faz o parsing de uma string no formato AAAAMMDD (CF-e) para <see cref="DateTime"/>.
    /// </summary>
    public static DateTime ParseValue_DatCFe(string? val) => DateTime.TryParseExact(val, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ? dt : default;

    /// <summary>
    /// Faz o parsing de uma string no formato HH:mm:ss para <see cref="DateTime"/>.
    /// </summary>
    public static DateTime ParseValue_Hor(string? val) => DateTime.TryParseExact(val, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ? dt : default;

    /// <summary>
    /// Faz o parsing de uma string no formato HHmmss (CF-e) para <see cref="DateTime"/>.
    /// </summary>
    public static DateTime ParseValue_HorCFe(string? val) => DateTime.TryParseExact(val, "HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ? dt : default;

    /// <summary>
    /// Faz o parsing de uma string contendo data e hora para <see cref="DateTime"/>.
    /// </summary>
    public static DateTime ParseValue_DatHor(string? val) => DateTime.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ? dt : default;

    /// <summary>
    /// Faz o parsing de uma string contendo data, hora e timezone para <see cref="DateTimeOffset"/>.
    /// </summary>
    public static DateTimeOffset ParseValue_DatHorTz(string? val) => DateTimeOffset.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : default;

    /// <summary>
    /// Faz o parsing de uma string numérica mantendo apenas os dígitos.
    /// </summary>
    public static string ParseValue_StrNumber(string? val) => val?.OnlyNumbers() ?? string.Empty;

    /// <summary>
    /// Faz o parsing de uma string numérica mantendo o preenchimento de zeros à esquerda.
    /// </summary>
    public static string ParseValue_StrNumberFill(string? val) => val ?? string.Empty;

    /// <summary>
    /// Faz o parsing de uma string de tipo customizado.
    /// </summary>
    public static string ParseValue_Custom(string? val) => val ?? string.Empty;

    /// <summary>
    /// Faz o parsing de uma string de tipo enumeração.
    /// </summary>
    public static string ParseValue_Enum(string? val) => val ?? string.Empty;

    #endregion Value Parsers
}
