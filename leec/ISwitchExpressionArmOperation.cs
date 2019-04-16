using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ISwitchExpressionArmOperation : IOperation
	{
		/// <summary>
		/// The pattern to match.
		/// </summary>
		IPatternOperation Pattern { get; }

		/// <summary>
		/// Guard (when clause expression) associated with the switch arm, if any.
		/// </summary>
		IOperation Guard { get; }

		/// <summary>
		/// Result value of the enclosing switch expression when this arm matches.
		/// </summary>
		IOperation Value { get; }

		/// <summary>
		/// Locals declared within the switch arm (e.g. pattern locals and locals declared in the guard) scoped to the arm.
		/// </summary>
		ILocalSymbol[] Locals { get; }
	}
}
