// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 08-10-2018
//
// Last Modified By : RFTD
// Last Modified On : 08-10-2018
// ***********************************************************************
// <copyright file="FiltroDFeReport.cs" company="OpenAC .Net">
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

namespace OpenAC.Net.DFe.Core.Common;

/// <summary>
/// Define o formato de exportação/filtro para geração de relatórios e DANFE/DACTE.
/// </summary>
public enum FiltroDFeReport
{
    /// <summary>
    /// Sem filtro de exportação (impressão direta).
    /// </summary>
    Nenhum,

    /// <summary>
    /// Exportação para arquivo no formato Adobe PDF.
    /// </summary>
    PDF,

    /// <summary>
    /// Exportação para arquivo no formato HTML.
    /// </summary>
    HTML
}