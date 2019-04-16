using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IObjectCreationOperation : IOperation
	{
		/// <summary>
		/// Constructor to be invoked on the created instance.
		/// </summary>
		IMethodSymbol Constructor { get; }
		/// <summary>
		/// Arguments of the object creation, excluding the instance argument. Arguments are in evaluation order.
		/// </summary>
		/// <remarks>
		/// If the invocation is in its expanded form, then params/ParamArray arguments would be collected into arrays. 
		/// Default values are supplied for optional arguments missing in source.
		/// </remarks>
		IArgumentOperation[] Arguments { get; }
		/// <summary>
		/// Object or collection initializer, if any.
		/// </summary>
		IObjectOrCollectionInitializerOperation Initializer { get; }
	}
}
