// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 03-10-2018
//
// Last Modified By : RFTD
// Last Modified On : 03-10-2018
// ***********************************************************************
// <copyright file="EnumExtensions.cs" company="OpenAC .Net">
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

using System;
using System.Linq;
using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Common;

namespace OpenAC.Net.DFe.Core.Extensions;

/// <summary>
/// Métodos de extensão para enumerações utilizadas no DFe.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Retorna o valor de serialização XML definido pelo atributo <see cref="DFeEnumAttribute"/> no membro do enum.
    /// </summary>
    /// <typeparam name="T">O tipo da enumeração.</typeparam>
    /// <param name="value">O membro da enumeração.</param>
    /// <returns>O valor configurado em <see cref="DFeEnumAttribute.Value"/>, ou o nome do membro se o atributo não estiver presente.</returns>
    public static string GetDFeValue<T>(this T value) where T : Enum
    {
        var member = typeof(T).GetMember(value.ToString()).FirstOrDefault();
        var enumAttribute = member?.GetCustomAttributes(false).OfType<DFeEnumAttribute>().FirstOrDefault();
        var enumValue = enumAttribute?.Value;
        return enumValue ?? value.ToString();
    }

    /// <summary>
    /// Converte a sigla da Unidade Federativa (<see cref="DFeSiglaUF"/>) para o seu respectivo código IBGE (<see cref="DFeCodUF"/>).
    /// </summary>
    /// <param name="uf">A sigla da UF.</param>
    /// <returns>O código numérico IBGE correspondente.</returns>
    public static DFeCodUF ToCodeUf(this DFeSiglaUF uf)
    {
        return (DFeCodUF)Enum.Parse(typeof(DFeCodUF), uf.ToString());
    }

    /// <summary>
    /// Converte o código IBGE da Unidade Federativa (<see cref="DFeCodUF"/>) para a sua respectiva sigla (<see cref="DFeSiglaUF"/>).
    /// </summary>
    /// <param name="uf">O código IBGE da UF.</param>
    /// <returns>A sigla da UF correspondente.</returns>
    public static DFeSiglaUF ToSiglaUF(this DFeCodUF uf)
    {
        return (DFeSiglaUF)Enum.Parse(typeof(DFeSiglaUF), uf.ToString());
    }
}