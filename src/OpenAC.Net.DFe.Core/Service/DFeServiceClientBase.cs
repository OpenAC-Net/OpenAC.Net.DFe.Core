// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 07-28-2016
//
// Last Modified By : RFTD
// Last Modified On : 07-28-2016
// ***********************************************************************
// <copyright file="DFeServiceClientBase.cs" company="OpenAC .Net">
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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using OpenAC.Net.Core;
using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core.Common;

namespace OpenAC.Net.DFe.Core.Service;

/// <summary>
/// Classe base abstrata para clientes de comunicação HTTP / Web Services dos documentos fiscais eletrônicos.
/// </summary>
/// <typeparam name="TDFeConfig">Tipo das configurações DFe do componente.</typeparam>
/// <typeparam name="TGeralConfig">Tipo das configurações gerais.</typeparam>
/// <typeparam name="TWebserviceConfig">Tipo das configurações de Web Services.</typeparam>
/// <typeparam name="TCertificadosConfig">Tipo das configurações de certificados digitais.</typeparam>
/// <typeparam name="TArquivosConfig">Tipo das configurações de arquivos.</typeparam>
public abstract class DFeServiceClientBase<TDFeConfig, TGeralConfig, TWebserviceConfig, TCertificadosConfig, TArquivosConfig> : IDisposable
    where TDFeConfig : DFeConfigBase<TGeralConfig, TWebserviceConfig, TCertificadosConfig, TArquivosConfig>
    where TGeralConfig : DFeGeralConfigBase
    where TWebserviceConfig : DFeWebserviceConfigBase
    where TCertificadosConfig : DFeCertificadosConfigBase
    where TArquivosConfig : DFeArquivosConfigBase
{
    #region Constructors

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeServiceClientBase{TDFeConfig, TGeralConfig, TWebserviceConfig, TCertificadosConfig, TArquivosConfig}"/>.
    /// </summary>
    /// <param name="config">Instância de configuração do componente DFe.</param>
    /// <param name="url">URL do endpoint de destino do serviço.</param>
    protected DFeServiceClientBase(TDFeConfig config, string url)
    {
        Configuracoes = config;
        Url = url;
        NomeArquivo = string.Empty;
        ArquivoEnvio = string.Empty;
        ArquivoResposta = string.Empty;
        EnvelopeEnvio = string.Empty;
        EnvelopeRetorno = string.Empty;
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Obtém as configurações do componente DFe.
    /// </summary>
    protected TDFeConfig Configuracoes { get; }

    /// <summary>
    /// Obtém ou define o prefixo/nome base para os arquivos de envio e retorno gravados em disco.
    /// </summary>
    public string NomeArquivo { get; protected set; }

    /// <summary>
    /// Obtém o nome do arquivo gerado para a mensagem de envio.
    /// </summary>
    public string ArquivoEnvio { get; protected set; }

    /// <summary>
    /// Obtém o nome do arquivo gerado para a mensagem de resposta.
    /// </summary>
    public string ArquivoResposta { get; protected set; }

    /// <summary>
    /// Obtém ou define o envelope de envio da requisição.
    /// </summary>
    public string EnvelopeEnvio { get; protected set; }

    /// <summary>
    /// Obtém ou define o envelope retornado pelo servidor.
    /// </summary>
    public string EnvelopeRetorno { get; protected set; }

    /// <summary>
    /// Obtém ou define a URL do serviço.
    /// </summary>
    protected string Url { get; set; }

    /// <summary>
    /// Obtém o certificado digital configurado para a requisição mTLS.
    /// </summary>
    protected X509Certificate2 Certificado => Configuracoes.Certificados.ObterCertificado();

    /// <summary>
    /// Indica se o cliente já teve seus recursos liberados (disposed).
    /// </summary>
    protected bool IsDisposed { get; private set; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Executa a requisição HTTP síncrona enviando o <see cref="EnvelopeEnvio"/> e preenchendo o <see cref="EnvelopeRetorno"/>.
    /// </summary>
    /// <param name="contentType">Content-Type do cabeçalho HTTP.</param>
    /// <param name="method">Método HTTP utilizado (ex: POST, GET).</param>
    /// <param name="headers">Cabeçalhos adicionais da requisição (opcional).</param>
    /// <exception cref="OpenDFeCommunicationException">Disparada em caso de falha de conexão ou erro HTTP.</exception>
    protected void Execute(string contentType, string method, NameValueCollection? headers = null)
    {
        var protocolos = ServicePointManager.SecurityProtocol;
        ServicePointManager.SecurityProtocol = Configuracoes.WebServices.Protocolos;

        try
        {
#pragma warning disable SYSLIB0014
            var request = WebRequest.CreateHttp(Url);
#pragma warning restore SYSLIB0014
            request.Method = method.IsEmpty() ? "POST" : method;
            request.ContentType = contentType;

            if (!ValidarCertificadoServidor())
                request.ServerCertificateValidationCallback += (_, _, _, _) => true;

            if (Configuracoes.WebServices.TimeOut.HasValue)
                request.Timeout = Configuracoes.WebServices.TimeOut.Value.Milliseconds;

            if (headers?.Count > 0)
                request.Headers.Add(headers);

            if (Certificado != null)
                request.ClientCertificates.Add(Certificado);

            if (!EnvelopeEnvio.IsEmpty())
            {
                ArquivoEnvio = $"{DateTime.Now:yyyyMMddssfff}_{NomeArquivo}_envio.xml";
                GravarSoap(EnvelopeEnvio, ArquivoEnvio);

                using var streamWriter = new StreamWriter(request.GetRequestStream());
                streamWriter.Write(EnvelopeEnvio);
                streamWriter.Flush();
            }

            var response = request.GetResponse();
            EnvelopeRetorno = GetResponse(response);

            ArquivoResposta = $"{DateTime.Now:yyyyMMddssfff}_{NomeArquivo}_retorno.xml";
            GravarSoap(EnvelopeRetorno, ArquivoResposta);
        }
        catch (Exception ex) when (ex is not OpenDFeCommunicationException)
        {
            throw new OpenDFeCommunicationException(ex.Message, ex);
        }
        finally
        {
            ServicePointManager.SecurityProtocol = protocolos;
        }
    }

    /// <summary>
    /// Lê a string de resposta completa a partir do fluxo de retorno <see cref="WebResponse"/>.
    /// </summary>
    /// <param name="response">A resposta HTTP retornada pelo servidor.</param>
    /// <returns>O corpo da resposta como texto.</returns>
    /// <exception cref="OpenDFeCommunicationException">Disparada se o stream de resposta for nulo.</exception>
    protected static string GetResponse(WebResponse response)
    {
        var stream = response.GetResponseStream();
        Guard.Against<OpenDFeCommunicationException>(stream == null, "Erro ao ler retorno do servidor.");

        using (stream)
        {
            using var reader = new StreamReader(stream!);
            var retorno = reader.ReadToEnd();
            response.Close();
            return retorno;
        }
    }

    /// <summary>
    /// Valida se o certificado SSL/TLS do servidor deve ser estritamente validado pela cadeia de confiança (padrão true).
    /// </summary>
    /// <returns><c>true</c> para validar; <c>false</c> para ignorar erros de validação SSL.</returns>
    protected virtual bool ValidarCertificadoServidor() => true;

    /// <summary>
    /// Grava a mensagem enviada ou recebida em disco conforme as configurações de salvamento.
    /// </summary>
    /// <param name="conteudoArquivo">Conteúdo do envelope/arquivo a ser salvo.</param>
    /// <param name="nomeArquivo">Nome do arquivo.</param>
    protected abstract void GravarSoap(string conteudoArquivo, string nomeArquivo);

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Libera recursos gerenciados da instância.
    /// </summary>
    protected virtual void DisposeManaged()
    {
    }

    /// <summary>
    /// Libera recursos não gerenciados da instância.
    /// </summary>
    protected virtual void DisposeUnmanaged()
    {
    }

    private void Dispose(bool disposing)
    {
        if (IsDisposed)
            return;

        if (disposing)
            DisposeManaged();

        DisposeUnmanaged();

        IsDisposed = true;
    }

    #endregion Methods
}