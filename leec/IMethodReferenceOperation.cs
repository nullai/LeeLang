using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IMethodReferenceOperation : IMemberReferenceOperation
	{
		/// <summary>
		/// Referenced method.
		/// </summary>
		IMethodSymbol Method { get; }

		/// <summary>
		/// Indicates whether the reference uses virtual semantics.
		/// </summary>
		bool IsVirtual { get; }
	}
}
