using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public abstract class CompilationOptions
	{

		/// <summary>
		/// Name of the primary module, or null if a default name should be used.
		/// </summary>
		/// <remarks>
		/// The name usually (but not necessarily) includes an extension, e.g. "MyModule.dll".
		/// 
		/// If <see cref="ModuleName"/> is null the actual name written to metadata  
		/// is derived from the name of the compilation (<see cref="Compilation.AssemblyName"/>)
		/// by appending a default extension for <see cref="OutputKind"/>.
		/// </remarks>
		public string ModuleName { get; protected set; }

		/// <summary>
		/// The full name of a type that declares static Main method. Must be a valid non-generic namespace-qualified name.
		/// Null if any static Main method is a candidate for an entry point.
		/// </summary>
		public string MainTypeName { get; protected set; }

		/// <summary>
		/// Whether bounds checking on integer arithmetic is enforced by default or not.
		/// </summary>
		public bool CheckOverflow { get; protected set; }

		/// <summary>
		/// Global warning report option
		/// </summary>
		public ReportDiagnostic GeneralDiagnosticOption { get; protected set; }

		/// <summary>
		/// Global warning level (from 0 to 4).
		/// </summary>
		public int WarningLevel { get; protected set; }

		/// <summary>
		/// Used for time-based version generation when <see cref="System.Reflection.AssemblyVersionAttribute"/> contains a wildcard.
		/// If equal to default(<see cref="DateTime"/>) the actual current local time will be used.
		/// </summary>
		public DateTime CurrentLocalTime { get; private set; }

		/// <summary>
		/// Modifies the incoming diagnostic, for example escalating its severity, or discarding it (returning null) based on the compilation options.
		/// </summary>
		/// <param name="diagnostic"></param>
		/// <returns>The modified diagnostic, or null</returns>
		public abstract Diagnostic FilterDiagnostic(Diagnostic diagnostic);

		/// <summary>
		/// Warning report option for each warning.
		/// </summary>
		public Dictionary<string, ReportDiagnostic> SpecificDiagnosticOptions { get; protected set; }

		/// <summary>
		/// Whether diagnostics suppressed in source, i.e. <see cref="Diagnostic.IsSuppressed"/> is true, should be reported.
		/// </summary>
		public bool ReportSuppressedDiagnostics { get; protected set; }

		public NullableContextOptions NullableContextOptions { get; private set; }

		private readonly Lazy<Diagnostic[]> _lazyErrors;

		// Expects correct arguments.
		public CompilationOptions(
			bool reportSuppressedDiagnostics,
			string moduleName,
			string mainTypeName,
			bool checkOverflow,
			ReportDiagnostic generalDiagnosticOption,
			int warningLevel,
			Dictionary<string, ReportDiagnostic> specificDiagnosticOptions,
			DateTime currentLocalTime,
			bool debugPlusMode)
		{
			this.ModuleName = moduleName;
			this.MainTypeName = mainTypeName;
			this.CheckOverflow = checkOverflow;
			this.GeneralDiagnosticOption = generalDiagnosticOption;
			this.WarningLevel = warningLevel;
			this.SpecificDiagnosticOptions = specificDiagnosticOptions;
			this.ReportSuppressedDiagnostics = reportSuppressedDiagnostics;
			this.CurrentLocalTime = currentLocalTime;

			_lazyErrors = new Lazy<Diagnostic[]>(() =>
			{
				var builder = new List<Diagnostic>();
				ValidateOptions(builder);
				return builder.ToArray();
			});
		}

		/// <summary>
		/// Gets the source language ("C#" or "Visual Basic").
		/// </summary>
		public abstract string Language { get; }

		public static bool IsValidFileAlignment(int value)
		{
			switch (value)
			{
				case 512:
				case 1024:
				case 2048:
				case 4096:
				case 8192:
					return true;

				default:
					return false;
			}
		}

		public abstract string[] GetImports();

		/// <summary>
		/// Creates a new options instance with the specified suppressed diagnostics reporting option.
		/// </summary>
		public CompilationOptions WithReportSuppressedDiagnostics(bool value)
		{
			return CommonWithReportSuppressedDiagnostics(value);
		}


		public CompilationOptions WithModuleName(string moduleName)
		{
			return CommonWithModuleName(moduleName);
		}

		public CompilationOptions WithMainTypeName(string mainTypeName)
		{
			return CommonWithMainTypeName(mainTypeName);
		}

		public CompilationOptions WithOverflowChecks(bool checkOverflow)
		{
			return CommonWithCheckOverflow(checkOverflow);
		}


		protected abstract CompilationOptions CommonWithReportSuppressedDiagnostics(bool reportSuppressedDiagnostics);
		protected abstract CompilationOptions CommonWithModuleName(string moduleName);
		protected abstract CompilationOptions CommonWithMainTypeName(string mainTypeName);
		protected abstract CompilationOptions CommonWithCheckOverflow(bool checkOverflow);

		/// <summary>
		/// Performs validation of options compatibilities and generates diagnostics if needed
		/// </summary>
		public abstract void ValidateOptions(List<Diagnostic> builder);

		public void ValidateOptions(List<Diagnostic> builder, CommonMessageProvider messageProvider)
		{
		}

		/// <summary>
		/// Errors collection related to an incompatible set of compilation options
		/// </summary>
		public Diagnostic[] Errors
		{
			get { return _lazyErrors.Value; }
		}

		public abstract override bool Equals(object obj);

		protected bool EqualsHelper(CompilationOptions other)
		{
			if (object.ReferenceEquals(other, null))
			{
				return false;
			}

			// NOTE: StringComparison.Ordinal is used for type name comparisons, even for VB.  That's because
			// a change in the canonical case should still change the option.
			bool equal =
				   this.CheckOverflow == other.CheckOverflow &&
				   this.CurrentLocalTime == other.CurrentLocalTime &&
				   this.GeneralDiagnosticOption == other.GeneralDiagnosticOption &&
				   string.Equals(this.MainTypeName, other.MainTypeName, StringComparison.Ordinal) &&
				   string.Equals(this.ModuleName, other.ModuleName, StringComparison.Ordinal) &&
				   this.ReportSuppressedDiagnostics == other.ReportSuppressedDiagnostics &&
				   this.WarningLevel == other.WarningLevel;

			return equal;
		}

		public abstract override int GetHashCode();

		protected int GetHashCodeHelper()
		{
			return Hash.Combine(this.CheckOverflow,
				   Hash.Combine(this.CurrentLocalTime.GetHashCode(),
				   Hash.Combine((int)this.GeneralDiagnosticOption,
				   Hash.Combine(this.MainTypeName != null ? StringComparer.Ordinal.GetHashCode(this.MainTypeName) : 0,
				   Hash.Combine(this.ModuleName != null ? StringComparer.Ordinal.GetHashCode(this.ModuleName) : 0,
				   Hash.Combine(this.ReportSuppressedDiagnostics,
				   Hash.Combine(Hash.CombineValues(this.SpecificDiagnosticOptions),
				   this.WarningLevel)))))));
		}

		public static bool operator ==(CompilationOptions left, CompilationOptions right)
		{
			return object.Equals(left, right);
		}

		public static bool operator !=(CompilationOptions left, CompilationOptions right)
		{
			return !object.Equals(left, right);
		}
	}
}
