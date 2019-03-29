using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public enum ThreeState : byte
	{
		Unknown = 0,
		False = 1,
		True = 2,
	}

	public static class ThreeStateHelpers
	{
		public static ThreeState ToThreeState(this bool value)
		{
			return value ? ThreeState.True : ThreeState.False;
		}

		public static bool HasValue(this ThreeState value)
		{
			return value != ThreeState.Unknown;
		}

		public static bool Value(this ThreeState value)
		{
			Debug.Assert(value != ThreeState.Unknown);
			return value == ThreeState.True;
		}
	}
}
