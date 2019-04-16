using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IIsNullOperation : IOperation
	{
		/// <summary>
		/// Value to check.
		/// </summary>
		IOperation Operand { get; }
	}
}
