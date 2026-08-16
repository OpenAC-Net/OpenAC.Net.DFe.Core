// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 03-27-2016
//
// Last Modified By : RFTD
// Last Modified On : 10-15-2016
// ***********************************************************************
// <copyright file="DFeElementAttribute.cs" company="OpenAC .Net">
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

using OpenAC.Net.DFe.Core.Serializer;
using System;

namespace OpenAC.Net.DFe.Core.Attributes;

/// <summary>
/// Atributo para mapeamento de propriedades como elementos/tags XML em documentos DFe.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DFeElementAttribute : DFeBaseAttribute
{
    #region Constructors

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeElementAttribute"/>.
    /// </summary>
    public DFeElementAttribute()
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeElementAttribute"/> com o nome da tag.
    /// </summary>
    /// <param name="tag">Nome da tag XML gerada.</param>
    public DFeElementAttribute(string tag)
    {
        Name = tag;
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeElementAttribute"/> com o tipo de campo e nome da tag informados.
    /// </summary>
    /// <param name="tipo">Tipo de dado do campo no XML.</param>
    /// <param name="name">Nome da tag XML gerada.</param>
    public DFeElementAttribute(TipoCampo tipo, string name)
    {
        Tipo = tipo;
        Name = name;
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Obtém ou define se o conteúdo deste elemento deve ser encapsulado em uma seção CDATA.
    /// </summary>
    public bool UseCData { get; set; }

    #endregion Properties
}