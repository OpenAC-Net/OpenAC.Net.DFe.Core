// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 01-31-2016
//
// Last Modified By : RFTD
// Last Modified On : 06-07-2016
// ***********************************************************************
// <copyright file="DFeGeralConfigBase.cs" company="OpenAC .Net">
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
using System.ComponentModel;

namespace OpenAC.Net.DFe.Core.Common;

/// <summary>
/// Classe base genérica para configurações gerais com tipo de versão de documento fiscal.
/// </summary>
/// <typeparam name="TVersaoDFe">Enum com as versões de layout suportadas pelo documento fiscal.</typeparam>
public abstract class DFeGeralConfigBase<TVersaoDFe> : DFeGeralConfigBase
    where TVersaoDFe : Enum
{
    #region Properties

    /// <summary>
    /// Obtém ou define a versão de layout do documento DFe.
    /// </summary>
    [Browsable(true)]
    public TVersaoDFe VersaoDFe { get; set; }

    /// <summary>
    /// Obtém ou define a forma de emissão padrão do documento fiscal eletrônico (Normal, Contingência, etc.).
    /// </summary>
    [Browsable(true)]
    [DefaultValue(DFeTipoEmissao.Normal)]
    public DFeTipoEmissao FormaEmissao { get; set; }

    #endregion Properties
}

/// <summary>
/// Classe base abstrata para configurações gerais de serialização, validação e sanitização do DFe.
/// </summary>
public abstract class DFeGeralConfigBase
{
    #region Constructor

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeGeralConfigBase"/>.
    /// </summary>
    protected DFeGeralConfigBase()
    {
        Salvar = true;
        ExibirErroSchema = true;
        FormatoAlerta = "TAG:%TAG% ID:%ID%/%TAG%(%DESCRICAO%) - %MSG%.";
        RetirarAcentos = true;
        RetirarEspacos = true;
        IdentarXml = false;
        ValidarDigest = false;
    }

    #endregion Constructor

    #region Properties

    /// <summary>
    /// Obtém ou define se devem ser salvos os arquivos gerais de envio e retorno sem validade jurídica.
    /// </summary>
    public bool Salvar { get; set; }

    /// <summary>
    /// Obtém ou define se deve exibir mensagens detalhadas de erro de validação do Schema XSD nas exceções.
    /// </summary>
    public bool ExibirErroSchema { get; set; }

    /// <summary>
    /// Obtém ou define o formato do alerta emitido durante a validação ou serialização.
    /// </summary>
    public string FormatoAlerta { get; set; }

    /// <summary>
    /// Obtém ou define se caracteres acentuados devem ser automaticamente convertidos para seus equivalentes sem acento.
    /// </summary>
    public bool RetirarAcentos { get; set; }

    /// <summary>
    /// Obtém ou define se espaços em excesso ou desnecessários devem ser removidos do XML.
    /// </summary>
    public bool RetirarEspacos { get; set; }

    /// <summary>
    /// Obtém ou define se o XML gerado deve ser formatado/indentado.
    /// </summary>
    public bool IdentarXml { get; set; }

    /// <summary>
    /// Obtém ou define se deve validar o DigestValue do documento fiscal no retorno da SEFAZ.
    /// </summary>
    public bool ValidarDigest { get; set; }

    #endregion Properties
}