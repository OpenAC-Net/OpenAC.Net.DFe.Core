// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 11-10-2021
//
// Last Modified By : RFTD
// Last Modified On : 11-10-2021
// ***********************************************************************
// <copyright file="DFeServiceClientResquest12.cs" company="OpenAC .Net">
//		        		   The MIT License (MIT)
//	     		    Copyright (c) 2016 Grupo OpenAC.Net
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
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using OpenAC.Net.Core;
using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core.Common;

namespace OpenAC.Net.DFe.Core.Service
{
    public abstract class DFeServiceClientResquest12<TDFeConfig, TParent, TGeralConfig, TVersaoDFe, TWebserviceConfig, TCertificadosConfig, TArquivosConfig, TSchemas> :
        DFeServiceClientResquest<TDFeConfig, TParent, TGeralConfig, TVersaoDFe, TWebserviceConfig, TCertificadosConfig, TArquivosConfig, TSchemas>
        where TDFeConfig : DFeConfigBase<TParent, TGeralConfig, TVersaoDFe, TWebserviceConfig, TCertificadosConfig, TArquivosConfig, TSchemas>
        where TParent : OpenComponent
        where TGeralConfig : DFeGeralConfigBase<TParent, TVersaoDFe>
        where TVersaoDFe : Enum
        where TWebserviceConfig : DFeWebserviceConfigBase<TParent>
        where TCertificadosConfig : DFeCertificadosConfigBase<TParent>
        where TArquivosConfig : DFeArquivosConfigBase<TParent, TSchemas>
        where TSchemas : Enum
    {
        #region Constructors

        /// <inheritdoc />
        protected DFeServiceClientResquest12(TDFeConfig config, string url, X509Certificate2 certificado = null) : base(config, url, certificado)
        {
            var custom = new CustomBinding(Endpoint.Binding);
            var version = custom.Elements.Find<TextMessageEncodingBindingElement>();
            version.MessageVersion = MessageVersion.CreateVersion(EnvelopeVersion.Soap12, AddressingVersion.None);

            Endpoint.Binding = custom;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Metodo para gerar o pacote soap.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="soapAction"></param>
        /// <param name="soapHeader"></param>
        /// <param name="soapNamespaces"></param>
        /// <returns></returns>
        protected override Message WriteSoapEnvelope(string message, string soapAction, string soapHeader, string[] soapNamespaces)
        {
            var envelope = new StringBuilder();
            envelope.Append("<soapenv:Envelope xmlns:soapenv=\"http://www.w3.org/2003/05/soap-envelope\"");

            foreach (var ns in soapNamespaces)
                envelope.Append($" {ns}");

            envelope.Append(">");
            envelope.Append(soapHeader.IsEmpty() ? "<soapenv:Header/>" : $"<soapenv:Header>{soapHeader}</soapenv:Header>");
            envelope.Append("<soapenv:Body>");
            envelope.Append(message);
            envelope.Append("</soapenv:Body>");
            envelope.Append("</soapenv:Envelope>");

            var request = Message.CreateMessage(XmlReader.Create(new StringReader(envelope.ToString())), int.MaxValue, Endpoint.Binding.MessageVersion);

            //Define a action no content type por ser SOAP 1.2
            var requestMessage = new HttpRequestMessageProperty
            {
                Headers =
                {
                    ["Content-Type"] = $"application/soap+xml;charset=UTF-8;action=\"{soapAction}\""
                }
            };

            request.Properties[HttpRequestMessageProperty.Name] = requestMessage;

            return request;
        }

        #endregion Methods
    }
}