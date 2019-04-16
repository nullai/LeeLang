using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ILocalReferenceOperation : IOperation
	{
		/// <summary>
		/// Referenced local variable.
		/// </summary>
		ILocalSymbol Local { get; }

		/// <summary>
		/// True if this reference is also the declaration site of this variable. This is true in out variable declarations
		/// and in deconstruction operations where a new variable is being declared.
		/// </summary>
		bool IsDeclaration { get; }
	}
}
