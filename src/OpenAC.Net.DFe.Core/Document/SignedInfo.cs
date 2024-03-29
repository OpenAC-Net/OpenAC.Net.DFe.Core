﻿// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 05-07-2016
//
// Last Modified By : RFTD
// Last Modified On : 05-08-2016
// ***********************************************************************
// <copyright file="SignedInfo.cs" company="OpenAC .Net">
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

using OpenAC.Net.DFe.Core.Attributes;

namespace OpenAC.Net.DFe.Core.Document;

public sealed class SignedInfo
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DFeSignature" /> class.
    /// </summary>
    public SignedInfo()
    {
        CanonicalizationMethod = new CanonicalizationMethod();
        SignatureMethod = new SignatureMethod();
        Reference = new Reference();
    }

    #endregion Constructors

    #region Propriedades

    /// <summary>
    /// XS03 - Grupo do Método de Canonicalização
    /// </summary>
    /// <value>The canonicalization method.</value>
    [DFeElement("CanonicalizationMethod", Id = "XS03")]
    public CanonicalizationMethod CanonicalizationMethod { get; set; }

    /// <summary>
    /// XS05 - Grupo do Método de Assinatura
    /// </summary>
    /// <value>The signature method.</value>
    [DFeElement("SignatureMethod", Id = "XS05")]
    public SignatureMethod SignatureMethod { get; set; }

    /// <summary>
    /// XS07 - Grupo Reference
    /// </summary>
    /// <value>The reference.</value>
    [DFeElement("Reference", Id = "XS07")]
    public Reference Reference { get; set; }

    #endregion Propriedades
}