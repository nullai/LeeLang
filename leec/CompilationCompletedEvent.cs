using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class CompilationCompletedEvent : CompilationEvent
	{
		public CompilationCompletedEvent(Compilation compilation) : base(compilation) { }
		public override string ToString()
		{
			return "CompilationCompletedEvent";
		}
	}
}
