using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public abstract class CompilationOptions
	{

		public OutputKind OutputKind { get; protected set; }
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

        public Platform Platform { get; protected set; }
		/// <summary>
		/// Global warning report option
		/// </summary>
		public ReportDiagnostic GeneralDiagnosticOption { get; protected set; }

		public MetadataReferenceResolver MetadataReferenceResolver { get; protected set; }

		/// <summary>
		/// Global warning level (from 0 to 4).
		/// </summary>
		public int WarningLevel { get; protected set; }

		/// <summary>
		/// Used for time-based version generation when <see cref="System.Reflection.AssemblyVersionAttribute"/> contains a wildcard.
		/// If equal to default(<see cref="DateTime"/>) the actual current local time will be used.
		/// </summary>
		public DateTime CurrentLocalTime { get; private set; }
		public MetadataImportOptions MetadataImportOptions { get; protected set; }

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
            Platform platform,
			ReportDiagnostic generalDiagnosticOption,
			int warningLevel,
			Dictionary<string, ReportDiagnostic> specificDiagnosticOptions,
			DateTime currentLocalTime,
			bool debugPlusMode,
			MetadataReferenceResolver metadataReferenceResolver,
            MetadataImportOptions metadataImportOptions
			)
		{
			this.ModuleName = moduleName;
			this.MainTypeName = mainTypeName;
			this.CheckOverflow = checkOverflow;
            this.Platform = platform;
			this.GeneralDiagnosticOption = generalDiagnosticOption;
			this.WarningLevel = warningLevel;
			this.SpecificDiagnosticOptions = specificDiagnosticOptions;
			this.ReportSuppressedDiagnostics = reportSuppressedDiagnostics;
			this.CurrentLocalTime = currentLocalTime;
			this.MetadataReferenceResolver = metadataReferenceResolver;
			this.MetadataImportOptions = metadataImportOptions;

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
	}
}
