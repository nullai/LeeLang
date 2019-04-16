using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IArrayElementReferenceOperation : IOperation
	{
		/// <summary>
		/// Array to be indexed.
		/// </summary>
		IOperation ArrayReference { get; }
		/// <summary>
		/// Indices that specify an individual element.
		/// </summary>
		IOperation[] Indices { get; }
	}
}
