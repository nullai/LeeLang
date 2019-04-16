using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IBlockOperation : IOperation
	{
		/// <summary>
		/// Operations contained within the block.
		/// </summary>
		IOperation[] Operations { get; }
		/// <summary>
		/// Local declarations contained within the block.
		/// </summary>
		ILocalSymbol[] Locals { get; }
	}
}
