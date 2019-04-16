using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IAssignmentOperation : IOperation
	{
		/// <summary>
		/// Target of the assignment.
		/// </summary>
		IOperation Target { get; }
		/// <summary>
		/// Value to be assigned to the target of the assignment.
		/// </summary>
		IOperation Value { get; }
	}
}
