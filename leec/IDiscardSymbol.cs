using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IDiscardSymbol : ISymbol
	{
		/// <summary>
		/// The type of the discarded value.
		/// </summary>
		ITypeSymbol Type { get; }
	}
}
