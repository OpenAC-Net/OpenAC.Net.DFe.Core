// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 01-31-2016
//
// Last Modified By : RFTD
// Last Modified On : 06-07-2016
// ***********************************************************************
// <copyright file="DFeCertificadosConfigBase.cs" company="OpenAC .Net">
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

using OpenAC.Net.Core.Extensions;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace OpenAC.Net.DFe.Core.Common;

/// <summary>
/// Classe base abstrata para configurações de certificados digitais X509 utilizados para autenticação e assinatura de documentos fiscais.
/// </summary>
public abstract class DFeCertificadosConfigBase
{
    #region Fields

    private DateTime dataVenc;
    private string subjectName;
    private string cnpj;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeCertificadosConfigBase"/>.
    /// </summary>
    protected DFeCertificadosConfigBase()
    {
        dataVenc = DateTime.MinValue;
        Certificado = string.Empty;
        subjectName = string.Empty;
        cnpj = string.Empty;
    }

    #endregion Constructor

    #region Properties

    /// <summary>
    /// Obtém ou define o caminho do arquivo de certificado (.pfx/.p12) ou o número de série do certificado instalado no Windows Store.
    /// </summary>
    public string Certificado { get; set; }

    /// <summary>
    /// Obtém ou define os bytes do arquivo de certificado digital em memória.
    /// </summary>
    public byte[] CertificadoBytes { get; set; }

    /// <summary>
    /// Obtém ou define a senha do certificado digital ou PIN do cartão/token A3.
    /// </summary>
    public string Senha { get; set; }

    /// <summary>
    /// Obtém a data de expiração/vencimento do certificado digital configurado.
    /// </summary>
    public DateTime DataVenc
    {
        get
        {
            if (dataVenc == DateTime.MinValue && (!Certificado.IsEmpty() || !CertificadoBytes.IsNullOrEmpty()))
                GetCertificado();

            return dataVenc;
        }
    }

    /// <summary>
    /// Obtém o nome/razão social do titular (Subject Name) do certificado digital configurado.
    /// </summary>
    public string Nome
    {
        get
        {
            if (subjectName.IsEmpty() && (!Certificado.IsEmpty() || !CertificadoBytes.IsNullOrEmpty()))
                GetCertificado();

            return subjectName;
        }
    }

    /// <summary>
    /// Obtém o CNPJ ou CPF extraído do certificado digital configurado.
    /// </summary>
    public string CNPJ
    {
        get
        {
            if (cnpj.IsEmpty() && (!Certificado.IsEmpty() || !CertificadoBytes.IsNullOrEmpty()))
                GetCertificado();

            return cnpj;
        }
    }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Abre a caixa de diálogo do Windows para seleção de um certificado instalado e retorna seu número de série.
    /// </summary>
    /// <returns>O número de série do certificado selecionado, ou string vazia se cancelado.</returns>
    public string SelecionarCertificado()
    {
        var cert = CertificadoDigital.SelecionarCertificado(string.Empty);
        return cert?.GetSerialNumberString() ?? string.Empty;
    }

    /// <summary>
    /// Carrega e retorna a instância de <see cref="X509Certificate2"/> de acordo com as configurações informadas (bytes, arquivo ou repositório do Windows).
    /// </summary>
    /// <returns>A instância de <see cref="X509Certificate2"/> carregada.</returns>
    public X509Certificate2 ObterCertificado()
    {
        if (CertificadoBytes?.Length > 0)
        {
            return CertificadoDigital.SelecionarCertificado(CertificadoBytes, Senha);
        }

        if (File.Exists(Certificado))
        {
            return CertificadoDigital.SelecionarCertificado(Certificado, Senha);
        }

        var ret = CertificadoDigital.SelecionarCertificado(Certificado);

#if NETFRAMEWORK
        if (!Senha.IsEmpty())
        {
            ret.SetPin(Senha);
        }
#endif

        return ret;
    }

    /// <summary>
    /// Carrega as informações e metadados do certificado digital (data de vencimento, titular e CNPJ).
    /// </summary>
    protected void GetCertificado()
    {
        var cert = ObterCertificado();

        try
        {
            dataVenc = cert.GetExpirationDateString().ToData();
            subjectName = cert.SubjectName.Name;
            cnpj = cert.GetCNPJ();
        }
        finally
        {
#if NET || NETSTANDARD
            cert?.Reset();
#else
            try
            {
                if (cert != null && cert.IsA3())
                {
#if NETFRAMEWORK
                    cert.ForceUnload();
#else
                    cert.Dispose();
#endif
                }
                else
                {
                    cert?.Reset();
                }
            }
            catch (Exception)
            {
                //
            }
#endif
        }
    }

    #endregion Methods
}