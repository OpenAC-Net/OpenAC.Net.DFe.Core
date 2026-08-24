using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Document;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Tests.Serializer.Models;

[DFeRoot("DPS", Namespace = "http://www.sped.fazenda.gov.br/nfse")]
public partial class DPSNamespaceModel : DFeDocument<DPSNamespaceModel>
{
    [DFeAttribute(TipoCampo.Str, "versao")]
    public string Versao { get; set; } = "1.01";

    [DFeElement("infDPS")]
    public InfDPSNamespaceModel InfDPS { get; set; } = new();
}

public partial class InfDPSNamespaceModel
{
    [DFeAttribute(TipoCampo.Str, "Id")]
    public string Id { get; set; } = "DPS3550308212ABC67800019900001000000000000100";

    [DFeElement(TipoCampo.Int, "tpAmb")]
    public int TpAmb { get; set; } = 2;

    [DFeElement(TipoCampo.Str, "dhEmi")]
    public string DhEmi { get; set; } = "2026-08-24T10:00:00-03:00";

    [DFeElement(TipoCampo.Str, "verAplic")]
    public string VerAplic { get; set; } = "OpenAC.NFSe.Nacional";

    [DFeElement(TipoCampo.Int, "serie")]
    public int Serie { get; set; } = 1;

    [DFeElement(TipoCampo.Int, "nDPS")]
    public int NDPS { get; set; } = 100;

    [DFeElement(TipoCampo.Str, "dCompet")]
    public string DCompet { get; set; } = "2026-08-24";

    [DFeElement(TipoCampo.Int, "tpEmit")]
    public int TpEmit { get; set; } = 1;

    [DFeElement(TipoCampo.Str, "cLocEmi")]
    public string CLocEmi { get; set; } = "3550308";

    [DFeElement("prest")]
    public PrestNamespaceModel Prest { get; set; } = new();

    [DFeElement("toma")]
    public TomaNamespaceModel Toma { get; set; } = new();

    [DFeElement("serv")]
    public ServNamespaceModel Serv { get; set; } = new();

    [DFeElement("valores")]
    public ValoresNamespaceModel Valores { get; set; } = new();
}

public partial class PrestNamespaceModel
{
    [DFeElement(TipoCampo.Str, "CNPJ")]
    public string CNPJ { get; set; } = "12ABC678000199";

    [DFeElement(TipoCampo.Str, "email")]
    public string Email { get; set; } = "prestador@teste.com";

    [DFeElement("regTrib")]
    public RegTribNamespaceModel RegTrib { get; set; } = new();
}

public partial class RegTribNamespaceModel
{
    [DFeElement(TipoCampo.Int, "opSimpNac")]
    public int OpSimpNac { get; set; } = 1;

    [DFeElement(TipoCampo.Int, "regEspTrib")]
    public int RegEspTrib { get; set; } = 0;
}

public partial class TomaNamespaceModel
{
    [DFeElement(TipoCampo.Str, "CNPJ")]
    public string CNPJ { get; set; } = "98XYZ432000188";

    [DFeElement(TipoCampo.Str, "xNome")]
    public string XNome { get; set; } = "Tomador Teste Ltda";

    [DFeElement("end")]
    public EndNamespaceModel End { get; set; } = new();
}

public partial class EndNamespaceModel
{
    [DFeElement("endNac")]
    public EndNacNamespaceModel EndNac { get; set; } = new();

    [DFeElement(TipoCampo.Str, "xLgr")]
    public string XLgr { get; set; } = "Avenida Principal";

    [DFeElement(TipoCampo.Str, "nro")]
    public string Nro { get; set; } = "100";

    [DFeElement(TipoCampo.Str, "xBairro")]
    public string XBairro { get; set; } = "Centro";
}

public partial class EndNacNamespaceModel
{
    [DFeElement(TipoCampo.Str, "cMun")]
    public string CMun { get; set; } = "3550308";

    [DFeElement(TipoCampo.Str, "CEP")]
    public string CEP { get; set; } = "01310100";
}

public partial class ServNamespaceModel
{
    [DFeElement("locPrest")]
    public LocPrestNamespaceModel LocPrest { get; set; } = new();

    [DFeElement("cServ")]
    public CServNamespaceModel CServ { get; set; } = new();
}

public partial class LocPrestNamespaceModel
{
    [DFeElement(TipoCampo.Str, "cLocPrestacao")]
    public string CLocPrestacao { get; set; } = "3550308";
}

public partial class CServNamespaceModel
{
    [DFeElement(TipoCampo.Str, "cTribNac")]
    public string CTribNac { get; set; } = "010101";

    [DFeElement(TipoCampo.Str, "cTribMun")]
    public string CTribMun { get; set; } = "001";

    [DFeElement(TipoCampo.Str, "xDescServ")]
    public string XDescServ { get; set; } = "Servico de desenvolvimento de software";
}

public partial class ValoresNamespaceModel
{
    [DFeElement("vServPrest")]
    public VServPrestNamespaceModel VServPrest { get; set; } = new();

    [DFeElement("trib")]
    public TribNamespaceModel Trib { get; set; } = new();
}

public partial class VServPrestNamespaceModel
{
    [DFeElement(TipoCampo.De2, "vServ")]
    public decimal VServ { get; set; } = 100.00m;
}

public partial class TribNamespaceModel
{
    [DFeElement("tribMun")]
    public TribMunNamespaceModel TribMun { get; set; } = new();
}

public partial class TribMunNamespaceModel
{
    [DFeElement(TipoCampo.Int, "tribISSQN")]
    public int TribISSQN { get; set; } = 1;

    [DFeElement(TipoCampo.Int, "tpRetISSQN")]
    public int TpRetISSQN { get; set; } = 1;
}
