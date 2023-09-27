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

public abstract class DFeSoapServiceClient<TDFeConfig, TGeralConfig, TWebserviceConfig, TCertificadosConfig, TArquivosConfig> :
    DFeServiceClientBase<TDFeConfig, TGeralConfig, TWebserviceConfig, TCertificadosConfig, TArquivosConfig>
    where TDFeConfig : DFeConfigBase<TGeralConfig, TWebserviceConfig, TCertificadosConfig, TArquivosConfig>
    where TGeralConfig : DFeGeralConfigBase
    where TWebserviceConfig : DFeWebserviceConfigBase
    where TCertificadosConfig : DFeCertificadosConfigBase
    where TArquivosConfig : DFeArquivosConfigBase
{
    #region Inner Types

    public enum SoapVersion
    {
        Soap11,
        Soap12,
    }

    #endregion Inner Types

    #region Constructors

    /// <summary>
    /// Inicializa uma nova instancia da classe <see cref="DFeSoapServiceClient{T, T, T, T, T}"/>.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="url"></param>
    /// <param name="version"></param>
    protected DFeSoapServiceClient(TDFeConfig config, string url, SoapVersion version) : base(config, url)
    {
        MessageVersion = version;
    }

    #endregion Constructors

    #region Properties

    protected SoapVersion MessageVersion { get; }

    protected string CharSet { get; set; } = "utf-8";

    #endregion Properties

    #region Methods

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
    /// Salvar o arquivo xml no disco de acordo com as propriedades.
    /// </summary>
    /// <param name="conteudoArquivo"></param>
    /// <param name="nomeArquivo"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    protected override void GravarSoap(string conteudoArquivo, string nomeArquivo)
    {
        if (Configuracoes.WebServices.Salvar == false) return;

        if (!Directory.Exists(Configuracoes.Arquivos.PathSalvar))
            Directory.CreateDirectory(Configuracoes.Arquivos.PathSalvar);

        nomeArquivo = Path.Combine(Configuracoes.Arquivos.PathSalvar, nomeArquivo);
        File.WriteAllText(nomeArquivo, conteudoArquivo, Encoding.UTF8);
    }

    /// <summary>
    /// Função para validar a menssagem a ser enviada para o webservice.
    /// </summary>
    /// <param name="xml"></param>
    /// <param name="schemaFile"></param>
    protected virtual void ValidateMessage(string xml, string schemaFile)
    {
        Guard.Against<FileNotFoundException>(!File.Exists(schemaFile), "Schema não encontrado.");
        XmlSchemaValidation.ValidarXml(xml, schemaFile, out var erros, out _);

        Guard.Against<OpenDFeValidationException>(erros.Any(), "Erros de validação do xml." +
                                                               $"{(Configuracoes.Geral.ExibirErroSchema ? Environment.NewLine + erros.AsString() : "")}");
    }

    protected abstract string TratarRetorno(XElement xmlDocument);

    #endregion Methods
}