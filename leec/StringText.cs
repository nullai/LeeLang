using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public class StringText : SourceText
	{
		private readonly string _source;
		public StringText(string source)
		{
			_source = source;
		}

		public string Source => _source;

		public override int Length => _source.Length;

		public override char this[int position]
		{
			get
			{
				// NOTE: we are not validating position here as that would not 
				//       add any value to the range check that string accessor performs anyways.

				return _source[position];
			}
		}
		public override string ToString(TextSpan span)
		{
			if (span.End > this.Source.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(span));
			}

			if (span.Start == 0 && span.Length == this.Length)
			{
				return this.Source;
			}

			return this.Source.Substring(span.Start, span.Length);
		}

		public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
		{
			this.Source.CopyTo(sourceIndex, destination, destinationIndex, count);
		}
	}
}
