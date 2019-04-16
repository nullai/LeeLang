using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public struct CommandLineAnalyzerReference : IEquatable<CommandLineAnalyzerReference>
	{
		private readonly string _path;

		public CommandLineAnalyzerReference(string path)
		{
			_path = path;
		}

		/// <summary>
		/// Assembly file path.
		/// </summary>
		public string FilePath
		{
			get
			{
				return _path;
			}
		}

		public override bool Equals(object obj)
		{
			return obj is CommandLineAnalyzerReference && base.Equals((CommandLineAnalyzerReference)obj);
		}

		public bool Equals(CommandLineAnalyzerReference other)
		{
			return _path == other._path;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(_path, 0);
		}
	}
}
