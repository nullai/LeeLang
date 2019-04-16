using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ISwitchExpressionOperation : IOperation
	{
		/// <summary>
		/// Value to be switched upon.
		/// </summary>
		IOperation Value { get; }

		/// <summary>
		/// Arms of the switch expression.
		/// </summary>
		ISwitchExpressionArmOperation[] Arms { get; }
	}
}
