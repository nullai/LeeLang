using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace leec
{
	public abstract class AdditionalText
	{
		/// <summary>
		/// Path to the text.
		/// </summary>
		public abstract string Path { get; }

		/// <summary>
		/// Retrieves a <see cref="SourceText"/> with the contents of this file.
		/// </summary>
		public abstract SourceText GetText(CancellationToken cancellationToken = default(CancellationToken));
	}
}
