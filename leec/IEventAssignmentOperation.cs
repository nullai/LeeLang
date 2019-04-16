using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IEventAssignmentOperation : IOperation
	{
		/// <summary>
		/// Reference to the event being bound.
		/// </summary>
		IOperation EventReference { get; }

		/// <summary>
		/// Handler supplied for the event.
		/// </summary>
		IOperation HandlerValue { get; }

		/// <summary>
		/// True for adding a binding, false for removing one.
		/// </summary>
		bool Adds { get; }
	}
}
