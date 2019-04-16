using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IWhileLoopOperation : ILoopOperation
	{
		/// <summary>
		/// Condition of the loop.
		/// </summary>
		IOperation Condition { get; }

		/// <summary>
		/// True if the <see cref="Condition"/> is evaluated at start of each loop iteration.
		/// False if it is evaluated at the end of each loop iteration.
		/// </summary>
		bool ConditionIsTop { get; }

		/// <summary>
		/// True if the loop has 'Until' loop semantics and the loop is executed while <see cref="Condition"/> is false.
		/// </summary>
		bool ConditionIsUntil { get; }

		/// <summary>
		/// Additional conditional supplied for loop in error cases, which is ignored by the compiler.
		/// For example, for VB 'Do While' or 'Do Until' loop with syntax errors where both the top and bottom conditions are provided.
		/// The top condition is preferred and exposed as <see cref="Condition"/> and the bottom condition is ignored and exposed by this property.
		/// This property should be null for all non-error cases.
		/// </summary>
		IOperation IgnoredCondition { get; }
	}
}
