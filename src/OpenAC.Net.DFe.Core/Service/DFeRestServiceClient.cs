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

using System.Collections.Specialized;
using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core.Common;

namespace OpenAC.Net.DFe.Core.Service;

/// <summary>
/// Cliente base para comunicação com APIs RESTful de serviços DFe (suporta verbos GET, POST, PUT, PATCH e DELETE).
/// </summary>
/// <typeparam name="TDFeConfig">Tipo das configurações DFe do componente.</typeparam>
/// <typeparam name="TGeralConfig">Tipo das configurações gerais.</typeparam>
/// <typeparam name="TWebserviceConfig">Tipo das configurações de Web Services.</typeparam>
/// <typeparam name="TCertificadosConfig">Tipo das configurações de certificados digitais.</typeparam>
/// <typeparam name="TArquivosConfig">Tipo das configurações de arquivos.</typeparam>
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
    /// Inicializa uma nova instância da classe <see cref="DFeRestServiceClient{TDFeConfig, TGeralConfig, TWebserviceConfig, TCertificadosConfig, TArquivosConfig}"/>.
    /// </summary>
    /// <param name="config">Configurações do componente.</param>
    /// <param name="url">URL base da API REST.</param>
    protected DFeRestServiceClient(TDFeConfig config, string url) : base(config, url)
    {
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Obtém ou define o nome do header HTTP utilizado para autenticação (padrão "AUTHORIZATION").
    /// </summary>
    public string AuthenticationHeader { get; protected set; } = "AUTHORIZATION";

    #endregion Properties

    #region Methods

    /// <summary>
    /// Executa uma requisição HTTP GET na rota/ação especificada.
    /// </summary>
    /// <param name="action">Ação/endpoint relativo.</param>
    /// <param name="contentyType">Content-Type do cabeçalho HTTP.</param>
    /// <returns>A string do corpo de resposta retornado.</returns>
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

    /// <summary>
    /// Executa uma requisição HTTP POST na rota/ação especificada com o payload informado.
    /// </summary>
    /// <param name="action">Ação/endpoint relativo.</param>
    /// <param name="message">Corpo da mensagem (payload).</param>
    /// <param name="contentyType">Content-Type do cabeçalho HTTP.</param>
    /// <returns>A string do corpo de resposta retornado.</returns>
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

    /// <summary>
    /// Executa uma requisição HTTP PUT na rota/ação especificada com o payload informado.
    /// </summary>
    /// <param name="action">Ação/endpoint relativo.</param>
    /// <param name="message">Corpo da mensagem (payload).</param>
    /// <param name="contentyType">Content-Type do cabeçalho HTTP.</param>
    /// <returns>A string do corpo de resposta retornado.</returns>
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

    /// <summary>
    /// Executa uma requisição HTTP PATCH na rota/ação especificada com o payload informado.
    /// </summary>
    /// <param name="action">Ação/endpoint relativo.</param>
    /// <param name="message">Corpo da mensagem (payload).</param>
    /// <param name="contentyType">Content-Type do cabeçalho HTTP.</param>
    /// <returns>A string do corpo de resposta retornado.</returns>
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

    /// <summary>
    /// Executa uma requisição HTTP DELETE na rota/ação especificada.
    /// </summary>
    /// <param name="action">Ação/endpoint relativo.</param>
    /// <param name="message">Corpo da mensagem (opcional).</param>
    /// <param name="contentyType">Content-Type do cabeçalho HTTP.</param>
    /// <returns>A string do corpo de resposta retornado.</returns>
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

    /// <summary>
    /// Retorna a string do token ou credencial de autenticação a ser enviada no header HTTP.
    /// </summary>
    /// <returns>Valor do cabeçalho de autorização.</returns>
    protected virtual string Authentication() => "";

    /// <summary>
    /// Concatena a rota/ação à URL base da API.
    /// </summary>
    /// <param name="action">Ação/endpoint relativo.</param>
    protected void SetAction(string action) => Url = !Url.EndsWith("/") ? $"{Url}/{action}" : $"{Url}{action}";

    #endregion Methods
}