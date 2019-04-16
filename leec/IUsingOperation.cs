using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IUsingOperation : IOperation
	{
		/// <summary>
		/// Body of the using, over which the resources of the using are maintained.
		/// </summary>
		IOperation Body { get; }

		/// <summary>
		/// Declaration introduced or resource held by the using.
		/// </summary>
		IOperation Resources { get; }

		/// <summary>
		/// Locals declared within the <see cref="Resources"/> with scope spanning across this entire <see cref="IUsingOperation"/>.
		/// </summary>
		ILocalSymbol[] Locals { get; }
	}
}
