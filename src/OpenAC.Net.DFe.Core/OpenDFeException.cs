// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 03-21-2014
//
// Last Modified By : RFTD
// Last Modified On : 06-22-2018
// ***********************************************************************
// <copyright file="OpenDFeException.cs" company="OpenAC .Net">
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

using System;
using System.Runtime.Serialization;
using OpenAC.Net.Core;

namespace OpenAC.Net.DFe.Core;

/// <summary>
/// Classe OpenDFeException.
/// </summary>
// ReSharper disable once InconsistentNaming
public class OpenDFeException : OpenException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpenDFeException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public OpenDFeException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenDFeException"/> class.
    /// </summary>
    public OpenDFeException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenDFeException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public OpenDFeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenDFeException"/> class.
    /// </summary>
    /// <param name="innerException">The inner exception.</param>
    /// <param name="message">The message.</param>
    /// <param name="args">The arguments.</param>
    public OpenDFeException(Exception innerException, string message, params object[] args)
        : base(string.Format(message, args), innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenDFeException" /> class with serialized data.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
    protected OpenDFeException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}