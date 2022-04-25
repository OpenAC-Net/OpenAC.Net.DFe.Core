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

using OpenAC.Net.Core.Logging;
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
/// Class DFeServiceClientBase.
/// </summary>
/// <typeparam name="TDFeConfig"></typeparam>
/// <typeparam name="TGeralConfig"></typeparam>
/// <typeparam name="TWebserviceConfig"></typeparam>
/// <typeparam name="TCertificadosConfig"></typeparam>
/// <typeparam name="TArquivosConfig"></typeparam>
/// <seealso cref="IOpenLog" />
public abstract class DFeServiceClientBase<TDFeConfig, TGeralConfig, TWebserviceConfig, TCertificadosConfig, TArquivosConfig> : IDisposable
    where TDFeConfig : DFeConfigBase<TGeralConfig, TWebserviceConfig, TCertificadosConfig, TArquivosConfig>
    where TGeralConfig : DFeGeralConfigBase
    where TWebserviceConfig : DFeWebserviceConfigBase
    where TCertificadosConfig : DFeCertificadosConfigBase
    where TArquivosConfig : DFeArquivosConfigBase
{
    #region Constructors

    /// <summary>
    /// Inicializa uma nova instancia da classe <see cref="DFeServiceClientBase{T, T, T, T, T}"/>.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="url"></param>
    protected DFeServiceClientBase(TDFeConfig config, string url)
    {
        Configuracoes = config;
        Url = url;
    }

    #endregion Constructors

    #region Properties

    protected TDFeConfig Configuracoes { get; }

    public string NomeArquivo { get; protected set; }

    public string ArquivoEnvio { get; protected set; }

    public string ArquivoResposta { get; protected set; }

    public string EnvelopeEnvio { get; protected set; }

    public string EnvelopeRetorno { get; protected set; }

    protected string Url { get; set; }

    protected X509Certificate2 Certificado => Configuracoes.Certificados.ObterCertificado();

    protected bool IsDisposed { get; private set; }

    #endregion Properties

    #region Methods

    protected void Execute(string contentType, string method, NameValueCollection headers = null)
    {
        var protocolos = ServicePointManager.SecurityProtocol;
        ServicePointManager.SecurityProtocol = Configuracoes.WebServices.Protocolos;

        try
        {
            var request = WebRequest.CreateHttp(Url);
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

    protected virtual bool ValidarCertificadoServidor() => true;

    /// <summary>
    /// Salvar o arquivo xml no disco de acordo com as propriedades.
    /// </summary>
    /// <param name="conteudoArquivo"></param>
    /// <param name="nomeArquivo"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    protected abstract void GravarSoap(string conteudoArquivo, string nomeArquivo);

    /// <inheritdoc />
    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // Dispose all managed and unmanaged resources.
        Dispose(true);

        // Take this object off the finalization queue and prevent finalization code for this
        // object from executing a second time.
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the managed resources implementing <see cref="IDisposable"/>.
    /// </summary>
    protected virtual void DisposeManaged()
    {
    }

    /// <summary>
    /// Disposes the unmanaged resources implementing <see cref="IDisposable"/>.
    /// </summary>
    protected virtual void DisposeUnmanaged()
    {
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
    /// <c>false</c> to release only unmanaged resources, called from the finalizer only.</param>
    private void Dispose(bool disposing)
    {
        // Check to see if Dispose has already been called.
        if (IsDisposed)
            return;

        // If disposing managed and unmanaged resources.
        if (disposing)
            DisposeManaged();

        DisposeUnmanaged();

        IsDisposed = true;
    }

    #endregion Methods
}