using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IMemberInitializerOperation : IOperation
	{
		/// <summary>
		/// Initialized member reference <see cref="IMemberReferenceOperation"/> or an invalid operation for error cases.
		/// </summary>
		IOperation InitializedMember { get; }

		/// <summary>
		/// Member initializer.
		/// </summary>
		IObjectOrCollectionInitializerOperation Initializer { get; }
	}
}
