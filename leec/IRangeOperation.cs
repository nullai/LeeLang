using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IRangeOperation : IOperation
	{
		/// <summary>
		/// Left operand.
		/// </summary>
		IOperation LeftOperand { get; }

		/// <summary>
		/// Right operand.
		/// </summary>
		IOperation RightOperand { get; }

		/// <summary>
		/// <code>true</code> if this is a 'lifted' range operation.  When there is an 
		/// operator that is defined to work on a value type, 'lifted' operators are 
		/// created to work on the <see cref="System.Nullable{T}"/> versions of those
		/// value types.
		/// </summary>
		bool IsLifted { get; }

		/// <summary>
		/// Factory method used to create this Range value. Can be null if appropriate
		/// symbol was not found.
		/// </summary>
		IMethodSymbol Method { get; }
	}
}
