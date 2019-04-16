using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IBranchOperation : IOperation
	{
		/// <summary>
		/// Label that is the target of the branch.
		/// </summary>
		ILabelSymbol Target { get; }
		/// <summary>
		/// Kind of the branch.
		/// </summary>
		BranchKind BranchKind { get; }
	}
}
