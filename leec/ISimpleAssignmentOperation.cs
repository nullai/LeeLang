using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ISimpleAssignmentOperation : IAssignmentOperation
	{
		/// <summary>
		/// Is this a ref assignment
		/// </summary>
		bool IsRef { get; }
	}
}
