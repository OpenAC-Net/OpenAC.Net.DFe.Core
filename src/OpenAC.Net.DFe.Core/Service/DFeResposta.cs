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

using System.Text;
using OpenAC.Net.DFe.Core.Document;

namespace OpenAC.Net.DFe.Core.Service;

/// <summary>
/// Classe base abstrata para objetos de resposta de Web Services DFe contendo XMLs e Envelopes de envio e retorno.
/// </summary>
/// <typeparam name="T">O tipo do resultado retornado pelo serviço.</typeparam>
public abstract class DFeResposta<T> where T : class
{
    #region Constructors

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeResposta{T}"/>.
    /// </summary>
    /// <param name="xmlEnvio">Conteúdo XML enviado.</param>
    /// <param name="xmlRetorno">Conteúdo XML retornado pela SEFAZ.</param>
    /// <param name="envelopeEnvio">Envelope SOAP/HTTP de envio.</param>
    /// <param name="resposta">Envelope SOAP/HTTP de resposta.</param>
    /// <param name="loadRetorno">Indica se deve deserializar automaticamente o XML de retorno para a propriedade <see cref="Resultado"/>.</param>
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

    /// <summary>
    /// Obtém o XML de envio da requisição.
    /// </summary>
    public string XmlEnvio { get; }

    /// <summary>
    /// Obtém o XML retornado pelo Web Service.
    /// </summary>
    public string XmlRetorno { get; }

    /// <summary>
    /// Obtém a mensagem completa do envelope SOAP/HTTP de envio.
    /// </summary>
    public string EnvelopeEnvio { get; }

    /// <summary>
    /// Obtém a mensagem completa do envelope SOAP/HTTP de retorno.
    /// </summary>
    public string EnvelopeRetorno { get; }

    /// <summary>
    /// Obtém o objeto tipado deserializado a partir do XML de retorno.
    /// </summary>
    public T Resultado { get; protected set; }

    #endregion Properties
}