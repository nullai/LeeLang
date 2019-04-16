using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IPropertyReferenceOperation : IMemberReferenceOperation
	{
		/// <summary>
		/// Referenced property.
		/// </summary>
		IPropertySymbol Property { get; }
		/// <summary>
		/// Arguments of the indexer property reference, excluding the instance argument. Arguments are in evaluation order.
		/// </summary>
		/// <remarks>
		/// If the invocation is in its expanded form, then params/ParamArray arguments would be collected into arrays. 
		/// Default values are supplied for optional arguments missing in source.
		/// </remarks>
		IArgumentOperation[] Arguments { get; }
	}
}
