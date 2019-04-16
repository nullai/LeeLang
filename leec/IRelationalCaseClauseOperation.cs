using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IRelationalCaseClauseOperation : ICaseClauseOperation
	{
		/// <summary>
		/// Case value.
		/// </summary>
		IOperation Value { get; }
		/// <summary>
		/// Relational operator used to compare the switch value with the case value.
		/// </summary>
		BinaryOperatorKind Relation { get; }
	}
}
