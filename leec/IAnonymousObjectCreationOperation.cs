using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IAnonymousObjectCreationOperation : IOperation
	{
		/// <summary>
		/// Property initializers.
		/// Each initializer is an <see cref="ISimpleAssignmentOperation"/>, with an <see cref="IPropertyReferenceOperation"/>
		/// as the target whose Instance is an <see cref="IInstanceReferenceOperation"/> with <see cref="InstanceReferenceKind.ImplicitReceiver"/> kind.
		/// </summary>
		IOperation[] Initializers { get; }
	}
}
