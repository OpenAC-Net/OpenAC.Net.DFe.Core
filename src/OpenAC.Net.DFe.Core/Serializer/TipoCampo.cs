// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 03-27-2016
//
// Last Modified By : RFTD
// Last Modified On : 05-08-2016
// ***********************************************************************
// <copyright file="TipoCampo.cs" company="OpenAC .Net">
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

namespace OpenAC.Net.DFe.Core.Serializer;

/// <summary>
/// Especifica o formato e as regras de conversão de tipos de dados nos campos de documentos DFe.
/// </summary>
public enum TipoCampo
{
    /// <summary>
    /// Campo do tipo texto simples (string).
    /// </summary>
    Str = 0,

    /// <summary>
    /// Campo numérico inteiro de 32 bits (int).
    /// </summary>
    Int = 1,

    /// <summary>
    /// Campo numérico inteiro de 64 bits (long).
    /// </summary>
    Long = 17,

    /// <summary>
    /// Campo de data no formato AAAA-MM-DD (DateTime).
    /// </summary>
    Dat = 2,

    /// <summary>
    /// Campo de data e hora no formato AAAA-MM-DDTHH:MM:SS (DateTime).
    /// </summary>
    DatHor = 3,

    /// <summary>
    /// Campo de data e hora com fuso horário / timezone no formato AAAA-MM-DDTHH:MM:SSzzz (DateTimeOffset/DateTime).
    /// </summary>
    DatHorTz = 4,

    /// <summary>
    /// Campo numérico textual com dígitos decimais sem separador de pontuação.
    /// </summary>
    StrNumber = 5,

    /// <summary>
    /// Campo numérico textual preenchido com zeros à esquerda até o tamanho especificado.
    /// </summary>
    StrNumberFill = 6,

    /// <summary>
    /// Campo numérico decimal formatado com exatamente 2 casas decimais (0.00).
    /// </summary>
    De2 = 7,

    /// <summary>
    /// Campo numérico decimal formatado com até 3 casas decimais (0.000).
    /// </summary>
    De3 = 8,

    /// <summary>
    /// Campo numérico decimal formatado com até 4 casas decimais (0.0000).
    /// </summary>
    De4 = 9,

    /// <summary>
    /// Campo numérico decimal formatado com até 10 casas decimais (0.0000000000).
    /// </summary>
    De10 = 10,

    /// <summary>
    /// Campo de hora no formato HH:MM:SS.
    /// </summary>
    Hor = 11,

    /// <summary>
    /// Campo numérico decimal formatado com até 6 casas decimais (0.000000).
    /// </summary>
    De6 = 12,

    /// <summary>
    /// Campo de data no formato do CF-e-SAT (AAAAMMDD).
    /// </summary>
    DatCFe = 13,

    /// <summary>
    /// Campo de hora no formato do CF-e-SAT (HHMMSS).
    /// </summary>
    HorCFe = 14,

    /// <summary>
    /// Campo do tipo enumeração mapeado via <see cref="Attributes.DFeEnumAttribute"/>.
    /// </summary>
    Enum = 15,

    /// <summary>
    /// Campo com tipo customizado com serialização própria.
    /// </summary>
    Custom = 16
}