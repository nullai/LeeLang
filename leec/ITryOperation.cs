using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ITryOperation : IOperation
	{
		/// <summary>
		/// Body of the try, over which the handlers are active.
		/// </summary>
		IBlockOperation Body { get; }
		/// <summary>
		/// Catch clauses of the try.
		/// </summary>
		ICatchClauseOperation[] Catches { get; }
		/// <summary>
		/// Finally handler of the try.
		/// </summary>
		IBlockOperation Finally { get; }
		/// <summary>
		/// Exit label for the try. This will always be null for C#.
		/// </summary>
		ILabelSymbol ExitLabel { get; }
	}
}
