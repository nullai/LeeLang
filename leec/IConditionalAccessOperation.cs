using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IConditionalAccessOperation : IOperation
	{
		/// <summary>
		/// Operation that will be evaluated and accessed if non null.
		/// </summary>
		IOperation Operation { get; }
		/// <summary>
		/// Operation to be evaluated if <see cref="Operation"/> is non null.
		/// </summary>
		IOperation WhenNotNull { get; }
	}
}
