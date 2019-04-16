using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IConstructorBodyOperation : IMethodBodyBaseOperation
	{
		/// <summary>
		/// Local declarations contained within the <see cref="Initializer"/>.
		/// </summary>
		ILocalSymbol[] Locals { get; }

		/// <summary>
		/// Constructor initializer, if any
		/// </summary>
		IOperation Initializer { get; }
	}
}
