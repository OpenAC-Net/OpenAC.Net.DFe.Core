using OpenAC.Net.DFe.Core.Attributes;

namespace OpenAC.Net.DFe.Core.Tests.Commom
{
    public enum TesteEnum
    {
        [DFeEnum("1")]
        Value1,

        [DFeEnum("2")]
        Value2,

        [DFeEnum("3")]
        Value3
    }
}