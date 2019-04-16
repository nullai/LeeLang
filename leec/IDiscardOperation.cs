using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IDiscardOperation : IOperation
	{
		/// <summary>
		/// The symbol of the discard operation.
		/// </summary>
		IDiscardSymbol DiscardSymbol { get; }
	}
}
