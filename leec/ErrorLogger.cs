using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public abstract class ErrorLogger
	{
		public abstract void LogDiagnostic(Diagnostic diagnostic);
	}
	public sealed class DiagnosticBagErrorLogger : ErrorLogger
	{
		public readonly DiagnosticBag Diagnostics;

		public DiagnosticBagErrorLogger(DiagnosticBag diagnostics)
		{
			Diagnostics = diagnostics;
		}

		public override void LogDiagnostic(Diagnostic diagnostic)
		{
			Diagnostics.Add(diagnostic);
		}
	}
}
