using System.Collections;
using System.Text;
using OpenAC.Net.DFe.Core.Attributes;
using OpenAC.Net.DFe.Core.Collection;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Tests.Commom
{
    [DFeRoot]
    public class TesteXml5
    {
        [DFeAttribute(TipoCampo.Int, "id", Id = "AT4", Min = 2, Max = 2, Ocorrencia = Ocorrencia.NaoObrigatoria)]
        public int Id { get; set; }

        [DFeElement(TipoCampo.De10, "decimal2", Id = "DC4", Min = 0, Max = 9, Ocorrencia = Ocorrencia.NaoObrigatoria)]
        public decimal TestDecimal { get; set; }

        private string GetRootName()
        {
            return TestDecimal > 10 ? "Root10" : "RootTeste";
        }

        private static string[] GetRootNames()
        {
            return new[] { "RootTeste", "Root10" };
        }

        #region Overrides

        public override string ToString()
        {
            var props = GetType().GetProperties();
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