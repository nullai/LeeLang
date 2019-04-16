using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ISwitchCaseOperation : IOperation
	{
		/// <summary>
		/// Clauses of the case.
		/// </summary>
		ICaseClauseOperation[] Clauses { get; }
		/// <summary>
		/// One or more operations to execute within the switch section.
		/// </summary>
		IOperation[] Body { get; }
		/// <summary>
		/// Locals declared within the switch case section scoped to the section.
		/// </summary>
		ILocalSymbol[] Locals { get; }
	}
}
