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
//	     		    Copyright (c) 2014-2022 Grupo OpenAC.Net
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

[AttributeUsage(AttributeTargets.Property)]
public sealed class DFeDictionaryAttribute : DFeBaseAttribute
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DFeDictionaryAttribute" /> class.
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
    /// Initializes a new instance of the <see cref="DFeDictionaryAttribute" /> class.
    /// </summary>
    /// <param name="tag">The Name.</param>
    public DFeDictionaryAttribute(string tag) : this()
    {
        Name = tag;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DFeDictionaryAttribute" /> class.
    /// </summary>
    /// <param name="tag">The Name.</param>
    /// <param name="itemName">The Name.</param>
    public DFeDictionaryAttribute(string tag, string itemName) : this()
    {
        Name = tag;
        ItemName = itemName;
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Gets or sets the Name.
    /// </summary>
    /// <value>The Name.</value>
    public string ItemName { get; set; }

    /// <summary>
    /// Gets or sets the name space.
    /// </summary>
    /// <value>The name space.</value>
    public string Namespace { get; set; }

    /// <summary>
    ///
    /// </summary>
    public int MinSize { get; set; }

    /// <summary>
    ///
    /// </summary>
    public int MaxSize { get; set; }

    #endregion Properties
}