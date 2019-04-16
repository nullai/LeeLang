using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IDeclarationExpressionOperation : IOperation
	{
		/// <summary>
		/// Underlying expression.
		/// </summary>
		IOperation Expression { get; }
	}
}
