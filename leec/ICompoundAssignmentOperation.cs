using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ICompoundAssignmentOperation : IAssignmentOperation
	{
		/// <summary>
		/// Kind of binary operation.
		/// </summary>
		BinaryOperatorKind OperatorKind { get; }

		/// <summary>
		/// Operator method used by the operation, null if the operation does not use an operator method.
		/// </summary>
		IMethodSymbol OperatorMethod { get; }

		/// <summary>
		/// <see langword="true"/> if this assignment contains a 'lifted' binary operation.
		/// </summary>
		bool IsLifted { get; }

		/// <summary>
		/// <see langword="true"/> if overflow checking is performed for the arithmetic operation.
		/// </summary>
		bool IsChecked { get; }

		/// <summary>
		/// Conversion applied to <see cref="IAssignmentOperation.Target"/> before the operation occurs.
		/// </summary>
		CommonConversion InConversion { get; }

		/// <summary>
		/// Conversion applied to the result of the binary operation, before it is assigned back to
		/// <see cref="IAssignmentOperation.Target"/>.
		/// </summary>
		CommonConversion OutConversion { get; }
	}
}
