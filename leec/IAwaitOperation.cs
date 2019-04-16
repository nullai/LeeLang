using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IAwaitOperation : IOperation
	{
		/// <summary>
		/// Awaited operation.
		/// </summary>
		IOperation Operation { get; }
	}
}
