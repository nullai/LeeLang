using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public enum LoopKind
	{
		/// <summary>
		/// Represents unknown loop kind.
		/// </summary>
		None = 0x0,

		/// <summary>
		/// Represents an <see cref="IWhileLoopOperation"/> in C# or VB.
		/// </summary>
		While = 0x1,

		/// <summary>
		/// Indicates an <see cref="IForLoopOperation"/> in C#.
		/// </summary>
		For = 0x2,

		/// <summary>
		/// Indicates an <see cref="IForToLoopOperation"/> in VB.
		/// </summary>
		ForTo = 0x3,

		/// <summary>
		/// Indicates an <see cref="IForEachLoopOperation"/> in C# or VB.
		/// </summary>
		ForEach = 0x4
	}
}
