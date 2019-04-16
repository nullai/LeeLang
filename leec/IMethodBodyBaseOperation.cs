using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IMethodBodyBaseOperation : IOperation
	{
		/// <summary>
		/// Method body corresponding to BaseMethodDeclarationSyntax.Body or AccessorDeclarationSyntax.Body
		/// </summary>
		IBlockOperation BlockBody { get; }

		/// <summary>
		/// Method body corresponding to BaseMethodDeclarationSyntax.ExpressionBody or AccessorDeclarationSyntax.ExpressionBody
		/// </summary>
		IBlockOperation ExpressionBody { get; }
	}
}
