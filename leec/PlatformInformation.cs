using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace leec
{
	public static class PlatformInformation
	{
		public static bool IsWindows => Path.DirectorySeparatorChar == '\\';
		public static bool IsUnix => Path.DirectorySeparatorChar == '/';
	}
}
