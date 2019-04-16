using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IParameterReferenceOperation : IOperation
	{
		/// <summary>
		/// Referenced parameter.
		/// </summary>
		IParameterSymbol Parameter { get; }
	}
}
