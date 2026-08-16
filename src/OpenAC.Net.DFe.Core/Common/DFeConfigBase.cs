// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 03-11-2018
//
// Last Modified By : RFTD
// Last Modified On : 03-11-2018
// ***********************************************************************
// <copyright file="DFeConfigBase.cs" company="OpenAC .Net">
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
/// Classe base abstrata para agregação de configurações de um componente DFe (Geral, WebServices, Certificados e Arquivos).
/// </summary>
/// <typeparam name="TGeralConfig">Tipo das configurações gerais.</typeparam>
/// <typeparam name="TWebserviceConfig">Tipo das configurações de Web Services.</typeparam>
/// <typeparam name="TCertificadosConfig">Tipo das configurações de certificados digitais.</typeparam>
/// <typeparam name="TArquivosConfig">Tipo das configurações de arquivos e diretórios.</typeparam>
public abstract class DFeConfigBase<TGeralConfig, TWebserviceConfig, TCertificadosConfig, TArquivosConfig>
    where TGeralConfig : DFeGeralConfigBase
    where TWebserviceConfig : DFeWebserviceConfigBase
    where TCertificadosConfig : DFeCertificadosConfigBase
    where TArquivosConfig : DFeArquivosConfigBase
{
    #region Properties

    /// <summary>
    /// Obtém as configurações gerais do componente DFe.
    /// </summary>
    public TGeralConfig Geral { get; protected set; }

    /// <summary>
    /// Obtém as configurações de comunicação com Web Services.
    /// </summary>
    public TWebserviceConfig WebServices { get; protected set; }

    /// <summary>
    /// Obtém as configurações do certificado digital.
    /// </summary>
    public TCertificadosConfig Certificados { get; protected set; }

    /// <summary>
    /// Obtém as configurações de diretórios e salvamento de arquivos XML.
    /// </summary>
    public TArquivosConfig Arquivos { get; protected set; }

    #endregion Properties
}