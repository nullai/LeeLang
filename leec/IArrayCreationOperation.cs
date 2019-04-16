using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IArrayCreationOperation : IOperation
	{
		/// <summary>
		/// Sizes of the dimensions of the created array instance.
		/// </summary>
		IOperation[] DimensionSizes { get; }
		/// <summary>
		/// Values of elements of the created array instance.
		/// </summary>
		IArrayInitializerOperation Initializer { get; }
	}
}
