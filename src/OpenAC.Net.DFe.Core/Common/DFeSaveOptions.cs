// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 09-11-2016
//
// Last Modified By : RFTD
// Last Modified On : 09-11-2016
// ***********************************************************************
// <copyright file="DFeSaveOptions.cs" company="OpenAC .Net">
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

namespace OpenAC.Net.DFe.Core.Common;

/// <summary>
/// Flags com opções de transformação e sanitização aplicadas ao salvar documentos DFe em formato XML.
/// </summary>
[Flags]
public enum DFeSaveOptions
{
    /// <summary>
    /// Nenhuma transformação adicional é aplicada.
    /// </summary>
    None = 1 << 0,

    /// <summary>
    /// Remove acentos dos textos do XML substituindo-os por seus equivalentes sem acentuação.
    /// </summary>
    RemoveAccents = 1 << 1,

    /// <summary>
    /// Remove espaços em branco redundantes e desnecessários do XML.
    /// </summary>
    RemoveSpaces = 1 << 2,

    /// <summary>
    /// Desabilita a indentação/formatação do XML, gerando-o em linha compacta.
    /// </summary>
    DisableFormatting = 1 << 3,

    /// <summary>
    /// Omite a declaração XML inicial (<c>&lt;?xml version="1.0" ... ?&gt;</c>).
    /// </summary>
    OmitDeclaration = 1 << 4
}