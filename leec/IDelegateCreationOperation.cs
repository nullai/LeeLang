using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IDelegateCreationOperation : IOperation
	{
		/// <summary>
		/// The lambda or method binding that this delegate is created from.
		/// </summary>
		IOperation Target { get; }
	}
}
