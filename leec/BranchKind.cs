using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public enum BranchKind
	{
		/// <summary>
		/// Represents unknown branch kind.
		/// </summary>
		None = 0x0,

		/// <summary>
		/// Represents a continue branch kind.
		/// </summary>
		Continue = 0x1,

		/// <summary>
		/// Represents a break branch kind.
		/// </summary>
		Break = 0x2,

		/// <summary>
		/// Represents a goto branch kind.
		/// </summary>
		GoTo = 0x3
	}
}
