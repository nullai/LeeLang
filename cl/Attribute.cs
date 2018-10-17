using System;
using System.Collections.Generic;

namespace LeeLang
{
	[Flags]
	public enum SysAttribute : uint
	{
		None = 0,
		Public = 1 << 0,
		Protected = 1 << 1,
		Private = 1 << 2,
		Internal = 1 << 3,
		Static = 1 << 4,
		Const = 1 << 5,
		Week = 1 << 6,
	}

	public class Attributes
	{
		public List<LocatedToken> sys;
	}
}

