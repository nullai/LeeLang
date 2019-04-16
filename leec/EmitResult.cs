using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public class EmitResult
	{
		/// <summary>
		/// True if the compilation successfully produced an executable.
		/// If false then the diagnostics should include at least one error diagnostic
		/// indicating the cause of the failure.
		/// </summary>
		public bool Success { get; }

		/// <summary>
		/// A list of all the diagnostics associated with compilations. This include parse errors, declaration errors,
		/// compilation errors, and emitting errors.
		/// </summary>
		public Diagnostic[] Diagnostics { get; }

		public EmitResult(bool success, Diagnostic[] diagnostics)
		{
			Success = success;
			Diagnostics = diagnostics;
		}

		protected virtual string GetDebuggerDisplay()
		{
			string result = "Success = " + (Success ? "true" : "false");
			if (Diagnostics != null)
			{
				result += ", Diagnostics.Count = " + Diagnostics.Length;
			}
			else
			{
				result += ", Diagnostics = null";
			}

			return result;
		}
	}
}
