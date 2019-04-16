using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IFlowAnonymousFunctionOperation : IOperation
	{
		/// <summary>
		/// Symbol of the anonymous function. 
		/// </summary>
		IMethodSymbol Symbol { get; }
	}
}
