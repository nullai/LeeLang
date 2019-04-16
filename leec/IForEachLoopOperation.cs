using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IForEachLoopOperation : ILoopOperation
	{
		/// <summary>
		/// Refers to the operation for declaring a new local variable or reference an existing variable or an expression.
		/// </summary>
		IOperation LoopControlVariable { get; }

		/// <summary>
		/// Collection value over which the loop iterates.
		/// </summary>
		IOperation Collection { get; }

		/// <summary>
		/// Optional list of comma separated next variables at loop bottom in VB.
		/// This list is always empty for C#.
		/// </summary>
		IOperation[] NextVariables { get; }
	}
}
