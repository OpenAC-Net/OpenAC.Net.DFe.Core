// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 04-01-2019
//
// Last Modified By : RFTD
// Last Modified On : 04-01-2019
// ***********************************************************************
// <copyright file="DFeServiceInfo.cs" company="OpenAC .Net">
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
using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Collection;
using OpenAC.Net.DFe.Core.Common;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Service;

/// <summary>
/// Representa as informações de configuração de serviços Web Services para um determinado tipo de documento fiscal e forma de emissão.
/// </summary>
/// <typeparam name="TTIpo">Enum com os tipos de serviço suportados.</typeparam>
public partial class DFeServiceInfo<TTIpo> where TTIpo : Enum
{
    #region Properties

    /// <summary>
    /// Indexador que busca e retorna o ambiente de serviço correspondente ao par (Ambiente, UF) especificado.
    /// </summary>
    /// <param name="ambiente">Ambiente desejado (Produção ou Homologação).</param>
    /// <param name="uf">Sigla da UF de destino.</param>
    /// <returns>O <see cref="DFeServiceEnvironment{TTIpo}"/> correspondente, ou <c>null</c> se não encontrado.</returns>
    [DFeIgnore]
    public DFeServiceEnvironment<TTIpo> this[DFeTipoAmbiente ambiente, DFeSiglaUF uf] =>
        Ambientes?.SingleOrDefault(x => x.Ambiente == ambiente && x.UF == uf);

    /// <summary>
    /// Obtém ou define o tipo de serviço fiscal (ex: NFe, NFCe, CTe, MDFe, NFSe).
    /// </summary>
    [DFeAttribute(TipoCampo.Enum, "Tipo")] 
    public DFeTipoServico Tipo { get; set; }

    /// <summary>
    /// Obtém ou define a forma de emissão associada ao conjunto de serviços (Normal, SVC, etc.).
    /// </summary>
    [DFeAttribute(TipoCampo.Enum, "TipoEmissao")]
    public DFeTipoEmissao TipoEmissao { get; set; } = DFeTipoEmissao.Normal;
    
    /// <summary>
    /// Obtém ou define a coleção de ambientes e UFs configurados para este serviço.
    /// </summary>
    [DFeCollection("Ambiente")]
    public DFeCollection<DFeServiceEnvironment<TTIpo>> Ambientes { get; set; } = new();

    #endregion Properties
}