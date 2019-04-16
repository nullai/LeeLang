using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IIncrementOrDecrementOperation : IOperation
	{
		/// <summary>
		/// Target of the assignment.
		/// </summary>
		IOperation Target { get; }

		/// <summary>
		/// Operator method used by the operation, null if the operation does not use an operator method.
		/// </summary>
		IMethodSymbol OperatorMethod { get; }

		/// <summary>
		/// <see langword="true"/> if this is a postfix expression.
		/// <see langword="false"/> if this is a prefix expression.
		/// </summary>
		bool IsPostfix { get; }

		/// <summary>
		/// <see langword="true"/> if this is a 'lifted' increment operator.  When there is an 
		/// operator that is defined to work on a value type, 'lifted' operators are 
		/// created to work on the <see cref="System.Nullable{T}"/> versions of those
		/// value types.
		/// </summary>
		bool IsLifted { get; }

		/// <summary>
		/// <see langword="true"/> if overflow checking is performed for the arithmetic operation.
		/// </summary>
		bool IsChecked { get; }
	}
}
