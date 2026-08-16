// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 05-04-2018
//
// Last Modified By : RFTD
// Last Modified On : 05-11-2018
// ***********************************************************************
// <copyright file="DFeDictionaryAttribute.cs" company="OpenAC .Net">
//		        		   The MIT License (MIT)
//	     		    Copyright (c) 2014-2026 Grupo OpenAC.Net
//
//	 Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//	到位 The above copyright notice and this permission notice shall be
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

namespace OpenAC.Net.DFe.Core.Attributes;

/// <summary>
/// Atributo para mapeamento de coleções do tipo dicionário (chave-valor) em XML DFe.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DFeDictionaryAttribute : DFeBaseAttribute
{
    #region Constructors

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeDictionaryAttribute"/>.
    /// </summary>
    public DFeDictionaryAttribute()
    {
        Id = string.Empty;
        Name = string.Empty;
        ItemName = string.Empty;
        Descricao = string.Empty;
        MinSize = 0;
        MaxSize = 0;
        Ocorrencia = Ocorrencia.NaoObrigatoria;
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeDictionaryAttribute"/> com o nome da tag do dicionário.
    /// </summary>
    /// <param name="tag">Nome da tag XML do dicionário.</param>
    public DFeDictionaryAttribute(string tag) : this()
    {
        Name = tag;
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeDictionaryAttribute"/> com os nomes da tag do dicionário e de cada item.
    /// </summary>
    /// <param name="tag">Nome da tag XML do dicionário.</param>
    /// <param name="itemName">Nome da tag XML de cada item do dicionário.</param>
    public DFeDictionaryAttribute(string tag, string itemName) : this()
    {
        Name = tag;
        ItemName = itemName;
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Obtém ou define o nome da tag XML de cada item contido no dicionário.
    /// </summary>
    public string ItemName { get; set; }

    /// <summary>
    /// Obtém ou define a quantidade mínima de entradas no dicionário.
    /// </summary>
    public int MinSize { get; set; }

    /// <summary>
    /// Obtém ou define a quantidade máxima de entradas permitidas no dicionário.
    /// </summary>
    public int MaxSize { get; set; }

    #endregion Properties
}