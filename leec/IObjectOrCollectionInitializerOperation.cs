using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IObjectOrCollectionInitializerOperation : IOperation
	{
		/// <summary>
		/// Object member or collection initializers.
		/// </summary>
		IOperation[] Initializers { get; }
	}
}
