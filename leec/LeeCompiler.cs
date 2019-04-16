using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace leec
{
	public class LeeCompiler : CommonCompiler
	{
		public LeeCompiler(CommandLineParser parser, string responseFile, string[] args, BuildPaths buildPaths, string additionalReferenceDirectories)
			: base(parser, responseFile, args, buildPaths, additionalReferenceDirectories)
		{

		}
		public override DiagnosticFormatter DiagnosticFormatter => throw new NotImplementedException();

		public override Type Type => throw new NotImplementedException();

		public override Compilation CreateCompilation(TextWriter consoleOutput, ErrorLogger errorLoggerOpt)
		{
			throw new NotImplementedException();
		}

		public override string GetToolName()
		{
			throw new NotImplementedException();
		}

		public override void PrintHelp(TextWriter consoleOutput)
		{
			throw new NotImplementedException();
		}

		public override void PrintLangVersions(TextWriter consoleOutput)
		{
			throw new NotImplementedException();
		}

		public override void PrintLogo(TextWriter consoleOutput)
		{
			throw new NotImplementedException();
		}

		public override bool SuppressDefaultResponseFile(IEnumerable<string> args)
		{
			throw new NotImplementedException();
		}

		protected override bool TryGetCompilerDiagnosticCode(string diagnosticId, out uint code)
		{
			throw new NotImplementedException();
		}
	}
}
