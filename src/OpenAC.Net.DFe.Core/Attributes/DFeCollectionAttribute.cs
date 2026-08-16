// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 04-26-2016
//
// Last Modified By : RFTD
// Last Modified On : 04-26-2016
// ***********************************************************************
// <copyright file="DFeCollectionAttribute.cs" company="OpenAC .Net">
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
/// Atributo para mapeamento de listas e coleções de elementos filhos no XML DFe.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DFeCollectionAttribute : DFeBaseAttribute
{
    #region Constructors

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeCollectionAttribute"/>.
    /// </summary>
    public DFeCollectionAttribute()
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeCollectionAttribute"/> com o nome da tag informado.
    /// </summary>
    /// <param name="tag">Nome da tag XML de cada item da coleção.</param>
    public DFeCollectionAttribute(string tag)
    {
        Name = tag;
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeCollectionAttribute"/> com tipo e nome informados.
    /// </summary>
    /// <param name="tipo">Tipo de dado do campo no XML.</param>
    /// <param name="name">Nome da tag XML de cada item da coleção.</param>
    public DFeCollectionAttribute(TipoCampo tipo, string name)
    {
        Tipo = tipo;
        Name = name;
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Obtém ou define a quantidade mínima de itens na coleção.
    /// </summary>
    public int MinSize { get; set; }

    /// <summary>
    /// Obtém ou define a quantidade máxima de itens permitidos na coleção.
    /// </summary>
    public int MaxSize { get; set; }

    #endregion Properties
}