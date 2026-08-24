using System.Xml.Linq;
using OpenAC.Net.DFe.Core.Common;
using OpenAC.Net.DFe.Core.Tests.Serializer.Models;

namespace OpenAC.Net.DFe.Core.Tests.Serializer;

public class NamespaceInheritanceSerializerTests
{
    [Test]
    public async Task TestSerializeDPSDoesNotInjectEmptyXmlns()
    {
        var model = new DPSNamespaceModel();

        var xml = model.GetXml(DFeSaveOptions.None);

        // Verify that xmlns="" is NOT present anywhere in the generated XML
        await Assert.That(xml.Contains("xmlns=\"\"")).IsFalse();

        // Verify that the root element has the correct namespace
        var xDoc = XDocument.Parse(xml);
        var root = xDoc.Root;
        await Assert.That(root).IsNotNull();
        await Assert.That(root!.Name.LocalName).IsEqualTo("DPS");
        await Assert.That(root.Name.NamespaceName).IsEqualTo("http://www.sped.fazenda.gov.br/nfse");

        // Verify that all descendant elements inherit the namespace
        foreach (var element in root.Descendants())
        {
            await Assert.That(element.Name.NamespaceName).IsEqualTo("http://www.sped.fazenda.gov.br/nfse");
        }
    }

    [Test]
    public async Task TestDeserializeDPSWithNamespace()
    {
        const string xml = """
                           <?xml version="1.0" encoding="utf-8"?>
                           <DPS versao="1.01" xmlns="http://www.sped.fazenda.gov.br/nfse">
                             <infDPS Id="DPS3550308212ABC67800019900001000000000000100">
                               <tpAmb>2</tpAmb>
                               <dhEmi>2026-08-24T10:00:00-03:00</dhEmi>
                               <verAplic>OpenAC.NFSe.Nacional</verAplic>
                               <serie>1</serie>
                               <nDPS>100</nDPS>
                               <dCompet>2026-08-24</dCompet>
                               <tpEmit>1</tpEmit>
                               <cLocEmi>3550308</cLocEmi>
                               <prest>
                                 <CNPJ>12ABC678000199</CNPJ>
                                 <email>prestador@teste.com</email>
                                 <regTrib>
                                   <opSimpNac>1</opSimpNac>
                                   <regEspTrib>0</regEspTrib>
                                 </regTrib>
                               </prest>
                               <toma>
                                 <CNPJ>98XYZ432000188</CNPJ>
                                 <xNome>Tomador Teste Ltda</xNome>
                                 <end>
                                   <endNac>
                                     <cMun>3550308</cMun>
                                     <CEP>01310100</CEP>
                                   </endNac>
                                   <xLgr>Avenida Principal</xLgr>
                                   <nro>100</nro>
                                   <xBairro>Centro</xBairro>
                                 </end>
                               </toma>
                               <serv>
                                 <locPrest>
                                   <cLocPrestacao>3550308</cLocPrestacao>
                                 </locPrest>
                                 <cServ>
                                   <cTribNac>010101</cTribNac>
                                   <cTribMun>001</cTribMun>
                                   <xDescServ>Servico de desenvolvimento de software</xDescServ>
                                 </cServ>
                               </serv>
                               <valores>
                                 <vServPrest>
                                   <vServ>100.00</vServ>
                                 </vServPrest>
                                 <trib>
                                   <tribMun>
                                     <tribISSQN>1</tribISSQN>
                                     <tpRetISSQN>1</tpRetISSQN>
                                   </tribMun>
                                 </trib>
                               </valores>
                             </infDPS>
                           </DPS>
                           """;

        var dps = DPSNamespaceModel.Load(xml);
        await Assert.That(dps).IsNotNull();
        await Assert.That(dps.Versao).IsEqualTo("1.01");
        await Assert.That(dps.InfDPS.Id).IsEqualTo("DPS3550308212ABC67800019900001000000000000100");
        await Assert.That(dps.InfDPS.Prest.CNPJ).IsEqualTo("12ABC678000199");
        await Assert.That(dps.InfDPS.Prest.RegTrib.OpSimpNac).IsEqualTo(1);
        await Assert.That(dps.InfDPS.Toma.XNome).IsEqualTo("Tomador Teste Ltda");
        await Assert.That(dps.InfDPS.Toma.End.XLgr).IsEqualTo("Avenida Principal");
        await Assert.That(dps.InfDPS.Toma.End.EndNac.CMun).IsEqualTo("3550308");
        await Assert.That(dps.InfDPS.Serv.CServ.CTribNac).IsEqualTo("010101");
        await Assert.That(dps.InfDPS.Valores.VServPrest.VServ).IsEqualTo(100.00m);
        await Assert.That(dps.InfDPS.Valores.Trib.TribMun.TribISSQN).IsEqualTo(1);
    }
}
