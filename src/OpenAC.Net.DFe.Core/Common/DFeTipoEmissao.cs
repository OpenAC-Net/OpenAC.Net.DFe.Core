// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 10-14-2016
//
// Last Modified By : RFTD
// Last Modified On : 10-14-2016
// ***********************************************************************
// <copyright file="DFeTipoEmissao.cs" company="OpenAC .Net">
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

namespace OpenAC.Net.DFe.Core.Common;

/// <summary>
/// Define os tipos de emissão do documento fiscal eletrônico (normal, contingência, SVC, etc.).
/// </summary>
public enum DFeTipoEmissao
{
    /// <summary>
    /// Emissão normal (1).
    /// </summary>
    [DFeEnum("1")]
    Normal,

    /// <summary>
    /// Emissão em contingência FS-IA (2).
    /// </summary>
    [DFeEnum("2")]
    Contingencia,

    /// <summary>
    /// Emissão SCAN (Sistema de Contingência do Ambiente Nacional) (3).
    /// </summary>
    [DFeEnum("3")]
    SCAN,

    /// <summary>
    /// Emissão DPEC (Declaração Prévia de Emissão em Contingência) (4).
    /// </summary>
    [DFeEnum("4")]
    DPEC,

    /// <summary>
    /// Emissão FS-DA (Contingência com Formulário de Segurança para Documento Auxiliar) (5).
    /// </summary>
    [DFeEnum("5")]
    FSDA,

    /// <summary>
    /// Emissão SVC-AN (SEFAZ Virtual de Contingência Ambiente Nacional) (6).
    /// </summary>
    [DFeEnum("6")]
    SVCAN,

    /// <summary>
    /// Emissão SVC-RS (SEFAZ Virtual de Contingência Rio Grande do Sul) (7).
    /// </summary>
    [DFeEnum("7")]
    SVCRS,

    /// <summary>
    /// Emissão SVC-SP (SEFAZ Virtual de Contingência São Paulo) (8).
    /// </summary>
    [DFeEnum("8")]
    SVCSP,

    /// <summary>
    /// Emissão em contingência Off-line para NFC-e (9).
    /// </summary>
    [DFeEnum("9")]
    OffLine
}