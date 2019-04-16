using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace leec
{
	public abstract class CommandLineArguments
	{
		/// <summary>
		/// Drop to an interactive loop. If a script is specified in <see cref="SourceFiles"/> executes the script first.
		/// </summary>
		public bool InteractiveMode { get; set; }

		/// <summary>
		/// Directory used to resolve relative paths stored in the arguments.
		/// </summary>
		/// <remarks>
		/// Except for paths stored in <see cref="MetadataReferences"/>, all
		/// paths stored in the properties of this class are resolved and
		/// absolute. This is the directory that relative paths specified on
		/// command line were resolved against.
		/// </remarks>
		public string BaseDirectory { get; set; }

		/// <summary>
		/// A list of pairs of paths. This stores the value of the command-line compiler
		/// option /pathMap:X1=Y1;X2=Y2... which causes a prefix of X1 followed by a path
		/// separator to be replaced by Y1 followed by a path separator, and so on for each following pair.
		/// </summary>
		/// <remarks>
		/// This option is used to help get build-to-build determinism even when the build
		/// directory is different from one build to the next.  The prefix matching is case sensitive.
		/// </remarks>
		public KeyValuePair<string, string>[] PathMap { get; set; }

		/// <summary>
		/// Sequence of absolute paths used to search for references.
		/// </summary>
		public string[] ReferencePaths { get; set; }

		/// <summary>
		/// Sequence of absolute paths used to search for sources specified as #load directives.
		/// </summary>
		public string SourcePaths { get; set; }

		/// <summary>
		/// Sequence of absolute paths used to search for key files.
		/// </summary>
		public string KeyFileSearchPaths { get; set; }

		/// <summary>
		/// If true, use UTF8 for output.
		/// </summary>
		public bool Utf8Output { get; set; }

		/// <summary>
		/// Compilation name or null if not specified.
		/// </summary>
		public string CompilationName { get; set; }

		/// <summary>
		/// Gets the emit options.
		/// </summary>
		public EmitOptions EmitOptions { get; set; }

		/// <summary>
		/// Name of the output file or null if not specified.
		/// </summary>
		public string OutputFileName { get; set; }

		/// <summary>
		/// Path of the PDB file or null if same as output binary path with .pdb extension.
		/// </summary>
		public string PdbPath { get; set; }

		/// <summary>
		/// Path of the file containing information linking the compilation to source server that stores 
		/// a snapshot of the source code included in the compilation.
		/// </summary>
		public string SourceLink { get; set; }

		/// <summary>
		/// Absolute path of the .ruleset file or null if not specified.
		/// </summary>
		public string RuleSetPath { get; set; }

		/// <summary>
		/// True to emit PDB information (to a standalone PDB file or embedded into the PE file).
		/// </summary>
		public bool EmitPdb { get; set; }

		/// <summary>
		/// Absolute path of the output directory.
		/// </summary>
		public string OutputDirectory { get; set; }

		/// <summary>
		/// An absolute path of the app.config file or null if not specified.
		/// </summary>
		public string AppConfigPath { get; set; }



		/// <summary>
		/// Errors while parsing the command line arguments.
		/// </summary>
		public Diagnostic[] Errors { get; set; }

		/// <summary>
		/// References to metadata supplied on the command line. 
		/// Includes assemblies specified via /r and netmodules specified via /addmodule.
		/// </summary>
		public CommandLineReference[] MetadataReferences { get; set; }

		/// <summary>
		/// References to analyzers supplied on the command line.
		/// </summary>
		public CommandLineAnalyzerReference[] AnalyzerReferences { get; set; }

		/// <summary>
		/// A set of additional non-code text files that can be used by analyzers.
		/// </summary>
		public CommandLineSourceFile[] AdditionalFiles { get; set; }

		/// <summary>
		/// A set of files to embed in the PDB.
		/// </summary>
		public CommandLineSourceFile[] EmbeddedFiles { get; set; }

		/// <value>
		/// Report additional information related to analyzers, such as analyzer execution time.
		/// </value>
		public bool ReportAnalyzer { get; set; }

		/// <summary>
		/// If true, prepend the command line header logo during 
		/// <see cref="CommonCompiler.Run"/>.
		/// </summary>
		public bool DisplayLogo { get; set; }

		/// <summary>
		/// If true, append the command line help during
		/// <see cref="CommonCompiler.Run"/>
		/// </summary>
		public bool DisplayHelp { get; set; }

		/// <summary>
		/// If true, append the compiler version during
		/// <see cref="CommonCompiler.Run"/>
		/// </summary>
		public bool DisplayVersion { get; set; }

		/// <summary>
		/// If true, prepend the compiler-supported language versions during
		/// <see cref="CommonCompiler.Run"/>
		/// </summary>
		public bool DisplayLangVersions { get; set; }

		/// <summary>
		/// The path to a Win32 resource.
		/// </summary>
		public string Win32ResourceFile { get; set; }

		/// <summary>
		/// The path to a .ico icon file.
		/// </summary>
		public string Win32Icon { get; set; }

		/// <summary>
		/// The path to a Win32 manifest file to embed
		/// into the output portable executable (PE) file.
		/// </summary>
		public string Win32Manifest { get; set; }

		/// <summary>
		/// If true, do not embed any Win32 manifest, including
		/// one specified by <see cref="Win32Manifest"/> or any
		/// default manifest.
		/// </summary>
		public bool NoWin32Manifest { get; set; }

		/// <summary>
		/// Resources specified as arguments to the compilation.
		/// </summary>
		public ResourceDescription[] ManifestResources { get; set; }

		/// <summary>
		/// Encoding to be used for source files or 'null' for autodetect/default.
		/// </summary>
		public Encoding Encoding { get; set; }

		/// <summary>
		/// Arguments following a script file or separator "--". Null if the command line parser is not interactive.
		/// </summary>
		public string[] ScriptArguments { get; set; }

		/// <summary>
		/// Source file paths.
		/// </summary>
		/// <remarks>
		/// Includes files specified directly on command line as well as files matching patterns specified 
		/// on command line using '*' and '?' wildcards or /recurse option.
		/// </remarks>
		public CommandLineSourceFile[] SourceFiles { get; set; }

		/// <summary>
		/// If true, prints the full path of the file containing errors or
		/// warnings in diagnostics.
		/// </summary>
		public bool PrintFullPaths { get; set; }

		/// <summary>
		/// Options to the <see cref="CommandLineParser"/>.
		/// </summary>
		/// <returns></returns>
		public ParseOptions ParseOptions
		{
			get { return ParseOptionsCore; }
		}

		/// <summary>
		/// Options to the <see cref="Compilation"/>.
		/// </summary>
		public CompilationOptions CompilationOptions
		{
			get { return CompilationOptionsCore; }
		}

		protected abstract ParseOptions ParseOptionsCore { get; }
		protected abstract CompilationOptions CompilationOptionsCore { get; }

		public StrongNameProvider GetStrongNameProvider(StrongNameFileSystem fileSystem) => throw new Exception("@@!!!");

		public CommandLineArguments()
		{
		}

		#region Metadata References

		/// <summary>
		/// Resolves metadata references stored in <see cref="MetadataReferences"/> using given file resolver and metadata provider.
		/// </summary>
		/// <param name="metadataResolver"><see cref="MetadataReferenceResolver"/> to use for assembly name and relative path resolution.</param>
		/// <returns>Yields resolved metadata references or <see cref="UnresolvedMetadataReference"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="metadataResolver"/> is null.</exception>
		public IEnumerable<MetadataReference> ResolveMetadataReferences(MetadataReferenceResolver metadataResolver)
		{
			if (metadataResolver == null)
			{
				throw new ArgumentNullException(nameof(metadataResolver));
			}

			return ResolveMetadataReferences(metadataResolver, diagnosticsOpt: null, messageProviderOpt: null);
		}

		/// <summary>
		/// Resolves metadata references stored in <see cref="MetadataReferences"/> using given file resolver and metadata provider.
		/// If a non-null diagnostic bag <paramref name="diagnosticsOpt"/> is provided, it catches exceptions that may be generated while reading the metadata file and
		/// reports appropriate diagnostics.
		/// Otherwise, if <paramref name="diagnosticsOpt"/> is null, the exceptions are unhandled.
		/// </summary>
		/// <remarks>
		/// called by CommonCompiler with diagnostics and message provider
		/// </remarks>
		public IEnumerable<MetadataReference> ResolveMetadataReferences(MetadataReferenceResolver metadataResolver, List<DiagnosticInfo> diagnosticsOpt, CommonMessageProvider messageProviderOpt)
		{
			Debug.Assert(metadataResolver != null);

			var resolved = new List<MetadataReference>();
			this.ResolveMetadataReferences(metadataResolver, diagnosticsOpt, messageProviderOpt, resolved);

			return resolved;
		}

		public virtual bool ResolveMetadataReferences(MetadataReferenceResolver metadataResolver, List<DiagnosticInfo> diagnosticsOpt, CommonMessageProvider messageProviderOpt, List<MetadataReference> resolved)
		{
			bool result = true;

			foreach (CommandLineReference cmdReference in MetadataReferences)
			{
				var references = ResolveMetadataReference(cmdReference, metadataResolver, diagnosticsOpt, messageProviderOpt);
				if (references.Length != 0)
				{
					resolved.AddRange(references);
				}
				else
				{
					result = false;
					if (diagnosticsOpt == null)
					{
						// no diagnostic, so leaved unresolved reference in list
						resolved.Add(new UnresolvedMetadataReference(cmdReference.Reference, cmdReference.Properties));
					}
				}
			}

			return result;
		}

		public static PortableExecutableReference[] ResolveMetadataReference(CommandLineReference cmdReference, MetadataReferenceResolver metadataResolver, List<DiagnosticInfo> diagnosticsOpt, CommonMessageProvider messageProviderOpt)
		{
			Debug.Assert(metadataResolver != null);
			Debug.Assert((diagnosticsOpt == null) == (messageProviderOpt == null));

			PortableExecutableReference[]references;
			try
			{
				references = metadataResolver.ResolveReference(cmdReference.Reference, baseFilePath: null, properties: cmdReference.Properties);
			}
			catch (Exception e) when (diagnosticsOpt != null && (e is BadImageFormatException || e is IOException))
			{
				var diagnostic = PortableExecutableReference.ExceptionToDiagnostic(e, messageProviderOpt, Location.None, cmdReference.Reference, cmdReference.Properties.Kind);
				diagnosticsOpt.Add(((DiagnosticWithInfo)diagnostic).Info);
				return new PortableExecutableReference[0];
			}

			if (references.Length == 0 && diagnosticsOpt != null)
			{
				diagnosticsOpt.Add(new DiagnosticInfo(messageProviderOpt, messageProviderOpt.ERR_MetadataFileNotFound, cmdReference.Reference));
				return new PortableExecutableReference[0];
			}

			return references;
		}

		#endregion
	}
}
