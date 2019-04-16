using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ISymbolInitializerOperation : IOperation
	{
		/// <summary>
		/// Local declared in and scoped to the <see cref="Value"/>.
		/// </summary>
		ILocalSymbol[] Locals { get; }

		/// <summary>
		/// Underlying initializer value.
		/// </summary>
		IOperation Value { get; }
	}
}
