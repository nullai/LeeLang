using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public struct CaptureId : IEquatable<CaptureId>
	{
		internal CaptureId(int value)
		{
			Value = value;
		}

		internal int Value { get; }

		/// <summary>
		/// Compares <see cref="CaptureId"/>s.
		/// </summary>
		public bool Equals(CaptureId other) => Value == other.Value;

		/// <inheritdoc/>
		public override bool Equals(object obj) => obj is CaptureId && Equals((CaptureId)obj);

		/// <inheritdoc/>
		public override int GetHashCode() => Value.GetHashCode();
	}
}
