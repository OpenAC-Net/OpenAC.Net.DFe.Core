// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 08-11-2020
//
// Last Modified By : RFTD
// Last Modified On : 08-11-2020
// ***********************************************************************
// <copyright file="StreamSerializer.cs" company="OpenAC .Net">
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
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core.Extensions;

namespace OpenAC.Net.DFe.Core.Serializer;

internal static class StreamSerializer
{
    public static XObject Serialize(DFeBaseAttribute tag, object item, PropertyInfo prop, SerializerOptions options)
    {
        var value = prop.GetValueOrIndex(item) as Stream;
        var estaVazio = value.IsNullOrEmpty();
        var dados = estaVazio ? "" : value.ToBase64();

        return PrimitiveSerializer.ProcessContent(tag, dados, estaVazio, options);
    }

    public static object Deserialize(XElement parentElement)
    {
        if (parentElement == null) return null;

        var dados = parentElement.GetValue<string>();
        return new MemoryStream(Convert.FromBase64String(dados));
    }
}