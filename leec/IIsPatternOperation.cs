using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IIsPatternOperation : IOperation
	{
		/// <summary>
		/// Underlying operation to test.
		/// </summary>
		IOperation Value { get; }

		/// <summary>
		/// Pattern.
		/// </summary>
		IPatternOperation Pattern { get; }
	}
}
