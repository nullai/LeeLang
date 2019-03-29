// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using EditorBrowsableAttribute = System.ComponentModel.EditorBrowsableAttribute;
using EditorBrowsableState = System.ComponentModel.EditorBrowsableState;

#pragma warning disable 0809  //warning CS0809: Obsolete member 'Span<T>.Equals(object)' overrides non-obsolete member 'object.Equals(object)'

#if BIT64
using nuint = System.UInt64;
#else
using nuint = System.UInt32;
#endif

namespace System
{
    /// <summary>
    /// Span represents a contiguous region of arbitrary memory. Unlike arrays, it can point to either managed
    /// or native memory, or to memory allocated on the stack. It is type- and memory-safe.
    /// </summary>
    public partial struct Span<T>
    {
        /// <summary>A byref or a native ptr.</summary>
        public readonly T[] _pointer;
		/// <summary>The number of elements this Span contains.</summary>
#if PROJECTN
        [Bound]
#endif
		public readonly int _start;
		public readonly int _length;

		/// <summary>
		/// Creates a new read-only span over the entirety of the target array.
		/// </summary>
		/// <param name="array">The target array.</param>
		/// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
		/// reference (Nothing in Visual Basic).</exception>
		public Span(T[] array)
        {
            if (array == null)
            {
				_pointer = null;
				_start = 0;
				_length = 0;
				return; // returns default
            }

            _pointer = array;
			_start = 0;
			_length = array.Length;
        }

        /// <summary>
        /// Creates a new read-only span over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="start">The index at which to begin the read-only span.</param>
        /// <param name="length">The number of items in the read-only span.</param>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        /// reference (Nothing in Visual Basic).</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;=Length).
        /// </exception>
        public Span(T[] array, int start, int length)
        {
            if (array == null)
            {
                if (start != 0 || length != 0)
                    throw new Exception();
				_pointer = null;
				_start = 0;
				_length = 0;
				return; // returns default
            }
            if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
				throw new Exception();

			_pointer = array;
			_start = start;
            _length = length;
		}

        /// <summary>
        /// Returns the specified element of the read-only span.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// Thrown when index less than 0 or index greater than or equal to Length
        /// </exception>
        public ref T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_length)
					throw new Exception();
				return ref _pointer[_start + index];
            }
        }

        /// <summary>
        /// Copies the contents of this read-only span into destination span. If the source
        /// and destinations overlap, this method behaves as if the original values in
        /// a temporary location before the destination is overwritten.
        ///
        /// <param name="destination">The span to copy items into.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the destination Span is shorter than the source Span.
        /// </exception>
        /// </summary>
        public void CopyTo(Span<T> destination)
        {
            // Using "if (!TryCopyTo(...))" results in two branches: one for the length
            // check, and one for the result of TryCopyTo. Since these checks are equivalent,
            // we can optimize by performing the check once ourselves then calling Memmove directly.

            if ((uint)_length <= (uint)destination.Length)
            {
				Array.Copy(_pointer, _start, destination._pointer, destination._start, _length);
            }
            else
            {
				throw new Exception();
			}
        }

        /// Copies the contents of this read-only span into destination span. If the source
        /// and destinations overlap, this method behaves as if the original values in
        /// a temporary location before the destination is overwritten.
        /// </summary>
        /// <returns>If the destination span is shorter than the source span, this method
        /// return false and no data is written to the destination.</returns>
        /// <param name="destination">The span to copy items into.</param>
        public bool TryCopyTo(Span<T> destination)
        {
            bool retVal = false;
            if ((uint)_length <= (uint)destination.Length)
            {
				Array.Copy(_pointer, _start, destination._pointer, destination._start, _length);
				retVal = true;
            }
            return retVal;
        }

        /// <summary>
        /// Returns true if left and right point at the same memory and have the same length.  Note that
        /// this does *not* check to see if the *contents* are equal.
        /// </summary>
        public static bool operator ==(Span<T> left, Span<T> right)
        {
            return left._length == right._length && left._start == right._start && left._pointer == right._pointer;
        }

		/// <summary>
		/// For <see cref="Span{Char}"/>, returns a new instance of string that represents the characters pointed to by the span.
		/// Otherwise, returns a <see cref="string"/> with the name of the type and the number of elements.
		/// </summary>
		public override string ToString()
        {
            return string.Format("System.Span<{0}>[{1}]", typeof(T).Name, _length);
        }

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// Forms a slice out of the given read-only span, beginning at 'start'.
		/// </summary>
		/// <param name="start">The index at which to begin this slice.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// Thrown when the specified <paramref name="start"/> index is not in range (&lt;0 or &gt;=Length).
		/// </exception>
		public Span<T> Slice(int start)
        {
            if ((uint)start > (uint)_length)
				throw new Exception();

			return new Span<T>(_pointer, _start + start, _length - start);
        }

        /// <summary>
        /// Forms a slice out of the given read-only span, beginning at 'start', of given length
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice (exclusive).</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in range (&lt;0 or &gt;=Length).
        /// </exception>
        public Span<T> Slice(int start, int length)
        {
            if ((uint)start > (uint)_length || (uint)length > (uint)(_length - start))
				throw new Exception();

			return new Span<T>(_pointer, _start + start, length);
        }

        /// <summary>
        /// Copies the contents of this read-only span into a new array.  This heap
        /// allocates, so should generally be avoided, however it is sometimes
        /// necessary to bridge the gap with APIs written in terms of arrays.
        /// </summary>
        public T[] ToArray()
        {
            if (_length == 0)
                return new T[0];

            var destination = new T[_length];
			Array.Copy(_pointer, _start, destination, 0, _length);
            return destination;
        }
    }
}
