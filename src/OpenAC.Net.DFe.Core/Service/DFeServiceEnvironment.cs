// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 04-01-2019
//
// Last Modified By : RFTD
// Last Modified On : 04-01-2019
// ***********************************************************************
// <copyright file="DFeServiceEnvironment.cs" company="OpenAC .Net">
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
using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Common;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Service;

/// <summary>
/// Representa as configurações de ambiente (Produção ou Homologação) e UF contendo os endpoints dos Web Services DFe.
/// </summary>
/// <typeparam name="TTIpo">Enum com os tipos de serviço suportados (ex: Autorizacao, RetAutorizacao, etc.).</typeparam>
public partial class DFeServiceEnvironment<TTIpo> where TTIpo : Enum
{
    #region Properties

    /// <summary>
    /// Indexador que obtém ou define a URL do endpoint para o tipo de serviço especificado.
    /// </summary>
    /// <param name="tipo">O tipo de serviço desejado.</param>
    /// <returns>A URL configurada para o serviço.</returns>
    [DFeIgnore]
    public string this[TTIpo tipo]
    {
        get => Enderecos[tipo];
        set => Enderecos[tipo] = value;
    }

    /// <summary>
    /// Obtém ou define o ambiente do serviço (Produção ou Homologação).
    /// </summary>
    [DFeAttribute(TipoCampo.Enum, "Tipo")]
    public DFeTipoAmbiente Ambiente { get; set; }

    /// <summary>
    /// Obtém ou define a sigla da UF associada ao ambiente de serviço.
    /// </summary>
    [DFeAttribute(TipoCampo.Enum, "UF")]
    public DFeSiglaUF UF { get; set; }

    /// <summary>
    /// Obtém ou define o dicionário de endereços/URLs mapeados por tipo de serviço.
    /// </summary>
    [DFeDictionary("Enderecos")]
    [DFeDictionaryKey(TipoCampo.Enum, "Tipo", AsAttribute = true)]
    [DFeDictionaryValue(TipoCampo.Str, "Endereco")]
    public Dictionary<TTIpo, string> Enderecos { get; set; } = new();

    #endregion Properties
}