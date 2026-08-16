// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 05-07-2016
//
// Last Modified By : RFTD
// Last Modified On : 05-07-2016
// ***********************************************************************
// <copyright file="XDocumentExtensions.cs" company="OpenAC .Net">
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
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core.Attributes;

namespace OpenAC.Net.DFe.Core.Extensions;

/// <summary>
/// Métodos de extensão internos para manipulação de nós LINQ to XML (<see cref="XElement"/>, <see cref="XObject"/>).
/// </summary>
internal static class XDocumentExtensions
{
    /// <summary>
    /// Determina o tipo CLR associado a um elemento XML com base no atributo 'Type' ou nos argumentos genéricos da classe pai.
    /// </summary>
    /// <param name="element">O elemento XML analisado.</param>
    /// <param name="parentType">O tipo CLR da classe pai que contém a propriedade genérica.</param>
    /// <param name="genericArgumentIndex">Índice do argumento de tipo genérico.</param>
    /// <returns>O tipo CLR resolvido para o elemento.</returns>
    public static Type GetElementType(this XElement element, Type parentType, int genericArgumentIndex)
    {
        Type type = null;
        var typeELement = element.Attribute("Type");
        if (typeELement != null)
            type = Type.GetType(typeELement.Value);

        if (type != null) return type;

        var arguments = parentType.GetGenericArguments();
        if (arguments.Length > genericArgumentIndex)
            type = arguments[genericArgumentIndex];

        return type;
    }

    /// <summary>
    /// Localiza todos os elementos filhos correspondentes ao mapeamento de uma propriedade (<see cref="DFeBaseAttribute"/> ou <see cref="DFeItemAttribute"/>).
    /// </summary>
    /// <param name="element">O elemento XML pai.</param>
    /// <param name="prop">A informação de reflexão da propriedade mapeada.</param>
    /// <returns>Array de elementos XML encontrados.</returns>
    public static XElement[] GetElements(this XElement element, PropertyInfo prop)
    {
        var listElement = new List<XElement>();

        var tag = prop.GetAttribute<DFeBaseAttribute>();

        var itemElement = element.ElementsAnyNs(tag.Name);
        if (!itemElement.IsNullOrEmpty())
            listElement.AddRange(itemElement);

        foreach (var att in prop.GetAttributes<DFeItemAttribute>())
        {
            itemElement = element.ElementsAnyNs(att.Name);
            if (!itemElement.IsNullOrEmpty())
                listElement.AddRange(itemElement);
        }

        return listElement.ToArray();
    }

    /// <summary>
    /// Adiciona múltiplos objetos XML (<see cref="XElement"/> ou <see cref="XAttribute"/>) como filhos deste elemento.
    /// </summary>
    /// <param name="element">O elemento XML de destino.</param>
    /// <param name="childs">Os elementos ou atributos XML a serem adicionados.</param>
    public static void AddChilds(this XElement element, params XObject[] childs)
    {
        foreach (var child in childs)
        {
            if (child is XElement childElement)
                element.AddChild(childElement);
            else
                element.AddAttribute((XAttribute)child);
        }
    }
}