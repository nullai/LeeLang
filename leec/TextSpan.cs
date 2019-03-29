using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public struct TextSpan : IEquatable<TextSpan>, IComparable<TextSpan>
	{
		public TextSpan(int start, int length)
		{
			if (start < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(start));
			}

			if (start + length < start)
			{
				throw new ArgumentOutOfRangeException(nameof(length));
			}

			Start = start;
			Length = length;
		}

		/// <summary>
		/// Start point of the span.
		/// </summary>
		public int Start { get; }

		/// <summary>
		/// End of the span.
		/// </summary>
		public int End => Start + Length;

		/// <summary>
		/// Length of the span.
		/// </summary>
		public int Length { get; }

		/// <summary>
		/// Determines whether or not the span is empty.
		/// </summary>
		public bool IsEmpty => this.Length == 0;

		public bool Contains(int position)
		{
			return unchecked((uint)(position - Start) < (uint)Length);
		}

		public bool Contains(TextSpan span)
		{
			return span.Start >= Start && span.End <= this.End;
		}

		public bool OverlapsWith(TextSpan span)
		{
			int overlapStart = Math.Max(Start, span.Start);
			int overlapEnd = Math.Min(this.End, span.End);

			return overlapStart < overlapEnd;
		}

		public bool IntersectsWith(TextSpan span)
		{
			return span.Start <= this.End && span.End >= Start;
		}

		public bool IntersectsWith(int position)
		{
			return unchecked((uint)(position - Start) <= (uint)Length);
		}

		public static TextSpan FromBounds(int start, int end)
		{
			if (start < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(start), "@@StartMustNotBeNegative");
			}

			if (end < start)
			{
				throw new ArgumentOutOfRangeException(nameof(end), "@@EndMustNotBeLessThanStart");
			}

			return new TextSpan(start, end - start);
		}

		public static bool operator ==(TextSpan left, TextSpan right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(TextSpan left, TextSpan right)
		{
			return !left.Equals(right);
		}

		public bool Equals(TextSpan other)
		{
			return Start == other.Start && Length == other.Length;
		}

		public override bool Equals(object obj)
		{
			return obj is TextSpan && Equals((TextSpan)obj);
		}

		public override int GetHashCode()
		{
			return Hash.Combine(Start, Length);
		}

		public override string ToString()
		{
			return $"[{Start}..{End})";
		}

		public int CompareTo(TextSpan other)
		{
			var diff = Start - other.Start;
			if (diff != 0)
			{
				return diff;
			}

			return Length - other.Length;
		}
	}
}
