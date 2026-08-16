// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 06-22-2018
//
// Last Modified By : RFTD
// Last Modified On : 06-22-2018
// ***********************************************************************
// <copyright file="OpenDFeCommunicationException.cs" company="OpenAC .Net">
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
using System.Runtime.Serialization;
using OpenAC.Net.Core;

namespace OpenAC.Net.DFe.Core;

/// <summary>
/// Exceção disparada quando ocorre uma falha na comunicação HTTP/SOAP ou transporte de dados com os Web Services da SEFAZ.
/// </summary>
[Serializable]
public class OpenDFeCommunicationException : OpenException
{
    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="OpenDFeCommunicationException"/> com a mensagem descritiva do erro.
    /// </summary>
    /// <param name="message">A mensagem descritiva do erro.</param>
    public OpenDFeCommunicationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="OpenDFeCommunicationException"/>.
    /// </summary>
    public OpenDFeCommunicationException()
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="OpenDFeCommunicationException"/> com mensagem descritiva e exceção interna.
    /// </summary>
    /// <param name="message">A mensagem descritiva do erro.</param>
    /// <param name="innerException">A exceção interna causadora do erro.</param>
    public OpenDFeCommunicationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="OpenDFeCommunicationException"/> com mensagem formatada.
    /// </summary>
    /// <param name="format">Texto com formato da mensagem.</param>
    /// <param name="args">Argumentos de substituição no formato.</param>
    public OpenDFeCommunicationException(string format, params object[] args)
        : base(string.Format(format, args))
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="OpenDFeCommunicationException"/> com mensagem formatada e exceção interna.
    /// </summary>
    /// <param name="innerException">A exceção interna causadora do erro.</param>
    /// <param name="format">Texto com formato da mensagem.</param>
    /// <param name="args">Argumentos de substituição no formato.</param>
    public OpenDFeCommunicationException(Exception innerException, string format, params object[] args)
        : base(string.Format(format, args), innerException)
    {
    }

#if NET462
    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="OpenDFeCommunicationException"/> com dados serializados.
    /// </summary>
    /// <param name="info">O objeto <see cref="SerializationInfo"/> contendo os dados serializados.</param>
    /// <param name="context">O contexto contextual sobre a origem ou destino.</param>
    protected OpenDFeCommunicationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
#endif
}