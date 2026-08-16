// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 01-31-2016
//
// Last Modified By : RFTD
// Last Modified On : 06-07-2016
// ***********************************************************************
// <copyright file="DFeArquivosConfigBase.cs" company="OpenAC .Net">
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
using System.IO;
using System.Linq;
using System.Reflection;
using OpenAC.Net.Core.Extensions;

namespace OpenAC.Net.DFe.Core.Common;

/// <summary>
/// Classe base abstrata para configurações de arquivos e schemas com tipo genérico de enumeração de schemas.
/// </summary>
/// <typeparam name="TSchemas">O tipo enum com os schemas disponíveis para o documento fiscal.</typeparam>
public abstract class DFeArquivosConfigBase<TSchemas> : DFeArquivosConfigBase where TSchemas : Enum
{
    #region Methods

    /// <summary>
    /// Retorna o caminho completo do arquivo de schema (.xsd) correspondente ao tipo solicitado.
    /// </summary>
    /// <param name="schema">O schema do documento fiscal desejado.</param>
    /// <returns>O caminho completo para o arquivo XSD do schema.</returns>
    public abstract string GetSchema(TSchemas schema);

    #endregion Methods
}

/// <summary>
/// Classe base abstrata para configurações de salvamento de arquivos XML de documentos fiscais eletrônicos.
/// </summary>
public abstract class DFeArquivosConfigBase
{
    #region Fields

    private string arquivoServicos;

    #endregion Fields

    #region Constructors

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeArquivosConfigBase"/>.
    /// </summary>
    protected DFeArquivosConfigBase()
    {
        var path = Path.GetDirectoryName((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location) ?? string.Empty;
        PathSalvar = Path.Combine(path, "Docs");
        PathSchemas = Path.Combine(path, "Schemas");
        arquivoServicos = string.Empty;

        Salvar = true;
        AdicionarLiteral = false;
        SepararPorCNPJ = false;
        SepararPorModelo = false;
        SepararPorAno = false;
        SepararPorMes = false;
        SepararPorDia = false;

        OrdenacaoPath = new List<TagOrdenacaoPath>();
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Obtém ou define o caminho do diretório padrão onde devem ser salvos os arquivos XML.
    /// </summary>
    public string PathSalvar { get; set; }

    /// <summary>
    /// Obtém ou define o caminho do diretório onde estão localizados os arquivos de schemas XSD.
    /// </summary>
    public string PathSchemas { get; set; }

    /// <summary>
    /// Obtém ou define o caminho do arquivo XML/INI com a tabela de URLs dos Web Services.
    /// </summary>
    public string ArquivoServicos
    {
        get => arquivoServicos;
        set
        {
            if (value == arquivoServicos) return;

            arquivoServicos = value;
            ArquivoServicoChange();
        }
    }

    /// <summary>
    /// Obtém ou define se deve salvar automaticamente os arquivos XML gerados (documentos com validade jurídica).
    /// </summary>
    public bool Salvar { get; set; }

    /// <summary>
    /// Obtém ou define se deve adicionar o texto literal da pasta ao caminho de salvamento.
    /// </summary>
    public bool AdicionarLiteral { get; set; }

    /// <summary>
    /// Obtém ou define se deve criar subpastas separadas pelo CNPJ do emitente.
    /// </summary>
    public bool SepararPorCNPJ { get; set; }

    /// <summary>
    /// Obtém ou define se deve criar subpastas separadas pelo modelo do documento fiscal.
    /// </summary>
    public bool SepararPorModelo { get; set; }

    /// <summary>
    /// Obtém ou define se deve criar subpastas separadas pelo ano de emissão (ex: 2026).
    /// </summary>
    public bool SepararPorAno { get; set; }

    /// <summary>
    /// Obtém ou define se deve criar subpastas separadas pelo mês de emissão (ex: 08).
    /// </summary>
    public bool SepararPorMes { get; set; }

    /// <summary>
    /// Obtém ou define se deve criar subpastas separadas pelo dia de emissão (ex: 16).
    /// </summary>
    public bool SepararPorDia { get; set; }

    /// <summary>
    /// Obtém a lista com a ordem de criação dos subdiretórios para organização dos arquivos salvos.
    /// </summary>
    public List<TagOrdenacaoPath> OrdenacaoPath { get; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Método invocado quando o caminho do arquivo de serviços (<see cref="ArquivoServicos"/>) é alterado.
    /// </summary>
    protected abstract void ArquivoServicoChange();

    /// <summary>
    /// Constrói e retorna o caminho completo da pasta para salvamento do arquivo, criando os diretórios se necessário.
    /// </summary>
    /// <param name="aPath">Diretório base de salvamento (se vazio, usa <see cref="PathSalvar"/>).</param>
    /// <param name="aLiteral">Nome do subdiretório literal (ex: "NFe", "Cancelamento").</param>
    /// <param name="cnpj">CNPJ do emitente.</param>
    /// <param name="data">Data de referência para divisão por ano/mês/dia.</param>
    /// <param name="modeloDescr">Descrição do modelo do documento fiscal.</param>
    /// <returns>O caminho completo do diretório pronto para receber o arquivo.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Disparado se uma regra de ordenação desconhecida for informada.</exception>
    protected virtual string GetPath(string aPath, string aLiteral, string cnpj = "", DateTime? data = null, string modeloDescr = "")
    {
        var dir = aPath.IsEmpty() ? PathSalvar : aPath;

        if (!OrdenacaoPath.Any())
        {
            if (SepararPorCNPJ) OrdenacaoPath.Add(TagOrdenacaoPath.CNPJ);
            if (SepararPorModelo) OrdenacaoPath.Add(TagOrdenacaoPath.Modelo);
            if (SepararPorAno || SepararPorMes || SepararPorDia) OrdenacaoPath.Add(TagOrdenacaoPath.Data);
            if (AdicionarLiteral) OrdenacaoPath.Add(TagOrdenacaoPath.Literal);
        }

        foreach (var ordenacaoPath in OrdenacaoPath)
        {
            {
                switch (ordenacaoPath)
                {
                    case TagOrdenacaoPath.CNPJ:
                        if (cnpj.IsEmpty()) continue;

                        dir = Path.Combine(dir, cnpj.OnlyNumbers());
                        break;

                    case TagOrdenacaoPath.Modelo:
                        if (modeloDescr.IsEmpty()) continue;

                        dir = Path.Combine(dir, modeloDescr);
                        break;

                    case TagOrdenacaoPath.Data:
                        data ??= DateTime.Now;

                        if (SepararPorAno)
                            dir = Path.Combine(dir, data.Value.ToString("yyyy"));

                        if (SepararPorMes)
                            dir = Path.Combine(dir, data.Value.ToString("MM"));

                        if (SepararPorDia)
                            dir = Path.Combine(dir, data.Value.ToString("dd"));
                        break;

                    case TagOrdenacaoPath.Literal:
                        if (aLiteral.IsEmpty()) continue;

                        if (!dir.ToLower().Contains(aLiteral.ToLower()))
                            dir = Path.Combine(dir, aLiteral);
                        break;

                    case TagOrdenacaoPath.Nenhum:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        return dir;
    }

    #endregion Methods
}