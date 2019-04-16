using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ICoalesceOperation : IOperation
	{
		/// <summary>
		/// Operation to be unconditionally evaluated.
		/// </summary>
		IOperation Value { get; }
		/// <summary>
		/// Operation to be conditionally evaluated if <see cref="Value"/> evaluates to null/Nothing.
		/// </summary>
		IOperation WhenNull { get; }

		/// <summary>
		/// Conversion associated with <see cref="Value"/> when it is not null/Nothing.
		/// 
		/// Identity if result type of the operation is the same as type of <see cref="Value"/>.
		/// Otherwise, if type of <see cref="Value"/> is nullable, then conversion is applied to an 
		/// unwrapped <see cref="Value"/>, otherwise to the <see cref="Value"/> itself.
		/// </summary>
		CommonConversion ValueConversion { get; }
	}
}
