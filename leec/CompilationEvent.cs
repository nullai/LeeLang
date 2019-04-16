using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public abstract class CompilationEvent
	{
		public CompilationEvent(Compilation compilation)
		{
			this.Compilation = compilation;
		}

		public Compilation Compilation { get; }

		/// <summary>
		/// Flush any cached data in this <see cref="CompilationEvent"/> to minimize space usage (at the possible expense of time later).
		/// The principal effect of this is to free cached information on events that have a <see cref="SemanticModel"/>.
		/// </summary>
		public virtual void FlushCache() { }
	}
}
