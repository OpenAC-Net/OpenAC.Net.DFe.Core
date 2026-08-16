// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 01-31-2016
//
// Last Modified By : RFTD
// Last Modified On : 06-07-2016
// ***********************************************************************
// <copyright file="DFeWebserviceConfigBase.cs" company="OpenAC .Net">
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
using System.Net;
using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core.Extensions;

namespace OpenAC.Net.DFe.Core.Common;

/// <summary>
/// Classe base abstrata para configurações de comunicação HTTP / SOAP com Web Services da SEFAZ.
/// </summary>
public abstract class DFeWebserviceConfigBase
{
    #region Constructor

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeWebserviceConfigBase"/>.
    /// </summary>
    protected DFeWebserviceConfigBase()
    {
        Ambiente = DFeTipoAmbiente.Homologacao;
        AjustaAguardaConsultaRet = false;
        AguardarConsultaRet = 1;
        Tentativas = 3;
        IntervaloTentativas = 1000;

#if NETCORE
        Protocolos = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#else
#pragma warning disable CS0618
        Protocolos = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                     SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#pragma warning restore CS0618
#endif
    }

    #endregion Constructor

    #region Properties

    /// <summary>
    /// Obtém ou define se deve salvar em disco as mensagens SOAP/HTTP de envio e retorno.
    /// </summary>
    public bool Salvar { get; set; }

    /// <summary>
    /// Obtém ou define o ambiente de destino dos Web Services (Produção ou Homologação).
    /// </summary>
    public DFeTipoAmbiente Ambiente { get; set; }

    /// <summary>
    /// Obtém o código numérico do ambiente (1 para Produção, 2 para Homologação).
    /// </summary>
    public int AmbienteCodigo => Ambiente.GetDFeValue().ToInt32();

    /// <summary>
    /// Obtém ou define o número máximo de tentativas de comunicação com o Web Service em caso de falha.
    /// </summary>
    public int Tentativas { get; set; }

    /// <summary>
    /// Obtém ou define o tempo de espera (em milissegundos) entre cada tentativa de comunicação.
    /// </summary>
    public int IntervaloTentativas { get; set; }

    /// <summary>
    /// Obtém ou define se o tempo de timeout da requisição deve ser ajustado automaticamente com base no tempo de espera do recibo.
    /// </summary>
    public bool AjustaAguardaConsultaRet { get; set; }

    /// <summary>
    /// Obtém ou define o tempo de espera (em segundos) antes de consultar o processamento do lote na SEFAZ.
    /// </summary>
    public uint AguardarConsultaRet { get; set; }

    /// <summary>
    /// Obtém ou define os protocolos de segurança SSL/TLS aceitos nas conexões seguras com os servidores da SEFAZ.
    /// </summary>
    public SecurityProtocolType Protocolos { get; set; }

    /// <summary>
    /// Obtém o timeout customizado calculado para a requisição SOAP.
    /// </summary>
    public TimeSpan? TimeOut
    {
        get
        {
            TimeSpan? timeOut = null;
            if (AjustaAguardaConsultaRet)
                timeOut = TimeSpan.FromSeconds((int)AguardarConsultaRet);

            return timeOut;
        }
    }

    #endregion Properties
}