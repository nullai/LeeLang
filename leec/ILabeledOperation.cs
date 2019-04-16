using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ILabeledOperation : IOperation
	{
		/// <summary>
		///  Label that can be the target of branches.
		/// </summary>
		ILabelSymbol Label { get; }
		/// <summary>
		/// Operation that has been labeled. In VB, this is always null.
		/// </summary>
		IOperation Operation { get; }
	}
}
