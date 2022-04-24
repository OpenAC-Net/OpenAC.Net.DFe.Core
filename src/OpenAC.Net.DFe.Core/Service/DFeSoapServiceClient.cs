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
using System.Linq;
using System.Text;
using System.Xml.Linq;
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

    protected DFeSoapServiceClient(string url) : base(url)
    {
    }

    #endregion Constructors

    #region Properties

    protected SoapVersion MessageVersion { get; }

    protected string CharSet { get; set; } = "utf-8";

    #endregion Properties

    #region Methods

    protected virtual string Execute(string soapAction, string message, string responseTag, params string[] soapNamespaces)
    {
        return Execute(soapAction, message, string.Empty, new[] { responseTag }, soapNamespaces);
    }

    protected virtual string Execute(string soapAction, string message, string[] responseTag, params string[] soapNamespaces)
    {
        return Execute(soapAction, message, string.Empty, responseTag, soapNamespaces);
    }

    protected virtual string Execute(string soapAction, string message, string soapHeader, string responseTag, params string[] soapNamespaces)
    {
        return Execute(soapAction, message, soapHeader, new[] { responseTag }, soapNamespaces);
    }

    protected virtual string Execute(string soapAction, string message, string soapHeader, string[] responseTag, params string[] soapNamespaces)
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
        var retorno = TratarRetorno(body, responseTag);
        if (retorno.IsValidXml()) return retorno;

        throw new OpenDFeCommunicationException(retorno);
    }

    protected abstract string TratarRetorno(XElement xmlDocument, string[] responseTag);

    #endregion Methods
}