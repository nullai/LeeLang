using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ISizeOfOperation : IOperation
	{
		/// <summary>
		/// Type operand.
		/// </summary>
		ITypeSymbol TypeOperand { get; }
	}
}
