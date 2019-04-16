using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface INameOfOperation : IOperation
	{
		/// <summary>
		/// Argument to the name of operation.
		/// </summary>
		IOperation Argument { get; }
	}
}
