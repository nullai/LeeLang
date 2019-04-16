using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ISingleValueCaseClauseOperation : ICaseClauseOperation
	{
		/// <summary>
		/// Case value.
		/// </summary>
		IOperation Value { get; }
	}
}
