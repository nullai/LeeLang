using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IReturnOperation : IOperation
	{
		/// <summary>
		/// Value to be returned.
		/// </summary>
		IOperation ReturnedValue { get; }
	}
}
