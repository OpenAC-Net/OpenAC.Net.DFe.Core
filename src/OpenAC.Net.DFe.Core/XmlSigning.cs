// ***********************************************************************
// Assembly         : OpenAC.Net.Core
// Author           : RFTD
// Created          : 12-27-2017
//
// Last Modified By : RFTD
// Last Modified On : 09-22-2020
// ***********************************************************************
// <copyright file="XmlSigning.cs" company="OpenAC .Net">
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
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using OpenAC.Net.Core;
using OpenAC.Net.Core.Extensions;
using OpenAC.Net.Core.Logging;
using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Common;
using OpenAC.Net.DFe.Core.Document;
using KeyInfo = System.Security.Cryptography.Xml.KeyInfo;
using Reference = System.Security.Cryptography.Xml.Reference;

namespace OpenAC.Net.DFe.Core
{
    /// <summary>
    /// Métodos utilitários e extensões para assinatura digital XML-DSig e validação de assinaturas em documentos DFe (SHA1 e SHA256).
    /// </summary>
    public static class XmlSigning
    {
        #region Methods

        /// <summary>
        /// Realiza a assinatura digital do XML informado utilizando o certificado digital.
        /// </summary>
        /// <param name="xml">String contendo o XML a ser assinado.</param>
        /// <param name="docElement">Nome do elemento principal onde a assinatura será anexada.</param>
        /// <param name="infoElement">Nome do elemento identificado com a tag de assinatura.</param>
        /// <param name="pCertificado">Certificado digital X509 com chave privada.</param>
        /// <param name="comments">Se <c>true</c>, insere o transform #withcomments.</param>
        /// <param name="identado">Se <c>true</c>, retorna o XML assinado com indentação.</param>
        /// <param name="showDeclaration">Se <c>true</c>, inclui a declaração XML inicial.</param>
        /// <param name="digest">Algoritmo de resumo criptográfico (padrão SHA1).</param>
        /// <returns>O XML assinado como string.</returns>
        /// <exception cref="OpenDFeException">Disparada em caso de falha na assinatura digital.</exception>
        public static string AssinarXml(string xml, string docElement, string infoElement, X509Certificate2 pCertificado,
            bool comments = false, bool identado = false, bool showDeclaration = true, SignDigest digest = SignDigest.SHA1)
        {
            return AssinarXml(xml, docElement, infoElement, "Id", pCertificado, comments, identado, showDeclaration, digest);
        }

        /// <summary>
        /// Realiza a assinatura digital do XML informado especificando o atributo identificador.
        /// </summary>
        /// <param name="xml">String contendo o XML a ser assinado.</param>
        /// <param name="docElement">Nome do elemento principal onde a assinatura será anexada.</param>
        /// <param name="infoElement">Nome do elemento identificado com a tag de assinatura.</param>
        /// <param name="signAtribute">Nome do atributo identificador (ex: "Id").</param>
        /// <param name="pCertificado">Certificado digital X509 com chave privada.</param>
        /// <param name="comments">Se <c>true</c>, insere o transform #withcomments.</param>
        /// <param name="identado">Se <c>true</c>, retorna o XML assinado com indentação.</param>
        /// <param name="showDeclaration">Se <c>true</c>, inclui a declaração XML inicial.</param>
        /// <param name="digest">Algoritmo de resumo criptográfico (padrão SHA1).</param>
        /// <returns>O XML assinado como string.</returns>
        /// <exception cref="OpenDFeException">Disparada em caso de falha na assinatura digital.</exception>
        public static string AssinarXml(string xml, string docElement, string infoElement, string signAtribute, X509Certificate2 pCertificado,
            bool comments = false, bool identado = false, bool showDeclaration = true, SignDigest digest = SignDigest.SHA1)
        {
            try
            {
                var xmlDoc = new XmlDocument { PreserveWhitespace = true };
                xmlDoc.LoadXml(xml);
                AssinarDocumento(xmlDoc, docElement, infoElement, signAtribute, pCertificado, comments, digest);
                return xmlDoc.AsString(identado, showDeclaration);
            }
            catch (Exception ex)
            {
                throw new OpenDFeException("Erro ao efetuar assinatura digital.", ex);
            }
        }

