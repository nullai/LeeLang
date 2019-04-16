using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ISwitchOperation : IOperation
	{
		/// <summary>
		/// Value to be switched upon.
		/// </summary>
		IOperation Value { get; }
		/// <summary>
		/// Cases of the switch.
		/// </summary>
		ISwitchCaseOperation[] Cases { get; }
		/// <summary>
		/// Exit label for the switch statement.
		/// </summary>
		ILabelSymbol ExitLabel { get; }
		/// <summary>
		/// Locals declared within the switch operation with scope spanning across all <see cref="Cases"/>.
		/// </summary>
		ILocalSymbol[] Locals { get; }
	}
}
