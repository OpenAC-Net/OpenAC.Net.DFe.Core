﻿using System.Xml.Linq;
using OpenAC.Net.DFe.Core.Collection;
using OpenAC.Net.DFe.Core.Common;
using OpenAC.Net.DFe.Core.Service;
using OpenAC.Net.DFe.Core.Tests.Commom;

namespace OpenAC.Net.DFe.Core.Tests
{
    public class SerializerTest
    {
        private static TesteXml GenerateXml()
        {
            var xml = new TesteXml
            {
                Id = 1,
                TestDate = DateTime.Now,
                TestDecimal = 100000.00M,
                TesteEnum = TesteEnum.Value3,
                TesteEnum1 = TesteEnum.Value1,
                TesteEnum2 = null,
                TestNullInt = 999,
                TestDateTz = new DateTimeOffset(DateTime.Now, TimeSpan.FromHours(-4))
            };

            var cdata = File.ReadAllText("cdata_teste.xml");

            for (var i = 0; i < 3; i++)
            {
                var item = new TesteXml2
                {
                    Id = i + 1,
                    TestDecimal = xml.TestDecimal + i + 1.000M,
                    TestString = $"<![CDATA[{cdata}]]>"
                };
                xml.XmlItems.Add(item);
            }

            xml.XmlItems2 = xml.XmlItems.AsEnumerable();
            xml.XmlItems3 = xml.XmlItems.ToArray();

            for (var i = 0; i < 3; i++)
            {
                xml.TesteListEnum.Add((TesteEnum)i);
            }

            xml.TesteDateTime.Add(DateTime.Now);
            xml.TesteDateTime.Add(DateTime.MinValue);
            xml.TesteDateTime.Add(DateTime.MaxValue);

            var collection = new Xml3Collection();

            for (var i = 0; i < 3; i++)
            {
                var item = new TesteXml3
                {
                    Id = i + 1,
                    TestDecimal = xml.TestDecimal + i + 1.000M,
                    TestString = $"XmlItem3 {i + 1}"
                };
                xml.XmlItems.Add(item);
                collection.Add(item);
            }

            xml.TestInterface3 = collection;

            for (var i = 0; i < 5; i++)
            {
                var prod = xml.XmlProd.AddNew();
                prod.Id = i;
                prod.TestDecimal = xml.TestDecimal + 1;
                prod.TestString = "XmlItem4  1";

                var prod2 = xml.XmlProd2.AddNew();
                prod2.Id = i;
                prod2.TestDecimal = xml.TestDecimal + 1;
                prod2.TestString = "XmlItem4  2";
            }

            xml.XmlProd3 = xml.XmlProd2.ToArray();

            xml.TestInterface1 = xml.XmlItems[0];
            xml.TestInterface2 = xml.XmlItems[1];
            xml.Xml5.Id = 10;
            xml.Xml5.TestDecimal = 5.0000000000M;

            return xml;
        }

        private const string Services = "<?xml version=\"1.0\" encoding=\"utf-8\"?><DFeServices><Services Tipo=\"CTe\" TipoEmissao=\"1\"><Enderecos Ambiente=\"2\" UF=\"50\"><Enderecos><Endereco Tipo=\"Envio\">Envio</Endereco><Endereco Tipo=\"Consulta\">Consulta</Endereco></Enderecos></Enderecos></Services></DFeServices>";

        [Fact]
        public void TestSerializer()
        {
            var xml = GenerateXml();

            xml.Save("teste.xml");

            Assert.True(File.Exists("teste.xml"), "Erro ao serializar a classe");

            var xmlDocument = XDocument.Load("teste.xml");
            Assert.NotNull(xmlDocument);

            Assert.NotNull(xmlDocument.Root);
            Assert.True(xmlDocument.Root?.Name == "RFTD", "Erro ao serializar root do Xml.");

            Assert.True(xmlDocument.Root.HasAttributes, "Erro ao serializar atributos do root.");
            Assert.True(xmlDocument.Root.Attributes().Count() == 1, "Erro ao serializar atributos do root.");
            Assert.True(xmlDocument.Root.FirstAttribute.Name == "id", "Erro ao serializar atributo id do root. Atributo com nome errado!");
            Assert.True(xmlDocument.Root.FirstAttribute.Value == "01", "Erro ao serializar atributo id do root. Valor incorreto!");

            var nodes = xmlDocument.Root.Nodes();
            Assert.Equal(39, nodes.Count());

            //var testXml = TesteXml.Load("teste.xml");
            //Assert.Equal(39, nodes.Count());

            File.Delete("teste.xml");
        }

        [Fact]
        public void TestDeserializer()
        {
            var xml = GenerateXml();
            xml.Save("teste.xml");

            var item = TesteXml.Load("teste.xml");

            Assert.NotEqual(xml, item);

            File.Delete("teste.xml");
        }

        [Fact]
        public void TestDFeServiceSerializer()
        {
            var services = new DFeServices<DFeTipo>();
            services.Webservices.Add(new DFeServiceInfo<DFeTipo>()
            {
                Tipo = DFeTipoServico.CTe,
                TipoEmissao = DFeTipoEmissao.Normal,
                Ambientes = new DFeCollection<DFeServiceEnvironment<DFeTipo>>()
                {
                    new DFeServiceEnvironment<DFeTipo>
                    {
                        Ambiente = DFeTipoAmbiente.Homologacao,
                        UF = DFeSiglaUF.MS,
                        Enderecos = new Dictionary<DFeTipo, string>()
                        {
                            { DFeTipo.Envio, "Envio" },
                            { DFeTipo.Consulta, "Consulta" }
                        }
                    },
                    new DFeServiceEnvironment<DFeTipo>
                    {
                        Ambiente = DFeTipoAmbiente.Producao,
                        UF = DFeSiglaUF.MS,
                        Enderecos = new Dictionary<DFeTipo, string>()
                        {
                            { DFeTipo.Envio, "Envio" },
                            { DFeTipo.Consulta, "Consulta" }
                        }
                    }
                }
            });

            var xml = services.GetXml();

            Assert.NotEqual(string.Empty, xml);

            var service = DFeServices<DFeTipo>.Load(xml);
            var serviceXml = service.GetXml();

            Assert.Equal(xml, serviceXml);
        }
    }
}