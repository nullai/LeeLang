using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IPropertySubpatternOperation : IOperation
	{
		/// <summary>
		/// The member being matched in a property subpattern.  This can be a <see cref="IMemberReferenceOperation"/>
		/// in non-error cases, or an <see cref="IInvalidOperation"/> in error cases.
		/// </summary>
		// The symbol should be exposed for error cases somehow:
		// https://github.com/dotnet/roslyn/issues/33175
		IOperation Member { get; }

		/// <summary>
		/// The pattern to which the member is matched in a property subpattern.
		/// </summary>
		IPatternOperation Pattern { get; }
	}
}
