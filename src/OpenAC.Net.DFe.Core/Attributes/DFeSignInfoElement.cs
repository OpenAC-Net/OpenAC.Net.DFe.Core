// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 07-08-2018
//
// Last Modified By : RFTD
// Last Modified On : 07-08-2018
// ***********************************************************************
// <copyright file="DFeSignInfoElement.cs" company="OpenAC .Net">
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
/// Atributo que define as configurações de assinatura digital (tag alvo e atributo identificador) para uma classe de documento DFe.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DFeSignInfoElement : Attribute
{
    #region Constructors

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeSignInfoElement"/>.
    /// </summary>
    public DFeSignInfoElement()
    {
        SignElement = string.Empty;
        SignAtribute = "Id";
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeSignInfoElement"/> informando o elemento a ser assinado.
    /// </summary>
    /// <param name="signElement">Nome do elemento XML que receberá a assinatura.</param>
    public DFeSignInfoElement(string signElement)
    {
        SignElement = signElement;
        SignAtribute = "Id";
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Obtém ou define o nome da tag do elemento XML a ser assinado.
    /// </summary>
    public string SignElement { get; set; }

    /// <summary>
    /// Obtém ou define o nome do atributo identificador (URI) do elemento a ser assinado (padrão "Id").
    /// </summary>
    public string SignAtribute { get; set; }

    #endregion Properties
}