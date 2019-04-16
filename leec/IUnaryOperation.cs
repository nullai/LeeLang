using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public enum UnaryOperatorKind
	{
		/// <summary>
		/// Represents unknown or error operator kind.
		/// </summary>
		None = 0x0,

		/// <summary>
		/// Represents the C# '~' operator.
		/// </summary>
		BitwiseNegation = 0x1,

		/// <summary>
		/// Represents the C# '!' operator and VB 'Not' operator.
		/// </summary>
		Not = 0x2,

		/// <summary>
		/// Represents the unary '+' operator.
		/// </summary>
		Plus = 0x3,

		/// <summary>
		/// Represents the unary '-' operator.
		/// </summary>
		Minus = 0x4,

		/// <summary>
		/// Represents the C# 'true' operator and VB 'IsTrue' operator.
		/// </summary>
		True = 0x5,

		/// <summary>
		/// Represents the C# 'false' operator and VB 'IsFalse' operator.
		/// </summary>
		False = 0x6,
	}

	public interface IUnaryOperation : IOperation
	{
		/// <summary>
		/// Kind of unary operation.
		/// </summary>
		UnaryOperatorKind OperatorKind { get; }

		/// <summary>
		/// Operand.
		/// </summary>
		IOperation Operand { get; }

		/// <summary>
		/// Operator method used by the operation, null if the operation does not use an operator method.
		/// </summary>
		IMethodSymbol OperatorMethod { get; }

		/// <summary>
		/// <see langword="true"/> if this is a 'lifted' unary operator.  When there is an 
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
