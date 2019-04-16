using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IConstantPatternOperation : IPatternOperation
	{
		/// <summary>
		/// Constant value of the pattern operation.
		/// </summary>
		IOperation Value { get; }
	}
}
