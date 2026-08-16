// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 09-11-2016
//
// Last Modified By : RFTD
// Last Modified On : 09-11-2016
// ***********************************************************************
// <copyright file="DFeDocument.cs" company="OpenAC .Net">
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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using OpenAC.Net.Core.Generics;
using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Common;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Document;

/// <summary>
/// Classe base abstrata para todos os documentos fiscais eletrônicos DFe com suporte a serialização, deserialização, carregamento e gravação de XML.
/// </summary>
/// <typeparam name="TDocument">O tipo concreto do documento derivado.</typeparam>
public abstract class DFeDocument<TDocument> : GenericClone<TDocument> where TDocument : class
{
    #region Properties

    /// <summary>
    /// Obtém o conteúdo XML bruto gerado ou carregado para esta instância de documento.
    /// </summary>
    [DFeIgnore]
    public string Xml { get; protected set; } = string.Empty;

    #endregion Properties

    #region Methods

    /// <summary>
    /// Serializa o documento para um objeto <see cref="XElement"/>.
    /// </summary>
    /// <param name="rootName">Nome customizado para a tag raiz (opcional).</param>
    /// <param name="rootNamespace">Namespace customizado para a tag raiz (opcional).</param>
    /// <param name="options">Opções de serialização (opcional).</param>
    /// <returns>O elemento XML (<see cref="XElement"/>) resultante da serialização.</returns>
    public abstract XElement WriteToXml(string? rootName = null, string? rootNamespace = null,
        SerializerOptions? options = null);

    /// <summary>
    /// Deserializa as propriedades do documento a partir de um objeto <see cref="XElement"/>.
    /// </summary>
    /// <param name="element">Elemento XML de origem contendo os dados do documento.</param>
    /// <param name="options">Opções de serialização (opcional).</param>
    public abstract void ReadXml(XElement element, SerializerOptions? options = null);

    /// <summary>
    /// Carrega e deserializa um documento DFe a partir de uma string XML ou caminho de arquivo em disco.
    /// </summary>
    /// <param name="document">String com o conteúdo XML ou caminho do arquivo .xml.</param>
    /// <param name="encoding">Codificação de caracteres do XML (padrão UTF-8).</param>
    /// <returns>A instância deserializada de <typeparamref name="TDocument"/>.</returns>
    public static TDocument Load(string document, Encoding? encoding = null)
    {
        var options = new SerializerOptions();
        if (encoding != null)
        {
            options.Encoding = encoding;
        }

        var content = File.Exists(document) ? File.ReadAllText(document, options.Encoding) : document;
        var xmlDoc = XDocument.Parse(content);

        var item = (TDocument)Activator.CreateInstance(typeof(TDocument))!;
        if (item is DFeDocument<TDocument> doc && xmlDoc.Root != null)
        {
            doc.ReadXml(xmlDoc.Root, options);
            doc.Xml = content;
        }
        return item;
    }

    /// <summary>
    /// Carrega e deserializa um documento DFe a partir de um <see cref="Stream"/>.
    /// </summary>
    /// <param name="document">Stream contendo os dados do documento XML.</param>
    /// <param name="encoding">Codificação de caracteres do XML (padrão UTF-8).</param>
    /// <returns>A instância deserializada de <typeparamref name="TDocument"/>.</returns>
    public static TDocument Load(Stream document, Encoding? encoding = null)
    {
        var options = new SerializerOptions();
        if (encoding != null)
        {
            options.Encoding = encoding;
        }

        using var reader = new StreamReader(document, options.Encoding, true, 1024, true);
        document.Position = 0;
        var content = reader.ReadToEnd();
        var xmlDoc = XDocument.Parse(content);

        var item = (TDocument)Activator.CreateInstance(typeof(TDocument))!;
        if (item is DFeDocument<TDocument> doc && xmlDoc.Root != null)
        {
            doc.ReadXml(xmlDoc.Root, options);
            doc.Xml = content;
        }
        return item;
    }

