// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 23-04-2022
//
// Last Modified By : RFTD
// Last Modified On : 23-04-2022
// ***********************************************************************
// <copyright file="DFeRestServiceClient.cs" company="OpenAC .Net">
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

using System.Collections.Specialized;
using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core.Common;

namespace OpenAC.Net.DFe.Core.Service;

public abstract class DFeRestServiceClient<TDFeConfig, TGeralConfig, TWebserviceConfig, TCertificadosConfig,
    TArquivosConfig> : DFeServiceClientBase<TDFeConfig, TGeralConfig, TWebserviceConfig, TCertificadosConfig, TArquivosConfig>
    where TDFeConfig : DFeConfigBase<TGeralConfig, TWebserviceConfig, TCertificadosConfig, TArquivosConfig>
    where TGeralConfig : DFeGeralConfigBase
    where TWebserviceConfig : DFeWebserviceConfigBase
    where TCertificadosConfig : DFeCertificadosConfigBase
    where TArquivosConfig : DFeArquivosConfigBase
{
    #region Constructors

    /// <summary>
    /// Inicializa uma nova instancia da classe <see cref="DFeRestServiceClient{T, T, T, T, T}"/>.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="url"></param>
    protected DFeRestServiceClient(TDFeConfig config, string url) : base(config, url)
    {
    }

    #endregion Constructors

    #region Properties

    public string AuthenticationHeader { get; protected set; } = "AUTHORIZATION";

    #endregion Properties

    #region Methods

    protected string Get(string action, string contentyType)
    {
        var url = Url;

        try
        {
            SetAction(action);
            EnvelopeEnvio = string.Empty;

            var auth = Authentication();
            var headers = !auth.IsEmpty() ? new NameValueCollection { { AuthenticationHeader, auth } } : null;

            Execute(contentyType, "GET", headers);
            return EnvelopeRetorno;
        }
        finally
        {
            Url = url;
        }
    }

    protected string Post(string action, string message, string contentyType)
    {
        var url = Url;

        try
        {
            SetAction(action);

            var auth = Authentication();
            var headers = !auth.IsEmpty() ? new NameValueCollection { { AuthenticationHeader, auth } } : null;

            EnvelopeEnvio = message;

            Execute(contentyType, "POST", headers);
            return EnvelopeRetorno;
        }
        finally
        {
            Url = url;
        }
    }

    protected string Put(string action, string message, string contentyType)
    {
        var url = Url;

        try
        {
            SetAction(action);

            var auth = Authentication();
            var headers = !auth.IsEmpty() ? new NameValueCollection { { AuthenticationHeader, auth } } : null;

            EnvelopeEnvio = message;

            Execute(contentyType, "PUT", headers);
            return EnvelopeRetorno;
        }
        finally
        {
            Url = url;
        }
    }

    protected string Patch(string action, string message, string contentyType)
    {
        var url = Url;

        try
        {
            SetAction(action);

            var auth = Authentication();
            var headers = !auth.IsEmpty() ? new NameValueCollection { { AuthenticationHeader, auth } } : null;

            EnvelopeEnvio = message;

            Execute(contentyType, "PATCH", headers);
            return EnvelopeRetorno;
        }
        finally
        {
            Url = url;
        }
    }

    protected string Delete(string action, string message, string contentyType)
    {
        var url = Url;

        try
        {
            SetAction(action);

            var auth = Authentication();
            var headers = !auth.IsEmpty() ? new NameValueCollection { { AuthenticationHeader, auth } } : null;

            EnvelopeEnvio = message;

            Execute(contentyType, "DELETE", headers);
            return EnvelopeRetorno;
        }
        finally
        {
            Url = url;
        }
    }

    protected virtual string Authentication() => "";

    protected void SetAction(string action) => Url = !Url.EndsWith("/") ? $"{Url}/{action}" : $"{Url}{action}";

    #endregion Methods
}