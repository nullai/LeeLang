using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ICatchClauseOperation : IOperation
	{
		/// <summary>
		/// Body of the exception handler.
		/// </summary>
		IBlockOperation Handler { get; }

		/// <summary>
		/// Locals declared by the <see cref="ExceptionDeclarationOrExpression"/> and/or <see cref="Filter"/> clause.
		/// </summary>
		ILocalSymbol[] Locals { get; }

		/// <summary>
		/// Type of the exception handled by the catch clause.
		/// </summary>
		ITypeSymbol ExceptionType { get; }

		/// <summary>
		/// Optional source for exception. This could be any of the following operation:
		/// 1. Declaration for the local catch variable bound to the caught exception (C# and VB) OR
		/// 2. Null, indicating no declaration or expression (C# and VB)
		/// 3. Reference to an existing local or parameter (VB) OR
		/// 4. Other expression for error scenarios (VB)
		/// </summary>
		IOperation ExceptionDeclarationOrExpression { get; }

		/// <summary>
		/// Filter operation to be executed to determine whether to handle the exception.
		/// </summary>
		IOperation Filter { get; }
	}
}
