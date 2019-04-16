using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IStaticLocalInitializationSemaphoreOperation : IOperation
	{
		/// <summary>
		/// The static local variable that is possibly initialized.
		/// </summary>
		ILocalSymbol Local { get; }
	}
}
