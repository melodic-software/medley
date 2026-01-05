// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// This file provides polyfills for System.Index and System.Range in netstandard2.0.
// Required for C# 8+ range/index syntax (^1, ..) when targeting netstandard2.0.
// Reference: https://github.com/dotnet/runtime/issues/28639

#if NETSTANDARD2_0

using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// Represents a type that can be used to index a collection either from the start or the end.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal readonly struct Index : IEquatable<Index>
{
    private readonly int _value;

    /// <summary>
    /// Initializes a new <see cref="Index"/> with a specified index position and a value indicating if the index is from the start or from the end.
    /// </summary>
    /// <param name="value">The index value.</param>
    /// <param name="fromEnd">Indicates if the index is from the end.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Index(int value, bool fromEnd = false)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "value must be non-negative");
        }

        _value = fromEnd ? ~value : value;
    }

    private Index(int value)
    {
        _value = value;
    }

    /// <summary>Gets an <see cref="Index"/> that points to the first element of a collection.</summary>
    public static Index Start => new(0);

    /// <summary>Gets an <see cref="Index"/> that points beyond the last element.</summary>
    public static Index End => new(~0);

    /// <summary>Creates an <see cref="Index"/> from the start at the given index position.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Index FromStart(int value)
    {
        return value < 0 ? throw new ArgumentOutOfRangeException(nameof(value), "value must be non-negative") : new Index(value);
    }

    /// <summary>Creates an <see cref="Index"/> from the end at the given index position.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Index FromEnd(int value)
    {
        return value < 0 ? throw new ArgumentOutOfRangeException(nameof(value), "value must be non-negative") : new Index(~value);
    }

    /// <summary>Gets the index value.</summary>
    public int Value => _value < 0 ? ~_value : _value;

    /// <summary>Gets a value indicating whether the index is from the start or the end.</summary>
    public bool IsFromEnd => _value < 0;

    /// <summary>Calculates the offset from the start using the given length.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetOffset(int length)
    {
        int offset = _value;
        if (IsFromEnd)
        {
            offset += length + 1;
        }

        return offset;
    }

    /// <inheritdoc />
    public override bool Equals(object? value) => value is Index index && _value == index._value;

    /// <inheritdoc />
    public bool Equals(Index other) => _value == other._value;

    /// <inheritdoc />
    public override int GetHashCode() => _value;

    /// <summary>Converts an integer to an Index.</summary>
    public static implicit operator Index(int value) => FromStart(value);

    /// <inheritdoc />
    public override string ToString() => IsFromEnd
        ? "^" + ((uint)Value).ToString(CultureInfo.InvariantCulture)
        : ((uint)Value).ToString(CultureInfo.InvariantCulture);
}

/// <summary>Represents a range that has start and end indexes.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal readonly struct Range : IEquatable<Range>
{
    /// <summary>Gets the inclusive start index of the Range.</summary>
    public Index Start { get; }

    /// <summary>Gets the exclusive end index of the Range.</summary>
    public Index End { get; }

    /// <summary>Initializes a new <see cref="Range"/> instance with the specified starting and ending indexes.</summary>
    public Range(Index start, Index end)
    {
        Start = start;
        End = end;
    }

    /// <inheritdoc />
    public override bool Equals(object? value) => value is Range r && r.Start.Equals(Start) && r.End.Equals(End);

    /// <inheritdoc />
    public bool Equals(Range other) => other.Start.Equals(Start) && other.End.Equals(End);

    /// <inheritdoc />
    public override int GetHashCode() => Start.GetHashCode() * 31 + End.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => Start + ".." + End;

    /// <summary>Creates a Range object starting from the start index to the end of the collection.</summary>
    public static Range StartAt(Index start) => new(start, Index.End);

    /// <summary>Creates a Range object from the start of the collection to the end index.</summary>
    public static Range EndAt(Index end) => new(Index.Start, end);

    /// <summary>Gets a Range object that starts from the first element and ends at the last element.</summary>
    public static Range All => new(Index.Start, Index.End);

    /// <summary>Calculates the start and length for the range.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int Offset, int Length) GetOffsetAndLength(int length)
    {
        int start = Start.GetOffset(length);
        int end = End.GetOffset(length);

        return (uint)end > (uint)length || (uint)start > (uint)end
            ? throw new ArgumentOutOfRangeException(nameof(length))
            : ((int Offset, int Length))(start, end - start);
    }
}

#endif
