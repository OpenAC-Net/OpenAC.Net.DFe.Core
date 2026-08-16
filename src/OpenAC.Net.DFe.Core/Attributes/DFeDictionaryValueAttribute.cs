// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 05-04-2018
//
// Last Modified By : RFTD
// Last Modified On : 05-11-2018
// ***********************************************************************
// <copyright file="DFeDictionaryValueAttribute.cs" company="OpenAC .Net">
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
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Attributes;

/// <summary>
/// Atributo para mapeamento do valor em um dicionário XML em documentos DFe.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DFeDictionaryValueAttribute : DFeBaseAttribute
{
    #region Constructors

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeDictionaryValueAttribute"/>.
    /// </summary>
    public DFeDictionaryValueAttribute()
    {
        Tipo = TipoCampo.Str;
        Id = "";
        Name = string.Empty;
        Min = 0;
        Max = 0;
        Ocorrencia = 0;
        Descricao = string.Empty;
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeDictionaryValueAttribute"/> com o nome da tag.
    /// </summary>
    /// <param name="tag">Nome da tag do valor.</param>
    public DFeDictionaryValueAttribute(string tag) : this()
    {
        Name = tag;
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeDictionaryValueAttribute"/> com tipo e nome informados.
    /// </summary>
    /// <param name="tipo">Tipo de dado do campo no XML.</param>
    /// <param name="name">Nome da tag do valor.</param>
    public DFeDictionaryValueAttribute(TipoCampo tipo, string name) : this()
    {
        Tipo = tipo;
        Name = name;
    }

    #endregion Constructors
}