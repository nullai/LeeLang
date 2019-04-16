using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IAliasSymbol : ISymbol
	{
		/// <summary>
		/// Gets the <see cref="INamespaceOrTypeSymbol"/> for the
		/// namespace or type referenced by the alias.
		/// </summary>
		INamespaceOrTypeSymbol Target { get; }
	}
}
