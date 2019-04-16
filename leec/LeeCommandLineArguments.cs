using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public class LeeCommandLineArguments : CommandLineArguments
	{
		public new CompilationOptions CompilationOptions { get; set; }

		/// <summary>
		/// Gets the parse options for the C# <see cref="Compilation"/>.
		/// </summary>
		public new ParseOptions ParseOptions { get; set; }

		protected override ParseOptions ParseOptionsCore
		{
			get { return ParseOptions; }
		}

		protected override CompilationOptions CompilationOptionsCore
		{
			get { return CompilationOptions; }
		}

		/// <value>
		/// Should the format of error messages include the line and column of
		/// the end of the offending text.
		/// </value>
		public bool ShouldIncludeErrorEndLocation { get; set; }

		public LeeCommandLineArguments()
		{
		}
	}
}