    /// <summary>
    /// Serializa o documento e retorna sua representação em formato string XML.
    /// </summary>
    /// <param name="options">Opções de formatação e salvamento do XML.</param>
    /// <param name="encoding">Codificação de caracteres (padrão UTF-8).</param>
    /// <returns>A string contendo o XML gerado.</returns>
    public virtual string GetXml(DFeSaveOptions options = DFeSaveOptions.DisableFormatting, Encoding? encoding = null)
    {
        using var stream = new MemoryStream();
        Save(stream, options, encoding);
        stream.Position = 0;
        using var streamReader = new StreamReader(stream, encoding ?? Encoding.UTF8);
        return streamReader.ReadToEnd();
    }

    /// <summary>
    /// Serializa e grava o documento XML em um arquivo no caminho informado.
    /// </summary>
    /// <param name="path">Caminho completo do arquivo onde o XML será gravado.</param>
    /// <param name="options">Opções de formatação e salvamento do XML.</param>
    /// <param name="encoding">Codificação de caracteres (padrão UTF-8).</param>
    public virtual void Save(string path, DFeSaveOptions options = DFeSaveOptions.DisableFormatting, Encoding? encoding = null)
    {
        var serOptions = ConfigureOptions(options, encoding);
        var element = WriteToXml(null, null, serOptions);
        var xmlDoc = new XDocument(new XDeclaration("1.0", serOptions.Encoding.WebName, null), element);

        var settings = new XmlWriterSettings
        {
            Encoding = serOptions.Encoding,
            Indent = serOptions.FormatarXml,
            OmitXmlDeclaration = serOptions.OmitirDeclaracao
        };

        using (var writer = XmlWriter.Create(path, settings))
        {
            xmlDoc.Save(writer);
        }
        Xml = File.ReadAllText(path, serOptions.Encoding);
    }

    /// <summary>
    /// Serializa e grava o documento XML no <see cref="Stream"/> informado.
    /// </summary>
    /// <param name="stream">Stream de destino para gravação do XML.</param>
    /// <param name="options">Opções de formatação e salvamento do XML.</param>
    /// <param name="encoding">Codificação de caracteres (padrão UTF-8).</param>
    public virtual void Save(Stream stream, DFeSaveOptions options = DFeSaveOptions.DisableFormatting, Encoding? encoding = null)
    {
        var serOptions = ConfigureOptions(options, encoding);
        var element = WriteToXml(null, null, serOptions);
        var xmlDoc = new XDocument(new XDeclaration("1.0", serOptions.Encoding.WebName, null), element);

        var settings = new XmlWriterSettings
        {
            Encoding = serOptions.Encoding,
            Indent = serOptions.FormatarXml,
            OmitXmlDeclaration = serOptions.OmitirDeclaracao
        };

        using (var writer = XmlWriter.Create(stream, settings))
        {
            xmlDoc.Save(writer);
        }

        using var ms = new MemoryStream();
        stream.Position = 0;
        stream.CopyTo(ms);
        ms.Position = 0;
        using var reader = new StreamReader(ms, serOptions.Encoding);
        Xml = reader.ReadToEnd();
    }

    private static SerializerOptions ConfigureOptions(DFeSaveOptions options, Encoding? encoding)
    {
        var serOptions = new SerializerOptions();
        if (!options.HasFlag(DFeSaveOptions.None))
        {
            serOptions.RemoverAcentos = options.HasFlag(DFeSaveOptions.RemoveAccents);
            serOptions.RemoverEspacos = options.HasFlag(DFeSaveOptions.RemoveSpaces);
            serOptions.FormatarXml = !options.HasFlag(DFeSaveOptions.DisableFormatting);
            serOptions.OmitirDeclaracao = options.HasFlag(DFeSaveOptions.OmitDeclaration);
        }

        if (encoding != null)
        {
            serOptions.Encoding = encoding;
        }

        return serOptions;
    }

    #endregion Methods
}