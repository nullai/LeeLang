using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IParameterInitializerOperation : ISymbolInitializerOperation
	{
		/// <summary>
		/// Initialized parameter.
		/// </summary>
		IParameterSymbol Parameter { get; }
	}
}
