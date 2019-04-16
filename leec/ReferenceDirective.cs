using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public struct ReferenceDirective
	{
		public readonly string File;
		public readonly Location Location;

		public ReferenceDirective(string file, Location location)
		{
			Debug.Assert(file != null);
			Debug.Assert(location != null);

			File = file;
			Location = location;
		}
	}
}
