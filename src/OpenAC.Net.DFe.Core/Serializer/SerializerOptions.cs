// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 05-04-2016
//
// Last Modified By : RFTD
// Last Modified On : 05-04-2016
// ***********************************************************************
// <copyright file="SerializerOptions.cs" company="OpenAC .Net">
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

using System.Collections.Generic;
using System.Text;

namespace OpenAC.Net.DFe.Core.Serializer;

/// <summary>
/// Opções e parâmetros de configuração utilizados pelo serializador XML DFe durante a geração e leitura de documentos.
/// </summary>
public class SerializerOptions
{
    #region Constructors

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="SerializerOptions"/> com configurações padrão.
    /// </summary>
    public SerializerOptions()
    {
        ErrosAlertas = new List<string>();
        FormatoAlerta = "TAG:%TAG% ID:%ID%/%TAG%(%DESCRICAO%) - %MSG%.";
        RemoverAcentos = false;
        RemoverEspacos = false;
        FormatarXml = true;
        OmitirDeclaracao = false;
        Encoding = Encoding.UTF8;
    }

    #endregion Constructors

    #region Propriedades

    /// <summary>
    /// Obtém ou define se deve remover caracteres acentuados durante a serialização XML.
    /// </summary>
    public bool RemoverAcentos { get; set; }

    /// <summary>
    /// Obtém ou define se deve remover espaços redundantes do conteúdo textual dos elementos XML.
    /// </summary>
    public bool RemoverEspacos { get; set; }

    /// <summary>
    /// Obtém ou define se o XML gerado deve ser indentado e formatado.
    /// </summary>
    public bool FormatarXml { get; set; }

    /// <summary>
    /// Obtém ou define se a declaração XML inicial (<c>&lt;?xml version="1.0" ... ?&gt;</c>) deve ser omitida.
    /// </summary>
    public bool OmitirDeclaracao { get; set; }

    /// <summary>
    /// Obtém ou define a codificação de caracteres utilizada na leitura e escrita do XML (padrão UTF-8).
    /// </summary>
    public Encoding Encoding { get; set; }

    /// <summary>
    /// Obtém a lista de mensagens de erros e alertas de validação e serialização coletados.
    /// </summary>
    public List<string> ErrosAlertas { get; }

    /// <summary>
    /// Obtém ou define o padrão de formatação das mensagens de alerta (utiliza marcadores como %TAG%, %ID%, %DESCRICAO%, %MSG%).
    /// </summary>
    public string FormatoAlerta { get; set; }

    #endregion Propriedades

    #region Methods

    /// <summary>
    /// Formata e adiciona uma mensagem de alerta na lista <see cref="ErrosAlertas"/> com base no <see cref="FormatoAlerta"/>.
    /// </summary>
    /// <param name="id">Identificador do campo no manual (ex: B01).</param>
    /// <param name="tag">Nome da tag XML associada.</param>
    /// <param name="descricao">Descrição legível do campo.</param>
    /// <param name="alerta">Texto descritivo do alerta ou inconsistência encontrada.</param>
    public void AddAlerta(string id, string tag, string descricao, string alerta)
    {
        var s = FormatoAlerta.Clone() as string;
        if (s == null)
            return;

        s = s.Replace("%ID%", id).Replace("%TAG%", $"<{tag}>").Replace("%DESCRICAO%", descricao).Replace("%MSG%", alerta);

        ErrosAlertas.Add(s);
    }

    #endregion Methods
}