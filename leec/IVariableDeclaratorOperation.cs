using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IVariableDeclaratorOperation : IOperation
	{
		/// <summary>
		/// Symbol declared by this variable declaration
		/// </summary>
		ILocalSymbol Symbol { get; }

		/// <summary>
		/// Optional initializer of the variable.
		/// </summary>
		/// <remarks>
		/// If this variable is in an <see cref="IVariableDeclarationOperation"/>, the initializer may be located
		/// in the parent operation. Call <see cref="OperationExtensions.GetVariableInitializer(IVariableDeclaratorOperation)"/>
		/// to check in all locations. It is only possible to have initializers in both locations in VB invalid code scenarios.
		/// </remarks>
		IVariableInitializerOperation Initializer { get; }

		/// <summary>
		/// Additional arguments supplied to the declarator in error cases, ignored by the compiler. This only used for the C# case of
		/// DeclaredArgumentSyntax nodes on a VariableDeclaratorSyntax.
		/// </summary>
		IOperation[] IgnoredArguments { get; }
	}
}
