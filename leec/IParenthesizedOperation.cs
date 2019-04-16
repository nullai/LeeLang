using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IParenthesizedOperation : IOperation
	{
		/// <summary>
		/// Operand enclosed in parentheses.
		/// </summary>
		IOperation Operand { get; }
	}
}
