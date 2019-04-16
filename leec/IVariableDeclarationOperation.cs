using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IVariableDeclarationOperation : IOperation
	{
		/// <summary>
		/// Individual variable declarations declared by this multiple declaration.
		/// </summary>
		/// <remarks>
		/// All <see cref="IVariableDeclarationGroupOperation"/> will have at least 1 <see cref="IVariableDeclarationOperation"/>,
		/// even if the declaration group only declares 1 variable.
		/// </remarks>
		IVariableDeclaratorOperation[] Declarators { get; }

		/// <summary>
		/// Optional initializer of the variable.
		/// </summary>
		/// <remarks>
		/// In C#, this will always be null.
		/// </remarks>
		IVariableInitializerOperation Initializer { get; }
	}
}
