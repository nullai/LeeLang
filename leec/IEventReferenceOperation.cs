using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IEventReferenceOperation : IMemberReferenceOperation
	{
		/// <summary>
		/// Referenced event.
		/// </summary>
		IEventSymbol Event { get; }
	}
}
