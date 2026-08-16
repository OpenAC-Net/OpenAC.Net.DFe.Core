// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 05-07-2016
//
// Last Modified By : RFTD
// Last Modified On : 05-07-2016
// ***********************************************************************
// <copyright file="Reference.cs" company="OpenAC .Net">
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
using OpenAC.Net.DFe.Core.Collection;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Document;

/// <summary>
/// Representa a referência ao elemento assinado (Reference) contendo as transformações e o hash do documento no padrão XMLDSig.
/// </summary>
public sealed partial class Reference
{
    #region Constructors

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="Reference"/>.
    /// </summary>
    public Reference()
    {
        Transforms = new DFeCollection<Transform>();
        DigestMethod = new DigestMethod();
    }

    #endregion Constructors

    #region Propriedades

    /// <summary>
    /// XS08 - Atributo URI da tag Reference apontando para o identificador do elemento assinado (ex: #NFe35...).
    /// </summary>
    [DFeAttribute(TipoCampo.Str, "URI", Id = "XS08", Min = 0, Max = 999, Ocorrencia = Ocorrencia.Obrigatoria)]
    public string URI { get; set; } = string.Empty;

    /// <summary>
    /// XS10 - Grupo de transformações (Transforms/Transform) aplicadas no documento.
    /// </summary>
    [DFeCollection("Transforms", Id = "XS10")]
    [DFeItem(typeof(Transform), "Transform")]
    public DFeCollection<Transform> Transforms { get; set; }

    /// <summary>
    /// XS15 - Grupo do método de cálculo do resumo criptográfico (DigestMethod).
    /// </summary>
    [DFeElement("DigestMethod", Id = "XS15")]
    public DigestMethod DigestMethod { get; set; }

    /// <summary>
    /// XS17 - Resumo criptográfico calculado do documento (DigestValue) em Base64.
    /// </summary>
    [DFeElement(TipoCampo.Str, "DigestValue", Id = "XS17", Min = 0, Max = 999, Ocorrencia = Ocorrencia.Obrigatoria)]
    public string DigestValue { get; set; } = string.Empty;

    #endregion Propriedades
}