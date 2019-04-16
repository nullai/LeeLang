using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IFieldInitializerOperation : ISymbolInitializerOperation
	{
		/// <summary>
		/// Initialized fields. There can be multiple fields for Visual Basic fields declared with AsNew clause.
		/// </summary>
		IFieldSymbol[] InitializedFields { get; }
	}
}
