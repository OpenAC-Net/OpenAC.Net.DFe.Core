// ***********************************************************************
// Assembly         : OpenAC.Net.NFe
// Author           : RFTD
// Created          : 07-26-2014
//
// Last Modified By : RFTD
// Last Modified On : 06-16-2017
// ***********************************************************************
// <copyright file="DFeCollection.cs" company="OpenAC .Net">
//		        		   The MIT License (MIT)
//	     		    Copyright (c) 2014-2022 Grupo OpenAC.Net
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
/// Classe DFeCollection.
/// </summary>
/// <typeparam name="TTipo"></typeparam>
[Serializable]
public class DFeCollection<TTipo> : List<TTipo>
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DFeCollection{T}"/> class.
    /// </summary>
    public DFeCollection()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DFeCollection{T}"/> class.
    /// </summary>
    /// <param name="capacity">The source.</param>
    public DFeCollection(int capacity) : base(capacity)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DFeCollection{T}"/> class.
    /// </summary>
    /// <param name="source">The source.</param>
    public DFeCollection(IEnumerable<TTipo> source) : base(source)
    {
    }

    #endregion Constructors

    #region Methods

    /// <summary>
    /// Adds an object to the end of the <see cref="DFeCollection{T}"/>.
    /// </summary>
    /// <returns>T.</returns>
    public virtual TTipo AddNew()
    {
        var item = (TTipo)Activator.CreateInstance(typeof(TTipo), true);
        Add(item);
        return item;
    }

    /// <summary>Adds an object to the end of the <see cref="DFeCollection{T}"/>.</summary>
    /// <param name="item">The object to be added to the end of the <see cref="DFeCollection{T}"/>. The value can be null for reference types.</param>
    public new virtual void Add(TTipo item)
    {
        base.Add(item);
    }

    /// <summary>Inserts an element into the <see cref="DFeCollection{T}"/> at the specified index.</summary>
    /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
    /// <param name="item">The object to insert. The value can be null for reference types.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="index" /> is less than 0.-or-<paramref name="index" /> is greater than <see cref="DFeCollection{T}.Count"/>.</exception>
    public new virtual void Insert(int index, TTipo item)
    {
        base.Insert(index, item);
    }

    /// <summary>Inserts the elements of a collection into the <see cref="DFeCollection{T}"/> at the specified index.</summary>
    /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
    /// <param name="collection">The collection whose elements should be inserted into the <see cref="DFeCollection{T}"/>. The collection itself cannot be null, but it can contain elements that are null, if type <paramref name="T" /> is a reference type.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="collection" /> is null.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="index" /> is less than 0.-or-<paramref name="index" /> is greater than <see cref="DFeCollection{T}.Count"/>.</exception>
    public new virtual void InsertRange(int index, IEnumerable<TTipo> collection)
    {
        base.InsertRange(index, collection);
    }

    #endregion Methods

    #region Operators

    /// <summary>
    /// Performs an implicit conversion from <see cref="T:TTipo[]"/> to <see cref="DFeCollection{TTipo}"/>.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator DFeCollection<TTipo>(TTipo[] source) => new DFeCollection<TTipo>(source);

    #endregion Operators
}