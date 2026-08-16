// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 23-04-2022
//
// Last Modified By : RFTD
// Last Modified On : 23-04-2022
// ***********************************************************************
// <copyright file="DFeSoapServiceClient.cs" company="OpenAC .Net">
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
using System.Linq;
using System.Text;
using System.Xml.Linq;
using OpenAC.Net.Core;
using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core.Common;
using OpenAC.Net.DFe.Core.Extensions;

namespace OpenAC.Net.DFe.Core.Service;

/// <summary>
/// Cliente base para comunicação com Web Services DFe utilizando protocolo SOAP (SOAP 1.1 e SOAP 1.2).
/// </summary>
/// <typeparam name="TDFeConfig">Tipo das configurações DFe do componente.</typeparam>
/// <typeparam name="TGeralConfig">Tipo das configurações gerais.</typeparam>
/// <typeparam name="TWebserviceConfig">Tipo das configurações de Web Services.</typeparam>
/// <typeparam name="TCertificadosConfig">Tipo das configurações de certificados digitais.</typeparam>
/// <typeparam name="TArquivosConfig">Tipo das configurações de arquivos.</typeparam>
public abstract class DFeSoapServiceClient<TDFeConfig, TGeralConfig, TWebserviceConfig, TCertificadosConfig, TArquivosConfig> :
    DFeServiceClientBase<TDFeConfig, TGeralConfig, TWebserviceConfig, TCertificadosConfig, TArquivosConfig>
    where TDFeConfig : DFeConfigBase<TGeralConfig, TWebserviceConfig, TCertificadosConfig, TArquivosConfig>
    where TGeralConfig : DFeGeralConfigBase
    where TWebserviceConfig : DFeWebserviceConfigBase
    where TCertificadosConfig : DFeCertificadosConfigBase
    where TArquivosConfig : DFeArquivosConfigBase
{
    #region Inner Types

    /// <summary>
    /// Versões do protocolo SOAP suportadas.
    /// </summary>
    public enum SoapVersion
    {
        /// <summary>
        /// Protocolo SOAP 1.1 (<c>http://schemas.xmlsoap.org/soap/envelope/</c>).
        /// </summary>
        Soap11,

        /// <summary>
        /// Protocolo SOAP 1.2 (<c>http://www.w3.org/2003/05/soap-envelope</c>).
        /// </summary>
        Soap12,
    }

    #endregion Inner Types

    #region Constructors

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeSoapServiceClient{TDFeConfig, TGeralConfig, TWebserviceConfig, TCertificadosConfig, TArquivosConfig}"/>.
    /// </summary>
    /// <param name="config">Configurações do componente.</param>
    /// <param name="url">URL do Web Service.</param>
    /// <param name="version">Versão do protocolo SOAP utilizada.</param>
    protected DFeSoapServiceClient(TDFeConfig config, string url, SoapVersion version) : base(config, url)
    {
        MessageVersion = version;
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Obtém a versão do envelope SOAP utilizada nas requisições.
    /// </summary>
    protected SoapVersion MessageVersion { get; }

    /// <summary>
    /// Obtém ou define o conjunto de caracteres informado no header Content-Type (padrão utf-8).
    /// </summary>
    protected string CharSet { get; set; } = "utf-8";

    #endregion Properties

    #region Methods

    /// <summary>
    /// Monta o envelope SOAP, executa a requisição HTTP POST e extrai o corpo de resposta.
    /// </summary>
    /// <param name="soapAction">Ação SOAP (header SOAPAction ou parâmetro de action no content-type).</param>
    /// <param name="message">Corpo da mensagem XML inserida no <c>&lt;soap:Body&gt;</c>.</param>
    /// <param name="soapHeader">Conteúdo do cabeçalho SOAP inserido no <c>&lt;soap:Header&gt;</c>.</param>
    /// <param name="soapNamespaces">Namespaces adicionais a serem incluídos na tag <c>&lt;soap:Envelope&gt;</c>.</param>
    /// <returns>A string do XML resultante extraído do corpo de resposta.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Disparado quando a versão SOAP informada for inválida.</exception>
    /// <exception cref="OpenDFeCommunicationException">Disparado quando a resposta obtida não for um XML válido.</exception>
    protected virtual string Execute(string soapAction, string message, string soapHeader, params string[] soapNamespaces)
    {
        string contetType;
        NameValueCollection headers;
        switch (MessageVersion)
        {
            case SoapVersion.Soap11:
                contetType = $"text/xml; charset={CharSet}";
                headers = new NameValueCollection { { "SOAPAction", soapAction } };
                break;

            case SoapVersion.Soap12:
                contetType = $"application/soap+xml; charset={CharSet};action={soapAction}";
                headers = null;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        var envelope = new StringBuilder();
        switch (MessageVersion)
        {
            case SoapVersion.Soap11:
                envelope.Append("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\"");
                break;

            case SoapVersion.Soap12:
                envelope.Append("<soapenv:Envelope xmlns:soapenv=\"http://www.w3.org/2003/05/soap-envelope\"");
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        envelope.Append(soapNamespaces.Aggregate("", (atual, next) => atual + $" {next}", namespaces => namespaces + ">"));
        envelope.Append(soapHeader.IsEmpty() ? "<soapenv:Header/>" : $"<soapenv:Header>{soapHeader}</soapenv:Header>");
        envelope.Append("<soapenv:Body>");
        envelope.Append(message);
        envelope.Append("</soapenv:Body>");
        envelope.Append("</soapenv:Envelope>");
        EnvelopeEnvio = envelope.ToString();

        Execute(contetType, "POST", headers);

        var xmlDocument = XDocument.Parse(EnvelopeRetorno);
        var body = xmlDocument.ElementAnyNs("Envelope").ElementAnyNs("Body");
        var retorno = TratarRetorno(body);

        if (retorno.IsValidXml()) return retorno;

        throw new OpenDFeCommunicationException(retorno);
    }

    /// <summary>
    /// Grava em disco o envelope SOAP enviado ou recebido.
    /// </summary>
    /// <param name="conteudoArquivo">Conteúdo do arquivo.</param>
    /// <param name="nomeArquivo">Nome do arquivo.</param>
    protected override void GravarSoap(string conteudoArquivo, string nomeArquivo)
    {
        if (Configuracoes.WebServices.Salvar == false) return;

        if (!Directory.Exists(Configuracoes.Arquivos.PathSalvar))
            Directory.CreateDirectory(Configuracoes.Arquivos.PathSalvar);

        nomeArquivo = Path.Combine(Configuracoes.Arquivos.PathSalvar, nomeArquivo);
        File.WriteAllText(nomeArquivo, conteudoArquivo, Encoding.UTF8);
    }

    /// <summary>
    /// Valida a mensagem XML contra o arquivo de schema XSD antes do envio ao Web Service.
    /// </summary>
    /// <param name="xml">Conteúdo XML a ser validado.</param>
    /// <param name="schemaFile">Caminho para o arquivo .xsd.</param>
    /// <exception cref="FileNotFoundException">Disparada quando o arquivo de schema XSD não for localizado.</exception>
    /// <exception cref="OpenDFeValidationException">Disparada quando forem detectados erros de validação estrutural contra o XSD.</exception>
    protected virtual void ValidateMessage(string xml, string schemaFile)
    {
        Guard.Against<FileNotFoundException>(!File.Exists(schemaFile), "Schema não encontrado.");
        XmlSchemaValidation.ValidarXml(xml, schemaFile, out var erros, out _);

        Guard.Against<OpenDFeValidationException>(erros.Any(), "Erros de validação do xml." +
                                                               $"{(Configuracoes.Geral.ExibirErroSchema ? Environment.NewLine + erros.AsString() : "")}");
    }

    /// <summary>
    /// Trata e extrai a mensagem de retorno específica a partir do elemento <c>&lt;soap:Body&gt;</c>.
    /// </summary>
    /// <param name="xmlDocument">Elemento <see cref="XElement"/> representando o <c>&lt;soap:Body&gt;</c>.</param>
    /// <returns>A string do conteúdo útil extraído do retorno.</returns>
    protected abstract string TratarRetorno(XElement xmlDocument);

    #endregion Methods
}