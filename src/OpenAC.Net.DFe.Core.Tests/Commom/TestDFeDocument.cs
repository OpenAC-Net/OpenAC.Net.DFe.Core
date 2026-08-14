using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Document;

namespace OpenAC.Net.DFe.Core.Tests.Commom
{
    [DFeRoot("RootTest")]
    public class TestDFeDocument : DFeDocument<TestDFeDocument>
    {
        [DFeElement("RequiredChild", Ocorrencia = Ocorrencia.Obrigatoria)]
        public string RequiredChild { get; set; } = string.Empty;

        [DFeElement("OptionalChildClass", Ocorrencia = Ocorrencia.NaoObrigatoria)]
        public OptionalClassModel? OptionalChildClass { get; set; }
    }

    public class OptionalClassModel
    {
        [DFeElement("Value", Ocorrencia = Ocorrencia.Obrigatoria)]
        public string Value { get; set; } = string.Empty;
    }
}