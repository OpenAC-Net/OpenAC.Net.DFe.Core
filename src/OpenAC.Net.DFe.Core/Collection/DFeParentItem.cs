// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 06-11-2017
//
// Last Modified By : RFTD
// Last Modified On : 06-11-2017
// ***********************************************************************
// <copyright file="DFeParentItem.cs" company="OpenAC .Net">
//		        		   The MIT License (MIT)
//	     		    Copyright (c) 2014 - 2026 Grupo OpenAC.Net
//
//	 Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//	 The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//	 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// </copyright>
// <summary></summary>
// ***********************************************************************

using OpenAC.Net.Core.Generics;
using OpenAC.Net.DFe.Core.Attributes;

namespace OpenAC.Net.DFe.Core.Collection;

/// <summary>
/// Classe base abstrata para itens DFe que mantêm uma referência navegável ao seu elemento pai (<typeparamref name="TParent"/>).
/// </summary>
/// <typeparam name="TTipo">O tipo concreto da classe derivada.</typeparam>
/// <typeparam name="TParent">O tipo da classe pai.</typeparam>
public abstract class DFeParentItem<TTipo, TParent> : GenericClone<DFeParentItem<TTipo, TParent>>
    where TParent : class
    where TTipo : class
{
    #region Fields

    /// <summary>
    /// Instância do elemento pai associado.
    /// </summary>
    protected TParent parent;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Obtém ou define a instância do elemento pai deste item na hierarquia DFe.
    /// </summary>
    [DFeIgnore]
    public TParent Parent
    {
        get => parent;
        set
        {
            if (value == parent) return;

            parent = value;
            OnParentChanged();
        }
    }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Método invocado quando a referência do elemento pai (<see cref="Parent"/>) é alterada.
    /// </summary>
    protected virtual void OnParentChanged()
    {
    }

    #endregion Methods
}