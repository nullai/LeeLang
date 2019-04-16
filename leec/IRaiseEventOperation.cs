using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IRaiseEventOperation : IOperation
	{
		/// <summary>
		/// Reference to the event to be raised.
		/// </summary>
		IEventReferenceOperation EventReference { get; }
		/// <summary>
		/// Arguments of the invocation, excluding the instance argument. Arguments are in evaluation order.
		/// </summary>
		/// <remarks>
		/// If the invocation is in its expanded form, then params/ParamArray arguments would be collected into arrays. 
		/// Default values are supplied for optional arguments missing in source.
		/// </remarks>
		IArgumentOperation[] Arguments { get; }
	}
}
