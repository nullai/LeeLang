using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IRecursivePatternOperation : IPatternOperation
	{
		/// <summary>
		/// The type accepted for the recursive pattern.
		/// </summary>
		ITypeSymbol MatchedType { get; }

		/// <summary>
		/// The symbol, if any, used for the fetching values for subpatterns. This is either a <code>Deconstruct</code>
		/// method, the type <code>System.Runtime.CompilerServices.ITuple</code>, or null (for example, in
		/// error cases or when matching a tuple type).
		/// </summary>
		ISymbol DeconstructSymbol { get; }

		/// <summary>
		/// This contains the patterns contained within a deconstruction or positional subpattern.
		/// </summary>
		IPatternOperation[] DeconstructionSubpatterns { get; }

		/// <summary>
		/// This contains the (symbol, property) pairs within a property subpattern.
		/// </summary>
		IPropertySubpatternOperation[] PropertySubpatterns { get; }

		/// <summary>
		/// Symbol declared by the pattern.
		/// </summary>
		ISymbol DeclaredSymbol { get; }
	}
}
