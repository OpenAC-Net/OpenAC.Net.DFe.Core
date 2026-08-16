using System;
using System.Collections.Generic;
using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Generator.Sample;

public enum TipoEmissaoSample
{
    [DFeEnum("1")]
    Normal = 1,

    [DFeEnum("9")]
    ContingenciaOffLine = 9
}

[DFeRoot("infNFe", Namespace = "http://www.portalfiscal.inf.br/nfe")]
public partial class NotaFiscalSample
{
    [DFeAttribute(TipoCampo.Int, "versao", Min = 1, Max = 4)]
    public int Versao { get; set; } = 4;

    [DFeElement(TipoCampo.Str, "Id", Min = 44, Max = 44, Ocorrencia = Ocorrencia.Obrigatoria)]
    public string ChaveAcesso { get; set; } = string.Empty;

    [DFeElement(TipoCampo.DatHor, "dhEmi", Ocorrencia = Ocorrencia.Obrigatoria)]
    public DateTime DataEmissao { get; set; } = DateTime.Now;

    [DFeElement(TipoCampo.Enum, "tpEmis", Ocorrencia = Ocorrencia.Obrigatoria)]
    public TipoEmissaoSample TipoEmissao { get; set; } = TipoEmissaoSample.Normal;

    [DFeElement(TipoCampo.De2, "vNF", Ocorrencia = Ocorrencia.Obrigatoria)]
    public decimal ValorTotal { get; set; }

    [DFeCollection("det")]
    public List<ItemSample> Itens { get; set; } = new();
}

public partial class ItemSample
{
    [DFeAttribute(TipoCampo.Int, "nItem")]
    public int NumeroItem { get; set; }

    [DFeElement(TipoCampo.Str, "cProd", Ocorrencia = Ocorrencia.Obrigatoria)]
    public string CodigoProduto { get; set; } = string.Empty;

    [DFeElement(TipoCampo.Str, "xProd", Ocorrencia = Ocorrencia.Obrigatoria)]
    public string DescricaoProduto { get; set; } = string.Empty;

    [DFeElement(TipoCampo.De4, "vUnCom", Ocorrencia = Ocorrencia.Obrigatoria)]
    public decimal ValorUnitario { get; set; }
}

public static class Program
{
    public static void Main()
    {
        var nfe = new NotaFiscalSample
        {
            Versao = 4,
            ChaveAcesso = "35230800000000000000550010000000011000000010",
            DataEmissao = DateTime.Now,
            TipoEmissao = TipoEmissaoSample.Normal,
            ValorTotal = 150.50m,
            Itens =
            {
                new ItemSample
                {
                    NumeroItem = 1,
                    CodigoProduto = "PROD001",
                    DescricaoProduto = "PRODUTO DE TESTE",
                    ValorUnitario = 150.5000m
                }
            }
        };

        // Source-generated WriteToXml
        var xml = nfe.WriteToXml();
        Console.WriteLine("XML Gerado via Source Generator:");
        Console.WriteLine(xml);

        // Source-generated ReadFromXml
        var nfeDeserializada = NotaFiscalSample.ReadFromXml(xml);
        Console.WriteLine($"\nDeserializado com sucesso! Chave: {nfeDeserializada?.ChaveAcesso}, Total: {nfeDeserializada?.ValorTotal}");
    }
}