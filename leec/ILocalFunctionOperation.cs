using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ILocalFunctionOperation : IOperation
	{
		/// <summary>
		/// Local function symbol.
		/// </summary>
		IMethodSymbol Symbol { get; }
		/// <summary>
		/// Body of the local function.
		/// </summary>
		IBlockOperation Body { get; }
		/// <summary>
		/// An extra body for the local function, if both a block body and expression body are specified in source.
		/// </summary>
		/// <remarks>
		/// This is only ever non-null in error situations.
		/// </remarks>
		IBlockOperation IgnoredBody { get; }
	}
}
