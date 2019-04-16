using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IFixedOperation : IOperation
	{
		/// <summary>
		/// Locals declared.
		/// </summary>
		ILocalSymbol[] Locals { get; }
		/// <summary>
		/// Variables to be fixed.
		/// </summary>
		IVariableDeclarationGroupOperation Variables { get; }
		/// <summary>
		/// Body of the fixed, over which the variables are fixed.
		/// </summary>
		IOperation Body { get; }
	}
}
