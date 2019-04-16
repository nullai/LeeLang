using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public struct CommandLineSourceFile
	{
		private readonly string _path;

		public CommandLineSourceFile(string path)
		{
			Debug.Assert(!string.IsNullOrEmpty(path));

			_path = path;
		}

		/// <summary>
		/// Resolved absolute path of the source file (does not contain wildcards).
		/// </summary>
		/// <remarks>
		/// Although this path is absolute it may not be normalized. That is, it may contain ".." and "." in the middle. 
		/// </remarks>
		public string Path
		{
			get { return _path; }
		}
	}
}
