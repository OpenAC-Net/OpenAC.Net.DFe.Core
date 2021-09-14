// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 04-01-2019
//
// Last Modified By : RFTD
// Last Modified On : 04-01-2019
// ***********************************************************************
// <copyright file="DFeServices.cs" company="OpenAC .Net">
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
using System.Linq;
using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Collection;
using OpenAC.Net.DFe.Core.Common;
using OpenAC.Net.DFe.Core.Document;

namespace OpenAC.Net.DFe.Core.Service
{
    [DFeRoot("DFeServices", Namespace = "https://openac-net.github.io")]
    public class DFeServices<TTIpo, TVersao> : DFeDocument<DFeServices<TTIpo, TVersao>>
        where TTIpo : Enum
        where TVersao : Enum
    {
        #region Constructors

        public DFeServices()
        {
            Webservices = new DFeCollection<DFeServiceInfo<TTIpo, TVersao>>();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///
        /// </summary>
        /// <param name="versao"></param>
        /// <param name="emissao"></param>
        [DFeIgnore]
        public DFeServiceInfo<TTIpo, TVersao> this[TVersao versao, DFeTipoEmissao emissao]
        {
            get { return Webservices?.SingleOrDefault(x => x.Versao.Equals(versao) && x.TipoEmissao == emissao); }
        }

        [DFeCollection("Services")]
        public DFeCollection<DFeServiceInfo<TTIpo, TVersao>> Webservices { get; set; }

        #endregion Properties
    }
}