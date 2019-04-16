using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IRangeCaseClauseOperation : ICaseClauseOperation
	{
		/// <summary>
		/// Minimum value of the case range.
		/// </summary>
		IOperation MinimumValue { get; }
		/// <summary>
		/// Maximum value of the case range.
		/// </summary>
		IOperation MaximumValue { get; }
	}
}
