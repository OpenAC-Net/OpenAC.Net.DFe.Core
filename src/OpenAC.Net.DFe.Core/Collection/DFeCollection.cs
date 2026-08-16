// ***********************************************************************
// Assembly         : OpenAC.Net.DFe.Core
// Author           : RFTD
// Created          : 07-26-2014
//
// Last Modified By : RFTD
// Last Modified On : 06-16-2017
// ***********************************************************************
// <copyright file="DFeCollection.cs" company="OpenAC .Net">
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

namespace OpenAC.Net.DFe.Core.Collection;

/// <summary>
/// Representa uma coleção fortemente tipada de itens para documentos DFe, derivada de <see cref="List{TTipo}"/>.
/// </summary>
/// <typeparam name="TTipo">O tipo dos elementos contidos na coleção.</typeparam>
[Serializable]
public class DFeCollection<TTipo> : List<TTipo>
{
    #region Constructors

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeCollection{TTipo}"/>.
    /// </summary>
    public DFeCollection()
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeCollection{TTipo}"/> com a capacidade inicial especificada.
    /// </summary>
    /// <param name="capacity">O número de elementos que a nova lista pode armazenar inicialmente.</param>
    public DFeCollection(int capacity) : base(capacity)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DFeCollection{TTipo}"/> contendo elementos copiados da coleção especificada.
    /// </summary>
    /// <param name="source">A coleção cujos elementos são copiados para a nova lista.</param>
    public DFeCollection(IEnumerable<TTipo> source) : base(source)
    {
    }

    #endregion Constructors

    #region Methods

    /// <summary>
    /// Cria uma nova instância de <typeparamref name="TTipo"/>, adiciona-a ao final da coleção e a retorna.
    /// </summary>
    /// <returns>A nova instância do item criado e adicionado à coleção.</returns>
    public virtual TTipo AddNew()
    {
        var item = (TTipo)Activator.CreateInstance(typeof(TTipo), true);
        Add(item);
        return item;
    }

    /// <summary>
    /// Adiciona um objeto ao final da coleção <see cref="DFeCollection{TTipo}"/>.
    /// </summary>
    /// <param name="item">O objeto a ser adicionado ao final da coleção.</param>
    public new virtual void Add(TTipo item)
    {
        base.Add(item);
    }

    /// <summary>
    /// Insere um elemento na coleção <see cref="DFeCollection{TTipo}"/> no índice especificado.
    /// </summary>
    /// <param name="index">O índice de base zero no qual o item deve ser inserido.</param>
    /// <param name="item">O objeto a ser inserido.</param>
    public new virtual void Insert(int index, TTipo item)
    {
        base.Insert(index, item);
    }

    /// <summary>
    /// Insere os elementos de uma coleção no índice especificado da <see cref="DFeCollection{TTipo}"/>.
    /// </summary>
    /// <param name="index">O índice de base zero no qual os novos elementos devem ser inseridos.</param>
    /// <param name="collection">A coleção cujos elementos devem ser inseridos.</param>
    public new virtual void InsertRange(int index, IEnumerable<TTipo> collection)
    {
        base.InsertRange(index, collection);
    }

    #endregion Methods

    #region Operators

    /// <summary>
    /// Converte implicitamente um array de <typeparamref name="TTipo"/> para <see cref="DFeCollection{TTipo}"/>.
    /// </summary>
    /// <param name="source">Array de origem.</param>
    /// <returns>Uma nova instância de <see cref="DFeCollection{TTipo}"/> contendo os elementos do array.</returns>
    public static implicit operator DFeCollection<TTipo>(TTipo[] source) => new DFeCollection<TTipo>(source);

    #endregion Operators
}