        /// <summary>
        /// Assina múltiplos elementos contidos no mesmo documento XML (ex: lote de eventos).
        /// </summary>
        /// <param name="xml">String contendo o XML do lote.</param>
        /// <param name="docElement">Nome do elemento de cada documento a ser assinado.</param>
        /// <param name="infoElement">Nome do elemento identificado para assinatura.</param>
        /// <param name="certificado">Certificado digital X509 com chave privada.</param>
        /// <param name="comments">Se <c>true</c>, insere o transform #withcomments.</param>
        /// <param name="identado">Se <c>true</c>, retorna o XML assinado com indentação.</param>
        /// <param name="showDeclaration">Se <c>true</c>, inclui a declaração XML inicial.</param>
        /// <param name="digest">Algoritmo de resumo criptográfico (padrão SHA1).</param>
        /// <returns>O XML com todos os elementos assinados.</returns>
        /// <exception cref="OpenDFeException">Disparada em caso de falha na assinatura digital.</exception>
        public static string AssinarXmlTodos(string xml, string docElement, string infoElement, X509Certificate2 certificado,
            bool comments = false, bool identado = false, bool showDeclaration = true, SignDigest digest = SignDigest.SHA1)
        {
            return AssinarXmlTodos(xml, docElement, infoElement, "Id", certificado, comments, identado, showDeclaration, digest);
        }

        /// <summary>
        /// Assina múltiplos elementos contidos no mesmo documento XML especificando o atributo identificador.
        /// </summary>
        /// <param name="xml">String contendo o XML do lote.</param>
        /// <param name="docElement">Nome do elemento de cada documento a ser assinado.</param>
        /// <param name="infoElement">Nome do elemento identificado para assinatura.</param>
        /// <param name="signAtribute">Nome do atributo identificador (ex: "Id").</param>
        /// <param name="certificado">Certificado digital X509 com chave privada.</param>
        /// <param name="comments">Se <c>true</c>, insere o transform #withcomments.</param>
        /// <param name="identado">Se <c>true</c>, retorna o XML assinado com indentação.</param>
        /// <param name="showDeclaration">Se <c>true</c>, inclui a declaração XML inicial.</param>
        /// <param name="digest">Algoritmo de resumo criptográfico (padrão SHA1).</param>
        /// <returns>O XML com todos os elementos assinados.</returns>
        /// <exception cref="OpenDFeException">Disparada em caso de falha na assinatura digital.</exception>
        public static string AssinarXmlTodos(string xml, string docElement, string infoElement, string signAtribute, X509Certificate2 certificado,
            bool comments = false, bool identado = false, bool showDeclaration = true, SignDigest digest = SignDigest.SHA1)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);

                XmlElement[] xmlElements;

                if (infoElement.IsEmpty())
                {
                    xmlElements = doc.GetElementsByTagName(docElement).Cast<XmlElement>().ToArray();
                }
                else
                {
                    xmlElements = doc.GetElementsByTagName(docElement).Cast<XmlElement>()
                        .Where(x => x.GetElementsByTagName(infoElement).Count == 1).ToArray();
                    Guard.Against<OpenDFeException>(!xmlElements.Any(), "Nome do elemento de assinatura incorreto");
                }

                foreach (var element in xmlElements)
                {
                    var xmlDoc = new XmlDocument { PreserveWhitespace = true };
                    xmlDoc.LoadXml(element.OuterXml);
                    AssinarDocumento(xmlDoc, docElement, infoElement, signAtribute, certificado, comments, digest);

                    // ReSharper disable once AssignNullToNotNullAttribute
                    var signedElement = doc.ImportNode(xmlDoc.DocumentElement, true);
                    element.ParentNode?.ReplaceChild(signedElement, element);
                }

