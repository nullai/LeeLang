using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IPropertyInitializerOperation : ISymbolInitializerOperation
	{
		/// <summary>
		/// Initialized properties. There can be multiple properties for Visual Basic 'WithEvents' declaration with AsNew clause.
		/// </summary>
		IPropertySymbol[] InitializedProperties { get; }
	}
}
