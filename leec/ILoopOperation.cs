using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ILoopOperation : IOperation
	{
		/// <summary>
		/// Kind of the loop.
		/// </summary>
		LoopKind LoopKind { get; }
		/// <summary>
		/// Body of the loop.
		/// </summary>
		IOperation Body { get; }
		/// <summary>
		/// Declared locals.
		/// </summary>
		ILocalSymbol[] Locals { get; }
		/// <summary>
		/// Loop continue label.
		/// </summary>
		ILabelSymbol ContinueLabel { get; }
		/// <summary>
		/// Loop exit/break label.
		/// </summary>
		ILabelSymbol ExitLabel { get; }
	}
}
