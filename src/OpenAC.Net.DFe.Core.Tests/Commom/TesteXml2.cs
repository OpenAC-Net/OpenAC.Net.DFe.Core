using System.Collections;
using System.Text;
using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Collection;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Tests.Commom
{
    public class TesteXml2 : IXmlItem
    {
        internal TesteXml2()
        {
        }

        internal static TesteXml2 Create()
        {
            return new TesteXml2();
        }

        [DFeAttribute(TipoCampo.Int, "id", Id = "AT2", Min = 2, Max = 2, Ocorrencia = Ocorrencia.NaoObrigatoria)]
        public int Id { get; set; }

        [DFeElement(TipoCampo.Str, "CData", Id = "ST2", Min = 0, Max = 19, Ocorrencia = Ocorrencia.NaoObrigatoria)]
        public string TestString { get; set; }

        [DFeElement(TipoCampo.De3, "decimal2", Id = "DC2", Min = 0, Max = 9, Ocorrencia = Ocorrencia.NaoObrigatoria)]
        public decimal TestDecimal { get; set; }

        #region Overrides

        public override string ToString()
        {
            var props = this.GetType().GetProperties();
            var builder = new StringBuilder();
            foreach (var prop in props)
            {
                if (prop.PropertyType.IsArray || prop.PropertyType.IsAssignableFrom(typeof(IEnumerable))
                    || prop.PropertyType.IsAssignableFrom(typeof(ICollection)) ||
                    (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>)) ||
                    (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(DFeCollection<>)))
                {
                    var values = ((IEnumerable<object>)prop.GetValue(this, null) ?? new object[0]).ToArray();
                    foreach (var value in values)
                        builder.AppendLine($"{prop.Name}: {value.GetType()}{Environment.NewLine}{value}");
                }
                else
                {
                    var value = prop.GetValue(this, null);
                    builder.AppendLine($"{prop.Name}: {value}");
                }
            }

            return builder.ToString();
        }

        #endregion Overrides
    }
}