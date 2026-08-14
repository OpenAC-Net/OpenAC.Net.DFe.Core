using OpenAC.Net.DFe.Core.Tests.Commom;

namespace OpenAC.Net.DFe.Core.Tests
{
    public class ClassTypeDeserializationTest
    {
        /// <summary>
        /// Valida que a ausência de uma tag XML opcional mantém a propriedade nula no objeto deserializado.
        /// </summary>
        [Fact]
        public void DeserializarXmlSemTagOpcional_DeveManterPropriedadeComoNull()
        {
            var xml = @"<RootTest><RequiredChild>Valor</RequiredChild></RootTest>";

            var result = TestDFeDocument.Load(xml);

            Assert.NotNull(result);
            Assert.Equal("Valor", result.RequiredChild);
            Assert.Null(result.OptionalChildClass);
        }

        /// <summary>
        /// Valida que a presença de uma tag XML opcional instancia e preenche a propriedade corretamente.
        /// </summary>
        [Fact]
        public void DeserializarXmlComTagOpcional_DeveInstanciarEPreencherPropriedade()
        {
            var xml = @"<RootTest>
                            <RequiredChild>Valor</RequiredChild>
                            <OptionalChildClass>
                                <Value>Teste</Value>
                            </OptionalChildClass>
                        </RootTest>";

            var result = TestDFeDocument.Load(xml);

            Assert.NotNull(result);
            Assert.Equal("Valor", result.RequiredChild);
            Assert.NotNull(result.OptionalChildClass);
            Assert.Equal("Teste", result.OptionalChildClass.Value);
        }
    }
}