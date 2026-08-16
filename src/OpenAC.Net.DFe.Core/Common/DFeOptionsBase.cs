// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 08-10-2018
//
// Last Modified By : RFTD
// Last Modified On : 08-10-2018
// ***********************************************************************
// <copyright file="DFeOptionsBase.cs" company="OpenAC .Net">
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
using System.Drawing;

namespace OpenAC.Net.DFe.Core.Common;

/// <summary>
/// Classe base genérica para opções de impressão e relatórios DFe com filtro específico.
/// </summary>
/// <typeparam name="TFiltro">Enum com as opções de filtro para exportação de relatório.</typeparam>
public abstract class DFeOptionsBase<TFiltro> : DFeOptionsBase where TFiltro : Enum
{
    #region Properties

    /// <summary>
    /// Obtém ou define o filtro de relatório utilizado na geração ou impressão.
    /// </summary>
    public TFiltro Filtro { get; set; }

    #endregion Properties
}

/// <summary>
/// Classe base abstrata para opções de impressão, visualização e geração de relatórios de documentos fiscais eletrônicos.
/// </summary>
public abstract class DFeOptionsBase
{
    #region Properties

    /// <summary>
    /// Obtém ou define a imagem do logotipo da empresa a ser impressa no documento fiscal.
    /// </summary>
    public Image Logo { get; set; }

    /// <summary>
    /// Obtém ou define se deve exibir a tela de pré-visualização da impressão antes de enviar à impressora.
    /// </summary>
    public bool MostrarPreview { get; set; }

    /// <summary>
    /// Obtém ou define se deve exibir o diálogo de configuração de impressão do Windows.
    /// </summary>
    public bool MostrarSetup { get; set; }

    /// <summary>
    /// Obtém ou define se deve utilizar o caminho padrão de PDF para salvamento de arquivos exportados.
    /// </summary>
    public bool UsarPathPDF { get; set; }

    /// <summary>
    /// Obtém ou define o nome da impressora de destino para envio direto da impressão.
    /// </summary>
    public string Impressora { get; set; }

    /// <summary>
    /// Obtém ou define o número de cópias a serem impressas.
    /// </summary>
    public int NumeroCopias { get; set; }

    /// <summary>
    /// Obtém ou define o nome do arquivo gerado em exportações para PDF/HTML.
    /// </summary>
    public string NomeArquivo { get; set; }

    /// <summary>
    /// Obtém ou define o nome da Software House / desenvolvedor do sistema exibido no rodapé do documento.
    /// </summary>
    public string SoftwareHouse { get; set; }

    #endregion Properties
}