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
//	     		    Copyright (c) 2016 Grupo OpenAC.Net
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
using OpenAC.Net.Core;

namespace OpenAC.Net.DFe.Core.Common
{
    /// <summary>
    ///
    /// </summary>
    public abstract class DFeGeralConfigBase<TVersaoDFe> : DFeGeralConfigBase
    where TVersaoDFe : Enum
    {
        #region Properties

        /// <summary>
        /// Define/retorna a versão do documento DFe.
        /// </summary>
        [Browsable(true)]
        public TVersaoDFe VersaoDFe { get; set; }

        /// <summary>
        /// Define/retorna o tipo de emissão.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(DFeTipoEmissao.Normal)]
        public DFeTipoEmissao FormaEmissao { get; set; }

        #endregion Properties
    }

    public abstract class DFeGeralConfigBase
    {
        #region Constructor

        /// <summary>
        /// Inicializa uma nova instancia da classe <see cref="DFeGeralConfigBase"/>.
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
        /// Define/retorna se deve ser salvo os arquivos gerais, ou seja, arquivos de envio e
        /// de retorno sem validade jurídica.
        /// </summary>
        /// <value><c>true</c> para salvar; caso contrário, <c>false</c>.</value>
        public bool Salvar { get; set; }

        /// <summary>
        /// Define/retorna se deve exibir os erros de validação do Schema na Execption.
        /// </summary>
        public bool ExibirErroSchema { get; set; }

        /// <summary>
        /// Define/retorna o formato do alerta do serializer.
        /// Valor Padrão = TAG:%TAG% ID:%ID%/%TAG%(%DESCRICAO%) - %MSG%.
        /// </summary>
        public string FormatoAlerta { get; set; }

        /// <summary>
        /// Define/retorna se deve retirar acentos do xml antes de enviar.
        /// </summary>
        public bool RetirarAcentos { get; set; }

        /// <summary>
        /// Define/retorna se deve ser retirado os espaços na hora de gerar o xml.
        /// </summary>
        public bool RetirarEspacos { get; set; }

        /// <summary>
        /// Define/retorna se deve identar o xml na hora de gerar.
        /// </summary>
        public bool IdentarXml { get; set; }

        /// <summary>
        /// Define/retorna se deve ser validado o digest.
        /// </summary>
        public bool ValidarDigest { get; set; }

        #endregion Properties
    }
}