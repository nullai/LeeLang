using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IAddressOfOperation : IOperation
	{
		/// <summary>
		/// Addressed reference.
		/// </summary>
		IOperation Reference { get; }
	}
}
