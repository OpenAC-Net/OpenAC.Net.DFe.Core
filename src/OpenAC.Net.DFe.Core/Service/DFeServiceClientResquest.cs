// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 11-10-2021
//
// Last Modified By : RFTD
// Last Modified On : 11-10-2021
// ***********************************************************************
// <copyright file="DFeServiceClientResquest.cs" company="OpenAC .Net">
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
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using OpenAC.Net.Core;
using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core.Common;

namespace OpenAC.Net.DFe.Core.Service
{
    public abstract class DFeServiceClientResquest<TDFeConfig, TParent, TGeralConfig, TVersaoDFe, TWebserviceConfig, TCertificadosConfig, TArquivosConfig, TSchemas> : DFeServiceClientBase<IRequestChannel>
        where TDFeConfig : DFeConfigBase<TParent, TGeralConfig, TVersaoDFe, TWebserviceConfig, TCertificadosConfig, TArquivosConfig, TSchemas>
        where TParent : OpenComponent
        where TGeralConfig : DFeGeralConfigBase<TParent, TVersaoDFe>
        where TVersaoDFe : Enum
        where TWebserviceConfig : DFeWebserviceConfigBase<TParent>
        where TCertificadosConfig : DFeCertificadosConfigBase<TParent>
        where TArquivosConfig : DFeArquivosConfigBase<TParent, TSchemas>
        where TSchemas : Enum
    {
        #region Fields

        /// <summary>
        /// 
        /// </summary>
        protected readonly object serviceLock;

        #endregion Fields

        #region Constructors

        /// <inheritdoc />
        protected DFeServiceClientResquest(TDFeConfig config, string url, X509Certificate2 certificado = null) : base(url, config.WebServices.TimeOut, certificado)
        {
            serviceLock = new object();
            Configuracoes = config;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///
        /// </summary>
        public TDFeConfig Configuracoes { get; }
        
        /// <summary>
        ///
        /// </summary>
        public string PrefixoEnvio { get; protected set; }

        /// <summary>
        ///
        /// </summary>
        public string PrefixoResposta { get; protected set; }

        /// <summary>
        ///
        /// </summary>
        public string ArquivoEnvio { get; protected set; }

        /// <summary>
        ///
        /// </summary>
        public string ArquivoResposta { get; protected set; }

        /// <summary>
        ///
        /// </summary>
        public string EnvelopeEnvio { get; protected set; }

        /// <summary>
        ///
        /// </summary>
        public string EnvelopeRetorno { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public bool ValidarCertificadoServidor { get; set; }

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
            var request = WriteSoapEnvelope(message, soapAction, soapHeader, soapNamespaces);

            RemoteCertificateValidationCallback validation = null;
            var naoValidarCertificado = !ValidarCertificadoServidor;

            if (naoValidarCertificado)
            {
                validation = ServicePointManager.ServerCertificateValidationCallback;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, certificate, chain, sslPolicyErrors) => true;
            }

            string soapResponse;

            try
            {
                lock (serviceLock)
                {
                    var response = Channel.Request(request);
                    Guard.Against<OpenDFeException>(response == null, "Nenhum retorno do webservice.");
                    var reader = response.GetReaderAtBodyContents();
                    soapResponse = reader.ReadOuterXml();
                }
            }
            finally
            {
                if (naoValidarCertificado)
                    ServicePointManager.ServerCertificateValidationCallback = validation;
            }

            var xmlDocument = XDocument.Parse(soapResponse);
            return TratarRetorno(xmlDocument, responseTag);
        }

        /// <summary>
        /// Metodo para gerar o pacote soap.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="soapAction"></param>
        /// <param name="soapHeader"></param>
        /// <param name="soapNamespaces"></param>
        /// <returns></returns>
        protected virtual Message WriteSoapEnvelope(string message, string soapAction, string soapHeader, string[] soapNamespaces)
        {
            var envelope = new StringBuilder();
            envelope.Append("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\"");

            foreach (var ns in soapNamespaces)
            {
                envelope.Append($" {ns}");
            }

            envelope.Append(">");
            envelope.Append(soapHeader.IsEmpty() ? "<soapenv:Header/>" : $"<soapenv:Header>{soapHeader}</soapenv:Header>");
            envelope.Append("<soapenv:Body>");
            envelope.Append(message);
            envelope.Append("</soapenv:Body>");
            envelope.Append("</soapenv:Envelope>");

            //Separei em uma variável para conseguir visualizar o envelope em formato XML durante a depuração
            var EnvelopeString = envelope.ToString();
            var SR = new StringReader(EnvelopeString);
            var XmlR = XmlReader.Create(SR);
            var request = Message.CreateMessage(XmlR, int.MaxValue, Endpoint.Binding.MessageVersion);

            //Define a action no Header por ser SOAP 1.1
            var requestMessage = new HttpRequestMessageProperty
            {
                Headers =
                {
                    ["SOAPAction"] = soapAction
                }
            };

            request.Properties[HttpRequestMessageProperty.Name] = requestMessage;
            return request;
        }

        protected abstract string TratarRetorno(XDocument xmlDocument, string[] responseTag);

        /// <summary>
        /// Função para validar a menssagem a ser enviada para o webservice.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="schema"></param>
        protected virtual void ValidateMessage(string xml, TSchemas schema)
        {
            var schemaFile = Configuracoes.Arquivos.GetSchema(schema);
            XmlSchemaValidation.ValidarXml(xml, schemaFile, out var erros, out _);

            Guard.Against<OpenDFeValidationException>(erros.Any(), "Erros de validação do xml." +
                                                                   $"{(Configuracoes.Geral.ExibirErroSchema ? Environment.NewLine + erros.AsString() : "")}");
        }

        protected abstract void GravarXml(string conteudoArquivo, string nomeArquivo);

        /// <summary>
        /// Salvar o arquivo xml no disco de acordo com as propriedades.
        /// </summary>
        /// <param name="conteudoArquivo"></param>
        /// <param name="nomeArquivo"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        protected void GravarSoap(string conteudoArquivo, string nomeArquivo)
        {
            if (Configuracoes.WebServices.Salvar == false) return;
            
            if (!Directory.Exists(Configuracoes.Arquivos.PathSalvar))
                Directory.CreateDirectory(Configuracoes.Arquivos.PathSalvar);

            nomeArquivo = Path.Combine(Configuracoes.Arquivos.PathSalvar, nomeArquivo);
            File.WriteAllText(nomeArquivo, conteudoArquivo, Encoding.UTF8);
        }

        /// <inheritdoc />
        protected override void BeforeSendDFeRequest(string message)
        {
            EnvelopeEnvio = message;
            GravarSoap(message, $"{DateTime.Now:yyyyMMddHHmmssfff}_{ArquivoEnvio}_env.xml");
        }

        /// <inheritdoc />
        protected override void AfterReceiveDFeReply(string message)
        {
            EnvelopeRetorno = message;
            GravarSoap(message, $"{DateTime.Now:yyyyMMddHHmmssfff}_{ArquivoResposta}_ret.xml");
        }

        #endregion Methods
    }
}