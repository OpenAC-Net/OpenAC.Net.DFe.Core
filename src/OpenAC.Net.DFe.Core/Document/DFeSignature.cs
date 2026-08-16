// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 05-07-2016
//
// Last Modified By : RFTD
// Last Modified On : 05-07-2016
// ***********************************************************************
// <copyright file="DFeSignature.cs" company="OpenAC .Net">
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

using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Document;

/// <summary>
/// Representa a estrutura de Assinatura Digital padrão XML-DSig (<c>&lt;Signature&gt;</c>) anexada aos documentos fiscais eletrônicos.
/// </summary>
[DFeRoot("Signature", Namespace = "http://www.w3.org/2000/09/xmldsig#")]
public sealed partial class DFeSignature : DFeDocument<DFeSignature>
{
    #region Constructors

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeSignature"/>.
    /// </summary>
    public DFeSignature()
    {
        SignedInfo = new SignedInfo();
        KeyInfo = new KeyInfo();
    }

    #endregion Constructors

    #region Propriedades

    /// <summary>
    /// XS02 - Grupo de informações da assinatura (SignedInfo).
    /// </summary>
    [DFeElement("SignedInfo", Id = "XS02")]
    public SignedInfo SignedInfo { get; set; }

    /// <summary>
    /// XS18 - Valor criptográfico da assinatura digital gerada em Base64 (SignatureValue).
    /// </summary>
    [DFeElement(TipoCampo.Str, "SignatureValue", Id = "XS18", Min = 0, Max = 999, Ocorrencia = Ocorrencia.Obrigatoria)]
    public string SignatureValue { get; set; } = string.Empty;

    /// <summary>
    /// XS19 - Grupo de informações do certificado/chave pública do signatário (KeyInfo).
    /// </summary>
    [DFeElement("KeyInfo", Id = "XS19")]
    public KeyInfo KeyInfo { get; set; }

    #endregion Propriedades
}