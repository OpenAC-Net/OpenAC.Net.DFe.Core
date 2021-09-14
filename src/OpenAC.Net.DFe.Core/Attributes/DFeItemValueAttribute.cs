using System;
using OpenAC.Net.DFe.Core.Serializer;

namespace OpenAC.Net.DFe.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DFeItemValueAttribute : DFeBaseAttribute
    {
        #region Constructors

        public DFeItemValueAttribute()
        {
        }

        public DFeItemValueAttribute(TipoCampo tipo)
        {
            Tipo = tipo;
        }

        #endregion Constructors
    }
}