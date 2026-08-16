// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 05-04-2016
//
// Last Modified By : RFTD
// Last Modified On : 08-06-2018
// ***********************************************************************
// <copyright file="DFeBaseAttribute.cs" company="OpenAC .Net">
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
using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core;

/// <summary>
/// Classe base abstrata para atributos de mapeamento XML DFe.
/// </summary>
public abstract class DFeBaseAttribute : Attribute
{
    #region Constructors

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeBaseAttribute"/>.
    /// </summary>
    protected DFeBaseAttribute()
    {
        Tipo = TipoCampo.Str;
        Id = "";
        Name = string.Empty;
        Min = 0;
        Max = 0;
        Ocorrencia = 0;
        Ordem = 0;
        Descricao = string.Empty;
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Obtém ou define o tipo de dado do campo no XML.
    /// </summary>
    public TipoCampo Tipo { get; set; }

    /// <summary>
    /// Obtém ou define o identificador do campo conforme o manual do DFe (ex: B01, H02).
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Obtém ou define o nome da tag XML gerada para o campo.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Obtém ou define o namespace XML do elemento ou atributo.
    /// </summary>
    public string Namespace { get; set; }

    /// <summary>
    /// Obtém ou define a descrição legível do campo para mensagens de erro e validação.
    /// </summary>
    public string Descricao { get; set; }

    /// <summary>
    /// Obtém ou define a ordem de serialização do elemento no XML.
    /// </summary>
    public int Ordem { get; set; }

    /// <summary>
    /// Obtém ou define o tamanho máximo de caracteres ou casas decimais permitidos.
    /// </summary>
    public int Max { get; set; }

    /// <summary>
    /// Obtém ou define o tamanho mínimo de caracteres ou casas decimais obrigatórios.
    /// </summary>
    public int Min { get; set; }

    /// <summary>
    /// Obtém ou define a regra de ocorrência do campo no XML.
    /// </summary>
    public Ocorrencia Ocorrencia { get; set; }

    #endregion Properties
}