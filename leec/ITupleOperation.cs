using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ITupleOperation : IOperation
	{
		/// <summary>
		/// Tuple elements.
		/// </summary>
		IOperation[] Elements { get; }

		/// <summary>
		/// Natural type of the tuple, or null if tuple doesn't have a natural type.
		/// Natural type can be different from <see cref="IOperation.Type"/> depending on the
		/// conversion context, in which the tuple is used. 
		/// </summary>
		ITypeSymbol NaturalType { get; }
	}
}
