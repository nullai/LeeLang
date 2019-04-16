using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ICaseClauseOperation : IOperation
	{
		/// <summary>
		/// Kind of the clause.
		/// </summary>
		CaseKind CaseKind { get; }

		/// <summary>
		/// Label associated with the case clause, if any.
		/// </summary>
		ILabelSymbol Label { get; }
	}
}
