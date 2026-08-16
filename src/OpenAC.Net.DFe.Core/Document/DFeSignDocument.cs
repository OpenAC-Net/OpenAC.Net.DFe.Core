// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 05-07-2016
//
// Last Modified By : RFTD
// Last Modified On : 05-07-2016
// ***********************************************************************
// <copyright file="DFeSignDocument.cs" company="OpenAC .Net">
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
using System.Security.Cryptography.X509Certificates;
using OpenAC.Net.Core;
using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Common;

namespace OpenAC.Net.DFe.Core.Document;

/// <summary>
/// Classe base abstrata para documentos fiscais eletrônicos DFe que suportam assinatura digital XML-DSig (<see cref="DFeSignature"/>).
/// </summary>
/// <typeparam name="TDocument">O tipo concreto do documento assinado.</typeparam>
public abstract class DFeSignDocument<TDocument> : DFeDocument<TDocument> where TDocument : class
{
    #region Constructors

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeSignDocument{TDocument}"/>.
    /// </summary>
    protected DFeSignDocument()
    {
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Obtém ou define a assinatura digital XMLDSig (<c>&lt;Signature&gt;</c>) anexada ao documento fiscal.
    /// </summary>
    [DFeElement("Signature", Namespace = "http://www.w3.org/2000/09/xmldsig#", Ocorrencia = Ocorrencia.NaoObrigatoria, Ordem = int.MaxValue)]
    public DFeSignature Signature { get; set; } = new();

    #endregion Properties

    #region Methods

    /// <summary>
    /// Realiza a assinatura digital do documento XML com o certificado informado.
    /// </summary>
    /// <param name="certificado">O certificado digital X509 contendo a chave privada.</param>
    /// <param name="options">Opções de formatação e salvamento do XML.</param>
    /// <param name="comments">Indica se comentários XML devem ser preservados durante a canonicalização.</param>
    /// <param name="digest">O algoritmo de hash criptográfico utilizado (SHA-1 ou SHA-256).</param>
    protected void AssinarDocumento(X509Certificate2 certificado, DFeSaveOptions options, bool comments, SignDigest digest = SignDigest.SHA1)
    {
        Guard.Against<ArgumentNullException>(certificado == null, nameof(certificado));
        Guard.Against<ArgumentException>(!certificado.HasPrivateKey, "O certificado informado não possui chave privada para assinatura.");

        Signature = this.AssinarDocumento(certificado, comments, digest, options, out var xml);
        Xml = xml;
    }

    /// <summary>
    /// Verifica se a assinatura digital possui todos os campos obrigatórios preenchidos para ser serializada no XML.
    /// </summary>
    /// <returns><c>true</c> se a assinatura digital deve ser serializada; caso contrário, <c>false</c>.</returns>
    protected virtual bool ShouldSerializeSignature()
    {
        return !Signature.SignatureValue.IsEmpty() &&
               !Signature.SignedInfo.Reference.DigestValue.IsEmpty() &&
               !Signature.KeyInfo.X509Data.X509Certificate.IsEmpty();
    }

    #endregion Methods
}