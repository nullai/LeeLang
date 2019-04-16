using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	[Flags]
	public enum LocalSlotConstraints : byte
	{
		None = 0,
		ByRef = 1,
		Pinned = 2,
	}
}
