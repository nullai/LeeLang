using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IForToLoopOperation : ILoopOperation
	{
		/// <summary>
		/// Refers to the operation for declaring a new local variable or reference an existing variable or an expression.
		/// </summary>
		IOperation LoopControlVariable { get; }

		/// <summary>
		/// Operation for setting the initial value of the loop control variable. This comes from the expression between the 'For' and 'To' keywords.
		/// </summary>
		IOperation InitialValue { get; }

		/// <summary>
		/// Operation for the limit value of the loop control variable. This comes from the expression after the 'To' keyword.
		/// </summary>
		IOperation LimitValue { get; }

		/// <summary>
		/// Operation for the step value of the loop control variable. This comes from the expression after the 'Step' keyword,
		/// or inferred by the compiler if 'Step' clause is omitted.
		/// </summary>
		IOperation StepValue { get; }

		/// <summary>
		/// <code>true</code> if arithmetic operations behind this loop are 'checked'.
		/// </summary>
		bool IsChecked { get; }

		/// <summary>
		/// Optional list of comma separated next variables at loop bottom.
		/// </summary>
		IOperation[] NextVariables { get; }
	}
}
