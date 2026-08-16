// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 05-04-2016
//
// Last Modified By : RFTD
// Last Modified On : 05-04-2016
// ***********************************************************************
// <copyright file="DFeParentCollection.cs" company="OpenAC .Net">
//		        		   The MIT License (MIT)
//	     		    Copyright (c) 2014-2026 Grupo OpenAC.Net
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

using System;
using System.Collections.Generic;
using OpenAC.Net.DFe.Core.Attributes;

namespace OpenAC.Net.DFe.Core.Collection;

/// <summary>
/// Coleção de itens DFe que mantém automaticamente a referência do elemento pai (<typeparamref name="TParent"/>) em cada item filho.
/// </summary>
/// <typeparam name="TTipo">O tipo dos itens contidos na coleção, derivado de <see cref="DFeParentItem{TTipo, TParent}"/>.</typeparam>
/// <typeparam name="TParent">O tipo do elemento pai.</typeparam>
public class DFeParentCollection<TTipo, TParent> : DFeCollection<TTipo>
    where TParent : class
    where TTipo : DFeParentItem<TTipo, TParent>
{
    #region Fields

    /// <summary>
    /// Instância do elemento pai vinculado a esta coleção.
    /// </summary>
    protected TParent parent;

    #endregion Fields

    #region Constructors

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeParentCollection{TTipo, TParent}"/>.
    /// </summary>
    public DFeParentCollection()
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeParentCollection{TTipo, TParent}"/> com o elemento pai especificado.
    /// </summary>
    /// <param name="parent">O elemento pai associado.</param>
    public DFeParentCollection(TParent parent)
    {
        Parent = parent;
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeParentCollection{TTipo, TParent}"/> contendo elementos copiados da coleção especificada.
    /// </summary>
    /// <param name="source">A coleção cujos elementos são copiados para a nova lista.</param>
    public DFeParentCollection(IEnumerable<TTipo> source) : base(source)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeParentCollection{TTipo, TParent}"/> com elemento pai e itens iniciais.
    /// </summary>
    /// <param name="parent">O elemento pai associado.</param>
    /// <param name="source">A coleção cujos elementos são copiados para a nova lista.</param>
    public DFeParentCollection(TParent parent, IEnumerable<TTipo> source) : base(source)
    {
        Parent = parent;
    }

    #endregion Constructors

    #region Propriedades

    /// <summary>
    /// Obtém ou define o elemento pai associado a esta coleção e propaga a referência para todos os itens filhos.
    /// </summary>
    [DFeIgnore]
    public TParent Parent
    {
        get => parent;
        set
        {
            parent = value;
            foreach (var item in this)
            {
                if (item.Parent == value) continue;

                item.Parent = value;
            }
        }
    }

    #endregion Propriedades

    #region Methods

    /// <summary>
    /// Cria uma instância de <typeparamref name="TTipo"/>, define o elemento pai, adiciona à coleção e a retorna.
    /// </summary>
    /// <returns>A nova instância do item criado e associado ao pai.</returns>
    public override TTipo AddNew()
    {
        var item = (TTipo)Activator.CreateInstance(typeof(TTipo), true);
        item.Parent = Parent;
        base.Add(item);
        return item;
    }

    /// <summary>
    /// Adiciona um item à coleção e define seu elemento pai.
    /// </summary>
    /// <param name="item">O objeto a ser adicionado.</param>
    public override void Add(TTipo item)
    {
        item.Parent = Parent;
        base.Add(item);
    }

    /// <summary>
    /// Insere um item na coleção na posição especificada e define seu elemento pai.
    /// </summary>
    /// <param name="index">O índice de base zero no qual o item deve ser inserido.</param>
    /// <param name="item">O objeto a ser inserido.</param>
    public override void Insert(int index, TTipo item)
    {
        item.Parent = Parent;
        base.Insert(index, item);
    }

    /// <summary>
    /// Insere uma sequência de elementos na coleção na posição especificada e define o elemento pai em cada um deles.
    /// </summary>
    /// <param name="index">O índice de base zero no qual os novos elementos devem ser inseridos.</param>
    /// <param name="collection">A coleção de elementos a ser inserida.</param>
    public override void InsertRange(int index, IEnumerable<TTipo> collection)
    {
        foreach (var item in collection)
        {
            item.Parent = Parent;
        }

        base.InsertRange(index, collection);
    }

    #endregion Methods

    #region Operators

    /// <summary>
    /// Converte implicitamente um array de <typeparamref name="TTipo"/> para <see cref="DFeParentCollection{TTipo, TParent}"/>.
    /// </summary>
    /// <param name="source">Array de origem.</param>
    /// <returns>Uma nova instância de <see cref="DFeParentCollection{TTipo, TParent}"/> contendo os elementos do array.</returns>
    public static implicit operator DFeParentCollection<TTipo, TParent>(TTipo[] source) => new(source);

    #endregion Operators
}