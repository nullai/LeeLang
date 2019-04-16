using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IForLoopOperation : ILoopOperation
	{
		/// <summary>
		/// List of operations to execute before entry to the loop. For C#, this comes from the first clause of the for statement.
		/// </summary>
		IOperation[] Before { get; }

		/// <summary>
		/// Locals declared within the loop Condition and are in scope throughout the <see cref="Condition"/>,
		/// <see cref="ILoopOperation.Body"/> and <see cref="AtLoopBottom"/>.
		/// They are considered to be declared per iteration.
		/// </summary>
		ILocalSymbol[] ConditionLocals { get; }

		/// <summary>
		/// Condition of the loop. For C#, this comes from the second clause of the for statement.
		/// </summary>
		IOperation Condition { get; }

		/// <summary>
		/// List of operations to execute at the bottom of the loop. For C#, this comes from the third clause of the for statement.
		/// </summary>
		IOperation[] AtLoopBottom { get; }
	}
}
