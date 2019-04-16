using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IMemberReferenceOperation : IOperation
	{
		/// <summary>
		/// Instance of the type. Null if the reference is to a static/shared member.
		/// </summary>
		IOperation Instance { get; }

		/// <summary>
		/// Referenced member.
		/// </summary>
		ISymbol Member { get; }
	}
}
