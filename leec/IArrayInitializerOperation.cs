using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IArrayInitializerOperation : IOperation
	{
		/// <summary>
		/// Values to initialize array elements.
		/// </summary>
		IOperation[] ElementValues { get; }
	}
}
