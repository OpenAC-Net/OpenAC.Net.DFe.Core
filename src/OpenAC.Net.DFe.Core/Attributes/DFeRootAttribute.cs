// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 04-24-2016
//
// Last Modified By : RFTD
// Last Modified On : 04-28-2016
// ***********************************************************************
// <copyright file="DFeRootAttribute.cs" company="OpenAC .Net">
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

namespace OpenAC.Net.DFe.Core.Attributes;

/// <summary>
/// Atributo que define a classe como elemento raiz (root) de um documento DFe serializado em XML.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DFeRootAttribute : Attribute
{
    #region Constructors

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeRootAttribute"/>.
    /// </summary>
    public DFeRootAttribute()
    {
        Name = string.Empty;
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeRootAttribute"/> com o nome da tag raiz.
    /// </summary>
    /// <param name="root">Nome da tag raiz no XML.</param>
    public DFeRootAttribute(string root)
    {
        Name = root;
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Obtém ou define o nome da tag raiz do documento no XML.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Obtém ou define o namespace XML padrão do elemento raiz.
    /// </summary>
    public string Namespace { get; set; }

    #endregion Properties
}