using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ILabelSymbol : ISymbol
	{
		/// <summary>
		/// Gets the immediately containing <see cref="IMethodSymbol"/> of this <see cref="ILocalSymbol"/>.
		/// </summary>
		IMethodSymbol ContainingMethod { get; }
	}
}
