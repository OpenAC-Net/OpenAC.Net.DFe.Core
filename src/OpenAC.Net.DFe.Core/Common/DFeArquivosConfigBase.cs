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
//	     		    Copyright (c) 2016 Grupo OpenAC.Net
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

namespace OpenAC.Net.DFe.Core.Common
{
    public abstract class DFeArquivosConfigBase<TSchemas> : DFeArquivosConfigBase where TSchemas : Enum
    {
        #region Methods

        /// <summary>
        /// Metodo que retorna o caminho para o tipo de schema solicitado.
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public abstract string GetSchema(TSchemas schema);

        #endregion Methods
    }

    public abstract class DFeArquivosConfigBase
    {
        #region Fields

        private string pathSalvar;
        private string arquivoServicos;
        private string pathSchemas;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inicializa uma nova instancia da classe <see cref="DFeArquivosConfigBase{TSchemas}"/>.
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
        /// Define/retorna o caminho onde deve ser salvo os arquivos.
        /// </summary>
        public string PathSalvar { get; set; }

        /// <summary>
        /// Define/retorna o caminho onde estão so schemas.
        /// </summary>
        public string PathSchemas { get; set; }

        /// <summary>
        /// Define/retorna o arquivo com os dados dos serviços.
        /// </summary>
        public string ArquivoServicos
        {
            get => arquivoServicos;
            set
            {
                if (value == arquivoServicos) return;

                arquivoServicos = value ?? string.Empty;
                ArquivoServicoChange();
            }
        }

        /// <summary>
        /// Define/retorna se deve salvar os arquivos xml, trata-se de arquivos com validade jurídica.
        /// </summary>
        public bool Salvar { get; set; }

        /// <summary>
        /// Define/retorna se deve ser adicionado um literal ao caminho de salvamento.
        /// </summary>
        public bool AdicionarLiteral { get; set; }

        /// <summary>
        /// Define/retorna se deve ser adicionado o CNPJ ao caminho de salvamento.
        /// </summary>
        public bool SepararPorCNPJ { get; set; }

        /// <summary>
        /// Define/retorna se deve ser adicionado o numero do
        /// modelo do arquivo DFe ao caminho de salvamento.
        /// </summary>
        public bool SepararPorModelo { get; set; }

        /// <summary>
        /// Define/retorna se deve ser adicionado o ano ao caminho de salvamento.
        /// </summary>
        public bool SepararPorAno { get; set; }

        /// <summary>
        /// Define/retorna se deve ser adicionado o mês ao caminho de salvamento.
        /// </summary>
        public bool SepararPorMes { get; set; }

        /// <summary>
        /// Define/retorna se deve ser adicionado o dia ao caminho de salvamento.
        /// </summary>
        public bool SepararPorDia { get; set; }

        /// <summary>
        /// Retorna a ordem de criação dos caminhos para salvamento dos arquivos.
        /// </summary>
        public List<TagOrdenacaoPath> OrdenacaoPath { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Metodo chamado quando muda o caminho do arquivo de serviços.
        /// </summary>
        protected abstract void ArquivoServicoChange();

        /// <summary>
        /// Gera um path de salvamento.
        /// </summary>
        /// <param name="aPath"></param>
        /// <param name="aLiteral"></param>
        /// <param name="cnpj"></param>
        /// <param name="data"></param>
        /// <param name="modeloDescr"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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
                            if (!data.HasValue) data = DateTime.Now;

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
}