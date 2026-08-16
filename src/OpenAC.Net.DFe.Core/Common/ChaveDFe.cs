// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 03-09-2018
//
// Last Modified By : RFTD
// Last Modified On : 03-09-2018
// ***********************************************************************
// <copyright file="ChaveDFe.cs" company="OpenAC .Net">
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
using System.Text;
using System.Text.RegularExpressions;
using OpenAC.Net.Core;
using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core.Extensions;

namespace OpenAC.Net.DFe.Core.Common;

/// <summary>
/// Representa a Chave de Acesso de 44 dígitos de um Documento Fiscal Eletrônico (DF-e), com métodos para geração, cálculo de dígito verificador, validação e formatação.
/// </summary>
public sealed class ChaveDFe
{
    #region Constructors

    internal ChaveDFe(DFeCodUF ufEmitente, DateTime dataEmissao, string cnpjEmitente, int modelo, int serie,
        long numero, DFeTipoEmissao tipoEmissao, int cNumerico)
    {
        var chave = new StringBuilder();

        chave.Append(ufEmitente.GetDFeValue())
            .Append(dataEmissao.ToString("yyMM"))
            .Append(cnpjEmitente)
            .Append(modelo.ToString("D2"))
            .Append(serie.ToString("D3"))
            .Append(numero.ToString("D9"))
            .Append(tipoEmissao.GetDFeValue())
            .Append(cNumerico.ToString("D8"));

        var calcDigito = new CalcDigito
        {
            FormulaDigito = CalcDigFormula.Modulo11,
            Documento = chave.ToString(),
            MultiplicadorInicial = 2,
            MultiplicadorFinal = 9
        };

        calcDigito.Calcular();

        chave.Append(calcDigito.DigitoFinal);

        Chave = chave.ToString();
        Digito = calcDigito.DigitoFinal;
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Obtém a chave de acesso completa de 44 dígitos com o dígito verificador.
    /// </summary>
    public string Chave { get; }

    /// <summary>
    /// Obtém o dígito verificador (DV) calculado da chave de acesso.
    /// </summary>
    public int Digito { get; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Gera a chave de acesso de 44 dígitos do documento fiscal eletrônico com base nos parâmetros informados.
    /// </summary>
    /// <param name="ufEmitente">Código IBGE da UF do emitente do DF-e.</param>
    /// <param name="dataEmissao">Data de emissão do DF-e (ano e mês são utilizados).</param>
    /// <param name="cnpjEmitente">CNPJ ou CPF do emitente (somente números).</param>
    /// <param name="modelo">Modelo do DF-e (ex: 55 para NF-e, 65 para NFC-e, 57 para CT-e).</param>
    /// <param name="serie">Série do DF-e.</param>
    /// <param name="numero">Número do DF-e.</param>
    /// <param name="tipoEmissao">Tipo de emissão do DF-e (Normal, Contingência, etc.).</param>
    /// <param name="cNumerico">Código numérico aleatório de 8 dígitos gerado pelo emitente.</param>
    /// <returns>Uma nova instância de <see cref="ChaveDFe"/> contendo a chave gerada e seu dígito verificador.</returns>
    public static ChaveDFe Gerar(DFeCodUF ufEmitente, DateTime dataEmissao, string cnpjEmitente, int modelo, int serie,
        long numero, DFeTipoEmissao tipoEmissao, int cNumerico)
    {
        return new ChaveDFe(ufEmitente, dataEmissao, cnpjEmitente, modelo, serie, numero, tipoEmissao, cNumerico);
    }

    /// <summary>
    /// Valida se uma chave de acesso de DF-e é válida, verificando seu tamanho (44 dígitos) e o dígito verificador (Módulo 11).
    /// </summary>
    /// <param name="chave">A chave de acesso com 44 dígitos numéricos.</param>
    /// <returns><c>true</c> se a chave for válida; caso contrário, <c>false</c>.</returns>
    public static bool Validar(string chave)
    {
        if (chave.IsEmpty()) return false;

        chave = chave.Trim();
        if (chave.Trim().Length != 44) return false;

        var digitoVerificador = chave.Substring(43, 1).ToInt32();

        var calcDigito = new CalcDigito
        {
            Documento = chave.Substring(0, 43)
        };

        calcDigito.CalculoPadrao();
        calcDigito.Calcular();

        return digitoVerificador == calcDigito.DigitoFinal;
    }

    /// <summary>
    /// Formata a chave de acesso inserindo um espaço a cada 4 dígitos (ex: "3516 0400 ...").
    /// </summary>
    /// <param name="chave">A chave de 44 dígitos a ser formatada.</param>
    /// <returns>A chave de acesso formatada em blocos de 4 dígitos.</returns>
    public static string Formatar(string chave)
    {
        return Regex.Replace(chave, ".{4}", "$0 ");
    }

    #endregion Methods
}