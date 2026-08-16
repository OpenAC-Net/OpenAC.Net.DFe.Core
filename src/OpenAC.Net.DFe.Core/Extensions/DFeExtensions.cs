// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 05-10-2016
//
// Last Modified By : RFTD
// Last Modified On : 05-11-2016
// ***********************************************************************
// <copyright file="DFeExtensions.cs" company="OpenAC .Net">
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
using System.Xml.Linq;
using OpenAC.Net.Core.Extensions;

namespace OpenAC.Net.DFe.Core.Extensions;

/// <summary>
/// Métodos de extensão auxiliares para manipulação de strings e validação de documentos XML.
/// </summary>
public static class DFeExtensions
{
    /// <summary>
    /// Remove os marcadores de seção <c>&lt;![CDATA[</c> e <c>]]&gt;</c> de uma string, se presentes.
    /// </summary>
    /// <param name="value">A string contendo ou não uma seção CDATA.</param>
    /// <returns>O conteúdo de texto desprovido das tags CDATA.</returns>
    public static string RemoveCData(this string value)
    {
        if (value.IsEmpty()) return value;
        return value.IsCData() ? value.GetStrBetween(9, value.Length - 4) : value;
    }

    /// <summary>
    /// Verifica se a string inicia com <c>&lt;![CDATA[</c> e finaliza com <c>]]&gt;</c>.
    /// </summary>
    /// <param name="value">A string a ser verificada.</param>
    /// <returns><c>true</c> se a string for uma seção CDATA válida; caso contrário, <c>false</c>.</returns>
    public static bool IsCData(this string value)
    {
        if (value.IsEmpty()) return false;

        return value.StartsWith("<![CDATA[") && value.EndsWith("]]>");
    }

    /// <summary>
    /// Verifica se o texto informado representa um XML válido e bem formado.
    /// </summary>
    /// <param name="xmlstring">A string XML a ser verificada.</param>
    /// <returns><c>true</c> se a string puder ser analisada como XML; caso contrário, <c>false</c>.</returns>
    public static bool IsValidXml(this string xmlstring)
    {
        try
        {
            var xDocument = XDocument.Parse(xmlstring);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}