using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IThrowOperation : IOperation
	{
		/// <summary>
		/// Instance of an exception being thrown.
		/// </summary>
		IOperation Exception { get; }
	}
}
