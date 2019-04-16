using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IAnonymousFunctionOperation : IOperation
	{
		/// <summary>
		/// Symbol of the anonymous function.
		/// </summary>
		IMethodSymbol Symbol { get; }
		/// <summary>
		/// Body of the anonymous function.
		/// </summary>
		IBlockOperation Body { get; }
	}
}