                return doc.AsString(identado, showDeclaration);
            }
            catch (Exception ex)
            {
                throw new OpenDFeException("Erro ao efetuar assinatura digital.", ex);
            }
        }

        /// <summary>
        /// Realiza a assinatura digital diretamente no objeto <see cref="XmlDocument"/> fornecido.
        /// </summary>
        /// <param name="doc">O documento <see cref="XmlDocument"/>.</param>
        /// <param name="docElement">Nome do elemento onde a tag Signature será anexada.</param>
        /// <param name="infoElement">Nome do elemento assinado referenciado.</param>
        /// <param name="signAtribute">Nome do atributo identificador da URI assinada.</param>
        /// <param name="certificado">Certificado digital X509 com chave privada.</param>
        /// <param name="comments">Se <c>true</c>, insere o transform #withcomments.</param>
        /// <param name="digest">Algoritmo de resumo criptográfico (padrão SHA1).</param>
        /// <exception cref="OpenDFeException">Disparada em caso de falha na assinatura.</exception>
        public static void AssinarDocumento(this XmlDocument doc, string docElement, string infoElement, string signAtribute,
            X509Certificate2 certificado, bool comments = false, SignDigest digest = SignDigest.SHA1)
        {
            Guard.Against<ArgumentNullException>(doc == null, "XmlDOcument não pode ser nulo.");
            Guard.Against<ArgumentException>(docElement.IsEmpty(), "docElement não pode ser nulo ou vazio.");

            var xmlDigitalSignature = GerarAssinatura(doc, infoElement, signAtribute, certificado, comments, digest);
            var xmlElement = doc.GetElementsByTagName(docElement).Cast<XmlElement>().FirstOrDefault();

            Guard.Against<OpenDFeException>(xmlElement == null, "Elemento principal não encontrado.");

            var element = doc.ImportNode(xmlDigitalSignature, true);
            xmlElement.AppendChild(element);
        }

        /// <summary>
        /// Gera a assinatura digital de uma instância de documento <see cref="DFeSignDocument{TDocument}"/> e retorna a estrutura <see cref="DFeSignature"/>.
        /// </summary>
        /// <typeparam name="TDocument">O tipo concreto do documento assinado.</typeparam>
        /// <param name="document">A instância do documento DFe.</param>
        /// <param name="certificado">O certificado digital X509 com chave privada.</param>
        /// <param name="comments">Se <c>true</c>, insere o transform #withcomments.</param>
        /// <param name="digest">Algoritmo de resumo criptográfico (SHA1 ou SHA256).</param>
        /// <param name="options">Opções de salvamento e formatação do XML.</param>
        /// <param name="signedXml">Parâmetro de saída com a string do XML assinado gerado.</param>
        /// <returns>A instância deserializada de <see cref="DFeSignature"/> correspondente à assinatura gerada.</returns>
        public static DFeSignature AssinarDocumento<TDocument>(this DFeSignDocument<TDocument> document,
            X509Certificate2 certificado, bool comments, SignDigest digest,
            DFeSaveOptions options, out string signedXml) where TDocument : class
        {
            Guard.Against<ArgumentException>(!typeof(TDocument).HasAttribute<DFeSignInfoElement>(), "Atributo [DFeSignInfoElement] não encontrado.");

            var xml = document.GetXml(options, Encoding.UTF8);
            var xmlDoc = new XmlDocument { PreserveWhitespace = true };
            xmlDoc.LoadXml(xml);

            var signatureInfo = typeof(TDocument).GetAttribute<DFeSignInfoElement>();
            var xmlSignature = GerarAssinatura(xmlDoc, signatureInfo.SignElement, signatureInfo.SignAtribute, certificado, comments, digest);

            // Adiciona a assinatura no documento e retorna o xml assinado no parametro signedXml
            var element = xmlDoc.ImportNode(xmlSignature, true);
            xmlDoc.DocumentElement?.AppendChild(element);
            signedXml = xmlDoc.AsString(!options.HasFlag(DFeSaveOptions.DisableFormatting), !options.HasFlag(DFeSaveOptions.OmitDeclaration));

            return DFeSignature.Load(xmlSignature.OuterXml);
        }

        /// <summary>
        /// Valida a integridade criptográfica da assinatura digital em um documento <see cref="DFeSignDocument{TDocument}"/>.
        /// </summary>
        /// <typeparam name="TDocument">O tipo do documento assinado.</typeparam>
        /// <param name="document">A instância do documento.</param>
        /// <param name="gerarXml">Indica se deve serializar um novo XML para validação se o cache estiver vazio.</param>
        /// <returns><c>true</c> se a assinatura for válida; caso contrário, <c>false</c>.</returns>
        public static bool ValidarAssinatura<TDocument>(this DFeSignDocument<TDocument> document, bool gerarXml) where TDocument : class
        {
            var xml = document.Xml.IsEmpty() || gerarXml ? document.GetXml(DFeSaveOptions.DisableFormatting, Encoding.UTF8) : document.Xml;
            var xmlDoc = new XmlDocument { PreserveWhitespace = true };
            xmlDoc.LoadXml(xml);
            return ValidarAssinatura(xmlDoc);
        }

        /// <summary>
        /// Valida a integridade criptográfica da assinatura digital contida em um <see cref="XmlDocument"/>.
        /// </summary>
        /// <param name="doc">O documento <see cref="XmlDocument"/> contendo a tag Signature.</param>
        /// <returns><c>true</c> se a assinatura for matematicamente válida contra a chave pública contida; caso contrário, <c>false</c>.</returns>
        public static bool ValidarAssinatura(this XmlDocument doc)
        {
            try
            {
                var signElement = doc.GetElementsByTagName("Signature");
                Guard.Against<OpenDFeException>(signElement.Count < 1, "Verificação falhou: Elemento [Signature] não encontrado no documento.");
                Guard.Against<OpenDFeException>(signElement.Count > 1, "Verificação falhou: Mais de um elemento [Signature] encontrado no documento.");

                var certificateElement = doc.GetElementsByTagName("X509Certificate");
                Guard.Against<OpenDFeException>(certificateElement.Count < 1, "Verificação falhou: Elemento [X509Certificate] não encontrado no documento.");
                Guard.Against<OpenDFeException>(certificateElement.Count > 1, "Verificação falhou: Mais de um elemento [X509Certificate] encontrado no documento.");

                var signedXml = new SignedXml(doc);
                signedXml.LoadXml((XmlElement)signElement[0]);

                var certificate = new X509Certificate2(Convert.FromBase64String(certificateElement[0].InnerText));

                return signedXml.CheckSignature(certificate, true);
            }
            catch (Exception exception)
            {
                var log = LoggerProvider.LoggerFor(typeof(XmlSigning));
                log.Error("Erro ao validar a assinatura.", exception);
                return false;
            }
        }
        
        private static XmlElement GerarAssinatura(XmlDocument doc, string infoElement, string signAtribute,
            X509Certificate2 certificado, bool comments, SignDigest digest)
        {
            Guard.Against<ArgumentException>(!infoElement.IsEmpty() && doc.GetElementsByTagName(infoElement).Count != 1, "Referencia invalida ou não é unica.");

            var uri = infoElement.IsEmpty() || signAtribute.IsEmpty() ? "" :
                $"#{doc.GetElementsByTagName(infoElement)[0].Attributes?[signAtribute]?.InnerText}";
            
            // Adiciona Certificado ao Key Info
            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(certificado));

            // Seta chaves
            var signedDocument = new SignedXml(doc)
            {
                SigningKey = certificado.GetRSAPrivateKey(),
                KeyInfo = keyInfo,
                SignedInfo =
                {
                    CanonicalizationMethod = comments ? SignedXml.XmlDsigC14NWithCommentsTransformUrl : SignedXml.XmlDsigC14NTransformUrl,
                    SignatureMethod = GetSignatureMethod(digest)
                }
            };

            // Cria referencia
            var reference = new Reference
            {
                Uri = uri,
                DigestMethod = GetDigestMethod(digest)
            };
            
            // Adiciona referencia ao xml
            signedDocument.AddReference(reference);

            // Adiciona transformação a referencia
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigC14NTransform(comments));

            // Calcula Assinatura
            signedDocument.ComputeSignature();

            // Pega representação da assinatura
            return signedDocument.GetXml();
        }

        private static string GetSignatureMethod(SignDigest digest)
        {
            switch (digest)
            {
                case SignDigest.SHA1:
                    return SignedXml.XmlDsigRSASHA1Url;

                case SignDigest.SHA256:
                    return SignedXml.XmlDsigRSASHA256Url;

                default:
                    throw new ArgumentOutOfRangeException(nameof(digest), digest, null);
            }
        }
        
        private static string GetDigestMethod(SignDigest digest)
        {
            return digest switch
            {
                SignDigest.SHA1 => SignedXml.XmlDsigSHA1Url,
                SignDigest.SHA256 => SignedXml.XmlDsigSHA256Url,
                _ => throw new ArgumentOutOfRangeException(nameof(digest), digest, null)
            };
        }

        #endregion Methods
    }
}