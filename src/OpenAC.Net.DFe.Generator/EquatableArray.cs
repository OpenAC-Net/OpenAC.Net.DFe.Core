using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace OpenAC.Net.DFe.Generator;

/// <summary>
/// Estrutura imutável e comparável por igualdade de elementos para uso nos modelos do Roslyn Incremental Generator, garantindo cache correto.
/// </summary>
/// <typeparam name="T">O tipo do item que implementa <see cref="IEquatable{T}"/>.</typeparam>
public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    /// <summary>
    /// Instância estática de array vazio.
    /// </summary>
    public static readonly EquatableArray<T> Empty = new(ImmutableArray<T>.Empty);

    private readonly ImmutableArray<T> array;

    /// <summary>
    /// Inicializa a partir de um <see cref="ImmutableArray{T}"/>.
    /// </summary>
    public EquatableArray(ImmutableArray<T> array)
    {
        this.array = array.IsDefault ? ImmutableArray<T>.Empty : array;
    }

    /// <summary>
    /// Inicializa a partir de uma coleção enumerável <see cref="IEnumerable{T}"/>.
    /// </summary>
    public EquatableArray(IEnumerable<T> collection)
    {
        array = collection?.ToImmutableArray() ?? ImmutableArray<T>.Empty;
    }

    /// <summary>
    /// Obtém o número de elementos no array.
    /// </summary>
    public int Length => array.IsDefault ? 0 : array.Length;

    /// <summary>
    /// Indica se o array está vazio.
    /// </summary>
    public bool IsEmpty => Length == 0;

    /// <summary>
    /// Obtém o elemento na posição especificada.
    /// </summary>
    public T this[int index] => array[index];

    /// <inheritdoc />
    public bool Equals(EquatableArray<T> other)
    {
        if (Length != other.Length) return false;
        for (int i = 0; i < Length; i++)
        {
            if (!this[i].Equals(other[i])) return false;
        }
        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (array.IsDefaultOrEmpty) return 0;
        unchecked
        {
            int hash = 17;
            for (int i = 0; i < array.Length; i++)
            {
                hash = hash * 31 + array[i].GetHashCode();
            }
            return hash;
        }
    }

    /// <summary>
    /// Operador de igualdade.
    /// </summary>
    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);

    /// <summary>
    /// Operador de desigualdade.
    /// </summary>
    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator()
    {
        return (array.IsDefault ? ImmutableArray<T>.Empty : array).AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
