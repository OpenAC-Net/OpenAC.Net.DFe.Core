// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 06-30-2018
//
// Last Modified By : RFTD
// Last Modified On : 06-30-2018
// ***********************************************************************
// <copyright file="DFeResposta.cs" company="OpenAC .Net">
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

using System.Text;
using OpenAC.Net.Core.Generics;
using OpenAC.Net.DFe.Core.Document;

namespace OpenAC.Net.DFe.Core.Service;

public abstract class DFeResposta<T> where T : class
{
    #region Constructors

    protected DFeResposta(string xmlEnvio, string xmlRetorno, string envelopeEnvio, string resposta, bool loadRetorno = true)
    {
        XmlEnvio = xmlEnvio;
        XmlRetorno = xmlRetorno;
        EnvelopeEnvio = envelopeEnvio;
        EnvelopeRetorno = resposta;

        if (typeof(DFeDocument<T>).IsAssignableFrom(typeof(T)) && loadRetorno)
        {
            Resultado = DFeDocument<T>.Load(xmlRetorno, Encoding.UTF8);
        }
    }

    #endregion Constructors

    #region Properties

    public string XmlEnvio { get; }

    public string XmlRetorno { get; }

    public string EnvelopeEnvio { get; }

    public string EnvelopeRetorno { get; }

    public T Resultado { get; protected set; }

    #endregion Properties
}