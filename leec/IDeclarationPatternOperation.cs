using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IDeclarationPatternOperation : IPatternOperation
	{
		/// <summary>
		/// The type explicitly specified, or null if it was inferred (e.g. using <code>var</code> in C#).
		/// </summary>
		ITypeSymbol MatchedType { get; }

		/// <summary>
		/// True if the pattern is of a form that accepts null.
		/// For example, in C# the pattern `var x` will match a null input,
		/// while the pattern `string x` will not.
		/// </summary>
		bool MatchesNull { get; }

		/// <summary>
		/// Symbol declared by the pattern, if any.
		/// </summary>
		ISymbol DeclaredSymbol { get; }
	}
}
