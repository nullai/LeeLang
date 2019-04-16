using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IPointerIndirectionReferenceOperation : IOperation
	{
		/// <summary>
		/// Pointer to be dereferenced.
		/// </summary>
		IOperation Pointer { get; }
	}
}
