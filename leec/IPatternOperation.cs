using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IPatternOperation : IOperation
	{
		/// <summary>
		/// The input type to the pattern-matching operation.
		/// </summary>
		ITypeSymbol InputType { get; }
	}
}
