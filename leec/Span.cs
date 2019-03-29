// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
#if !FEATURE_PORTABLE_SPAN
using System.Runtime.Versioning;
#endif // !FEATURE_PORTABLE_SPAN

namespace System
{
    /// <summary>
    /// Span represents a contiguous region of arbitrary memory. Unlike arrays, it can point to either managed
    /// or native memory, or to memory allocated on the stack. It is type- and memory-safe.
    /// </summary>
    public partial struct Span<T>
    {
        /// <summary>
        /// The number of items in the read-only span.
        /// </summary>
        public int Length
        {
            get
            {
                return _length;
            }
        }

        /// <summary>
        /// Returns true if Length is 0.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return _length == 0;
            }
        }
        /// <summary>
        /// Returns false if left and right point at the same memory and have the same length.  Note that
        /// this does *not* check to see if the *contents* are equal.
        /// </summary>
        public static bool operator !=(Span<T> left, Span<T> right) => !(left == right);

        /// <summary>
        /// Defines an implicit conversion of an array to a <see cref="Span{T}"/>
        /// </summary>
        public static implicit operator Span<T>(T[] array) => new Span<T>(array);

        /// <summary>
        /// Defines an implicit conversion of a <see cref="ArraySegment{T}"/> to a <see cref="Span{T}"/>
        /// </summary>
        public static implicit operator Span<T>(ArraySegment<T> segment)
            => new Span<T>(segment.Array, segment.Offset, segment.Count);

        /// <summary>
        /// Returns a 0-length read-only span whose base is the null pointer.
        /// </summary>
        public static Span<T> Empty => null;

        /// <summary>Gets an enumerator for this span.</summary>
        public Enumerator GetEnumerator() => new Enumerator(this);

        /// <summary>Enumerates the elements of a <see cref="Span{T}"/>.</summary>
        public struct Enumerator
        {
            /// <summary>The span being enumerated.</summary>
            private Span<T> _span;
            /// <summary>The next index to yield.</summary>
            private int _index;

            /// <summary>Initialize the enumerator.</summary>
            /// <param name="span">The span to enumerate.</param>
            public Enumerator(Span<T> span)
            {
                _span = span;
                _index = -1;
            }

            /// <summary>Advances the enumerator to the next element of the span.</summary>
            public bool MoveNext()
            {
                int index = _index + 1;
                if (index < _span.Length)
                {
                    _index = index;
                    return true;
                }

                return false;
            }

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            public T Current
            {
                get => _span[_index];
            }
        }
    }
}
