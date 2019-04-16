using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IConditionalOperation : IOperation
	{
		/// <summary>
		/// Condition to be tested.
		/// </summary>
		IOperation Condition { get; }
		/// <summary>
		/// Operation to be executed if the <see cref="Condition"/> is true.
		/// </summary>
		IOperation WhenTrue { get; }
		/// <summary>
		/// Operation to be executed if the <see cref="Condition"/> is false.
		/// </summary>
		IOperation WhenFalse { get; }

		/// <summary>
		/// Is result a managed reference
		/// </summary>
		bool IsRef { get; }
	}
}
