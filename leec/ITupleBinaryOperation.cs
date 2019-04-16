using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ITupleBinaryOperation : IOperation
	{
		/// <summary>
		/// Kind of binary operation.
		/// </summary>
		BinaryOperatorKind OperatorKind { get; }
		/// <summary>
		/// Left operand.
		/// </summary>
		IOperation LeftOperand { get; }
		/// <summary>
		/// Right operand.
		/// </summary>
		IOperation RightOperand { get; }
	}
}
