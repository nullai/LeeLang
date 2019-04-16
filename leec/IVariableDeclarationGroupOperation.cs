using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IVariableDeclarationGroupOperation : IOperation
	{
		/// <summary>
		/// Variable declaration in the statement.
		/// </summary>
		/// <remarks>
		/// In C#, this will always be a single declaration, with all variables in <see cref="IVariableDeclarationOperation.Declarators"/>.
		/// </remarks>
		IVariableDeclarationOperation[] Declarations { get; }
	}
}
