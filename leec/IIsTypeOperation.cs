using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IIsTypeOperation : IOperation
	{
		/// <summary>
		/// Value to test.
		/// </summary>
		IOperation ValueOperand { get; }

		/// <summary>
		/// Type for which to test.
		/// </summary>
		ITypeSymbol TypeOperand { get; }

		/// <summary>
		/// Flag indicating if this is an "is not" type expression.
		/// True for VB "TypeOf ... IsNot ..." expression.
		/// False, otherwise.
		/// </summary>
		bool IsNegated { get; }
	}
}
