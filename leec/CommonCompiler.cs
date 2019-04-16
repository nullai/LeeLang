using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace leec
{
	public enum CompilationStage
	{
		Parse,
		Declare,
		Compile,
		Emit
	}
	public struct BuildPaths
	{
		/// <summary>
		/// The path which contains the compiler binaries and response files.
		/// </summary>
		public string ClientDirectory { get; }

		/// <summary>
		/// The path in which the compilation takes place.
		/// </summary>
		public string WorkingDirectory { get; }

		/// <summary>
		/// The path which contains mscorlib.  This can be null when specified by the user or running in a 
		/// CoreClr environment.
		/// </summary>
		public string SdkDirectory { get; }

		/// <summary>
		/// The temporary directory a compilation should use instead of <see cref="Path.GetTempPath"/>.  The latter
		/// relies on global state individual compilations should ignore.
		/// </summary>
		public string TempDirectory { get; }

		public BuildPaths(string clientDir, string workingDir, string sdkDir, string tempDir)
		{
			ClientDirectory = clientDir;
			WorkingDirectory = workingDir;
			SdkDirectory = sdkDir;
			TempDirectory = tempDir;
		}
	}
	public abstract class CommonCompiler
	{
		public const int Failed = 1;
		public const int Succeeded = 0;

		private readonly string _clientDirectory;

		public CommonMessageProvider MessageProvider { get; }
		public CommandLineArguments Arguments { get; }
		public abstract DiagnosticFormatter DiagnosticFormatter { get; }

		private readonly HashSet<Diagnostic> _reportedDiagnostics = new HashSet<Diagnostic>();

		public abstract Compilation CreateCompilation(TextWriter consoleOutput, ErrorLogger errorLoggerOpt);
		public abstract void PrintLogo(TextWriter consoleOutput);
		public abstract void PrintHelp(TextWriter consoleOutput);
		public abstract void PrintLangVersions(TextWriter consoleOutput);

		/// <summary>
		/// Print compiler version
		/// </summary>
		/// <param name="consoleOutput"></param>
		public virtual void PrintVersion(TextWriter consoleOutput)
		{
			consoleOutput.WriteLine(GetAssemblyFileVersion());
		}

		protected abstract bool TryGetCompilerDiagnosticCode(string diagnosticId, out uint code);
		public CommonCompiler(CommandLineParser parser, string responseFile, string[] args, BuildPaths buildPaths, string additionalReferenceDirectories)
		{
			IEnumerable<string> allArgs = args;
			_clientDirectory = buildPaths.ClientDirectory;

			Debug.Assert(null == responseFile || PathUtilities.IsAbsolute(responseFile));
			if (!SuppressDefaultResponseFile(args) && File.Exists(responseFile))
			{
				allArgs = new[] { "@" + responseFile }.Concat(allArgs);
			}

			this.Arguments = parser.Parse(allArgs, buildPaths.WorkingDirectory, buildPaths.SdkDirectory, additionalReferenceDirectories);
			this.MessageProvider = parser.MessageProvider;
		}

		public abstract bool SuppressDefaultResponseFile(IEnumerable<string> args);

		/// <summary>
		/// The type of the compiler class for version information in /help and /version.
		/// We don't simply use this.GetType() because that would break mock subclasses.
		/// </summary>
		public abstract Type Type { get; }

		/// <summary>
		/// The assembly file version of this compiler, used in logo and /version output.
		/// </summary>
		public virtual string GetAssemblyFileVersion()
		{
			Assembly assembly = this.GetType().Assembly;
			return GetAssemblyFileVersion(assembly);
		}

		public static string GetAssemblyFileVersion(Assembly assembly)
		{
			return assembly.GetName().Version.ToString();
		}

		public static string ExtractShortCommitHash(string hash)
		{
			// leave "<developer build>" alone, but truncate SHA to 8 characters
			if (hash != null && hash.Length >= 8 && hash[0] != '<')
			{
				return hash.Substring(0, 8);
			}

			return hash;
		}

		/// <summary>
		/// Tool name used, along with assembly version, for error logging.
		/// </summary>
		public abstract string GetToolName();

		/// <summary>
		/// Tool version identifier used for error logging.
		/// </summary>
		public Version GetAssemblyVersion()
		{
			return this.GetType().Assembly.GetName().Version;
		}

		public virtual Func<string, MetadataReferenceProperties, PortableExecutableReference> GetMetadataProvider()
		{
			return (path, properties) => MetadataReference.CreateFromFile(path, properties);
		}

		public virtual MetadataReferenceResolver GetCommandLineMetadataReferenceResolver()
		{
			var pathResolver = new RelativePathResolver(Arguments.ReferencePaths, Arguments.BaseDirectory);
			return new LoggingMetadataFileReferenceResolver(pathResolver, GetMetadataProvider());
		}

		/// <summary>
		/// Resolves metadata references stored in command line arguments and reports errors for those that can't be resolved.
		/// </summary>
		public List<MetadataReference> ResolveMetadataReferences(
			List<DiagnosticInfo> diagnostics,
			out MetadataReferenceResolver referenceDirectiveResolver)
		{
			var commandLineReferenceResolver = GetCommandLineMetadataReferenceResolver();

			List<MetadataReference> resolved = new List<MetadataReference>();
			Arguments.ResolveMetadataReferences(commandLineReferenceResolver, diagnostics, this.MessageProvider, resolved);

			// when compiling into an assembly (csc/vbc) we only allow #r that match references given on command line:
			referenceDirectiveResolver = new ExistingReferencesResolver(commandLineReferenceResolver, resolved.ToArray());

			return resolved;
		}

		/// <summary>
		/// Reads content of a source file.
		/// </summary>
		/// <param name="file">Source file information.</param>
		/// <param name="diagnostics">Storage for diagnostics.</param>
		/// <returns>File content or null on failure.</returns>
		public SourceText TryReadFileContent(CommandLineSourceFile file, IList<DiagnosticInfo> diagnostics)
		{
			string discarded;
			return TryReadFileContent(file, diagnostics, out discarded);
		}

		/// <summary>
		/// Reads content of a source file.
		/// </summary>
		/// <param name="file">Source file information.</param>
		/// <param name="diagnostics">Storage for diagnostics.</param>
		/// <param name="normalizedFilePath">If given <paramref name="file"/> opens successfully, set to normalized absolute path of the file, null otherwise.</param>
		/// <returns>File content or null on failure.</returns>
		public SourceText TryReadFileContent(CommandLineSourceFile file, IList<DiagnosticInfo> diagnostics, out string normalizedFilePath)
		{
			var filePath = file.Path;
			try
			{
				using (var data = OpenFileForReadWithSmallBufferOptimization(filePath))
				{
					normalizedFilePath = data.Name;
					return EncodedStringText.Create(data, Arguments.Encoding);
				}
			}
			catch (Exception e)
			{
				diagnostics.Add(ToFileReadDiagnostics(this.MessageProvider, e, filePath));
				normalizedFilePath = null;
				return null;
			}
		}

		private static FileStream OpenFileForReadWithSmallBufferOptimization(string filePath)
		{
			// PERF: Using a very small buffer size for the FileStream opens up an optimization within EncodedStringText/EmbeddedText where
			// we read the entire FileStream into a byte array in one shot. For files that are actually smaller than the buffer
			// size, FileStream.Read still allocates the public buffer.
			return new FileStream(
				filePath,
				FileMode.Open,
				FileAccess.Read,
				FileShare.ReadWrite,
				bufferSize: 1,
				options: FileOptions.None);
		}

		public static DiagnosticInfo ToFileReadDiagnostics(CommonMessageProvider messageProvider, Exception e, string filePath)
		{
			DiagnosticInfo diagnosticInfo;

			if (e is FileNotFoundException || e is DirectoryNotFoundException)
			{
				diagnosticInfo = new DiagnosticInfo(messageProvider, messageProvider.ERR_FileNotFound, filePath);
			}
			else if (e is InvalidDataException)
			{
				diagnosticInfo = new DiagnosticInfo(messageProvider, messageProvider.ERR_BinaryFile, filePath);
			}
			else
			{
				diagnosticInfo = new DiagnosticInfo(messageProvider, messageProvider.ERR_NoSourceFile, filePath, e.Message);
			}

			return diagnosticInfo;
		}

		public bool ReportErrors(IEnumerable<Diagnostic> diagnostics, TextWriter consoleOutput, ErrorLogger errorLoggerOpt)
		{
			bool hasErrors = false;
			foreach (var diag in diagnostics)
			{
				if (_reportedDiagnostics.Contains(diag))
				{
					// TODO: This invariant fails (at least) in the case where we see a member declaration "x = 1;".
					// First we attempt to parse a member declaration starting at "x".  When we see the "=", we
					// create an IncompleteMemberSyntax with return type "x" and an error at the location of the "x".
					// Then we parse a member declaration starting at "=".  This is an invalid member declaration start
					// so we attach an error to the "=" and attach it (plus following tokens) to the IncompleteMemberSyntax
					// we previously created.
					//this assert isn't valid if we change the design to not bail out after each phase.
					//System.Diagnostics.Debug.Assert(diag.Severity != DiagnosticSeverity.Error);
					continue;
				}
				else if (diag.Severity == DiagnosticSeverity.Hidden)
				{
					// Not reported from the command-line compiler.
					continue;
				}

				// We want to report diagnostics with source suppression in the error log file.
				// However, these diagnostics should not be reported on the console output.
				errorLoggerOpt?.LogDiagnostic(diag);
				if (diag.IsSuppressed)
				{
					continue;
				}

				// Diagnostics that aren't suppressed will be reported to the console output and, if they are errors,
				// they should fail the run
				if (diag.Severity == DiagnosticSeverity.Error)
				{
					hasErrors = true;
				}

				PrintError(diag, consoleOutput);

				_reportedDiagnostics.Add(diag);
			}

			return hasErrors;
		}

		private bool ReportErrors(DiagnosticBag diagnostics, TextWriter consoleOutput, ErrorLogger errorLoggerOpt)
			=> ReportErrors(diagnostics.ToReadOnly(), consoleOutput, errorLoggerOpt);

		/// <summary>
		/// Returns true if the diagnostic is an error that should be reported.
		/// </summary>
		private static bool IsReportedError(Diagnostic diagnostic)
		{
			return (diagnostic.Severity == DiagnosticSeverity.Error) && !diagnostic.IsSuppressed;
		}

		public bool ReportErrors(IEnumerable<DiagnosticInfo> diagnostics, TextWriter consoleOutput, ErrorLogger errorLoggerOpt) =>
			ReportErrors(diagnostics.Select(info => Diagnostic.Create(info)), consoleOutput, errorLoggerOpt);

		protected virtual void PrintError(Diagnostic diagnostic, TextWriter consoleOutput)
		{
			consoleOutput.WriteLine(DiagnosticFormatter.Format(diagnostic));
		}

		/// <summary>
		/// csc.exe and vbc.exe entry point.
		/// </summary>
		public virtual int Run(TextWriter consoleOutput, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				return RunCore(consoleOutput, null, cancellationToken);
			}
			catch (OperationCanceledException)
			{
				var errorCode = MessageProvider.ERR_CompileCancelled;
				if (errorCode > 0)
				{
					var diag = new DiagnosticInfo(MessageProvider, errorCode);
					ReportErrors(new[] { diag }, consoleOutput, null);
				}

				return Failed;
			}
		}

		private int RunCore(TextWriter consoleOutput, ErrorLogger errorLogger, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (Arguments.DisplayVersion)
			{
				PrintVersion(consoleOutput);
				return Succeeded;
			}

			if (Arguments.DisplayLangVersions)
			{
				PrintLangVersions(consoleOutput);
				return Succeeded;
			}

			if (Arguments.DisplayLogo)
			{
				PrintLogo(consoleOutput);
			}

			if (Arguments.DisplayHelp)
			{
				PrintHelp(consoleOutput);
				return Succeeded;
			}

			if (ReportErrors(Arguments.Errors, consoleOutput, errorLogger))
			{
				return Failed;
			}

			Compilation compilation = CreateCompilation(consoleOutput, errorLogger);
			if (compilation == null)
			{
				return Failed;
			}

			var diagnosticInfos = new List<DiagnosticInfo>();
			var additionalTextFiles = ResolveAdditionalFilesFromArguments(diagnosticInfos, MessageProvider);
			if (ReportErrors(diagnosticInfos, consoleOutput, errorLogger))
			{
				return Failed;
			}

			var diagnostics = DiagnosticBag.GetInstance();
			
			CompileAndEmit(
				ref compilation,
				additionalTextFiles,
				diagnostics,
				cancellationToken);

			var exitCode = ReportErrors(diagnostics, consoleOutput, errorLogger)
				? Failed
				: Succeeded;

			// The act of reporting errors can cause more errors to appear in
			// additional files due to forcing all additional files to fetch text
			foreach (var additionalFile in additionalTextFiles)
			{
				if (ReportErrors(additionalFile.Diagnostics, consoleOutput, errorLogger))
				{
					exitCode = Failed;
				}
			}

			diagnostics.Free();

			return exitCode;
		}

		/// <summary>
		/// Perform all the work associated with actual compilation
		/// (parsing, binding, compile, emit), resulting in diagnostics
		/// and analyzer output.
		/// </summary>
		private void CompileAndEmit(
			ref Compilation compilation,
			AdditionalTextFile[] additionalTextFiles,
			DiagnosticBag diagnostics,
			CancellationToken cancellationToken)
		{
			// Print the diagnostics produced during the parsing stage and exit if there were any errors.
			compilation.GetDiagnostics(CompilationStage.Parse, false, diagnostics, cancellationToken);
			if (diagnostics.HasAnyErrors())
			{
				return;
			}

			DiagnosticBag analyzerExceptionDiagnostics = null;

			compilation.GetDiagnostics(CompilationStage.Declare, false, diagnostics, cancellationToken);
			if (diagnostics.HasAnyErrors())
			{
				return;
			}

			cancellationToken.ThrowIfCancellationRequested();

			string outputName = GetOutputFileName(compilation, cancellationToken);
			var finalPeFilePath = Path.Combine(Arguments.OutputDirectory, outputName);
			var finalPdbFilePath = Arguments.PdbPath ?? Path.ChangeExtension(finalPeFilePath, ".pdb");

			NoThrowStreamDisposer sourceLinkStreamDisposerOpt = null;

			try
			{
				// NOTE: Unlike the PDB path, the XML doc path is not embedded in the assembly, so we don't need to pass it to emit.
				var emitOptions = Arguments.EmitOptions;
				emitOptions.OutputNameOverride = outputName;
				emitOptions.PdbFilePath = PathUtilities.NormalizePathPrefix(finalPdbFilePath, Arguments.PathMap);
				
				if (!string.IsNullOrEmpty(emitOptions.PdbFilePath))
				{
					emitOptions.PdbFilePath = Path.GetFileName(emitOptions.PdbFilePath);
				}

				if (Arguments.SourceLink != null)
				{
					var sourceLinkStreamOpt = OpenFile(
						Arguments.SourceLink,
						diagnostics,
						FileMode.Open,
						FileAccess.Read,
						FileShare.Read);

					if (sourceLinkStreamOpt != null)
					{
						sourceLinkStreamDisposerOpt = new NoThrowStreamDisposer(
							sourceLinkStreamOpt,
							Arguments.SourceLink,
							diagnostics,
							MessageProvider);
					}
				}

				var moduleBeingBuilt = compilation.CheckOptionsAndCreateModuleBuilder(
					diagnostics,
					Arguments.ManifestResources,
					emitOptions,
					debugEntryPoint: null,
					sourceLinkStream: sourceLinkStreamDisposerOpt?.Stream,
					cancellationToken: cancellationToken);

				if (moduleBeingBuilt != null)
				{
					bool success;

					try
					{
						success = compilation.CompileMethods(
							moduleBeingBuilt,
							Arguments.EmitPdb,
							emitOptions.EmitMetadataOnly,
							diagnostics,
							filterOpt: null,
							cancellationToken: cancellationToken);

						if (success)
						{
							using (var win32ResourceStreamOpt = GetWin32Resources(MessageProvider, Arguments, compilation, diagnostics))
							{
								if (diagnostics.HasAnyErrors())
								{
									return;
								}

								success = compilation.GenerateResourcesAndDocumentationComments(
									moduleBeingBuilt,
									win32ResourceStreamOpt,
									emitOptions.OutputNameOverride,
									diagnostics,
									cancellationToken);
							}

							// only report unused usings if we have success.
							if (success)
							{
								compilation.ReportUnusedImports(null, diagnostics, cancellationToken);
							}
						}

						compilation.CompleteTrees(null);
					}
					finally
					{
						moduleBeingBuilt.CompilationFinished();
					}

					if (success)
					{
						var peStreamProvider = new CompilerEmitStreamProvider(this, finalPeFilePath);

						success = compilation.SerializeToPeStream(
							moduleBeingBuilt,
							peStreamProvider,
							null,
							null,
							diagnostics: diagnostics,
							metadataOnly: emitOptions.EmitMetadataOnly,
							includePrivateMembers: emitOptions.IncludePrivateMembers,
							pePdbFilePath: emitOptions.PdbFilePath,
							cancellationToken: cancellationToken);

						peStreamProvider.Close(diagnostics);
					}
				}

				if (diagnostics.HasAnyErrors())
				{
					return;
				}
			}
			finally
			{
				sourceLinkStreamDisposerOpt?.Dispose();
			}

			if (sourceLinkStreamDisposerOpt?.HasFailedToDispose == true)
			{
				return;
			}

			cancellationToken.ThrowIfCancellationRequested();

			if (analyzerExceptionDiagnostics != null)
			{
				diagnostics.AddRange(analyzerExceptionDiagnostics);
				if (analyzerExceptionDiagnostics.HasAnyErrors())
				{
					return;
				}
			}

			cancellationToken.ThrowIfCancellationRequested();
		}

		protected virtual AdditionalTextFile[] ResolveAdditionalFilesFromArguments(List<DiagnosticInfo> diagnostics, CommonMessageProvider messageProvider)
		{
			var builder = new List<AdditionalTextFile>();

			foreach (var file in Arguments.AdditionalFiles)
			{
				builder.Add(new AdditionalTextFile(file, this));
			}

			return builder.ToArray();
		}

		/// <summary>
		/// Given a compilation and a destination directory, determine three names:
		///   1) The name with which the assembly should be output (default = null, which indicates that the compilation output name should be used).
		///   2) The path of the assembly/module file (default = destination directory + compilation output name).
		///   3) The path of the pdb file (default = assembly/module path with ".pdb" extension).
		/// </summary>
		/// <remarks>
		/// C# has a special implementation that implements idiosyncratic behavior of csc.
		/// </remarks>
		protected virtual string GetOutputFileName(Compilation compilation, CancellationToken cancellationToken)
		{
			return Arguments.OutputFileName;
		}

		/// <summary>
		/// Test hook for intercepting File.Open.
		/// </summary>
		public Func<string, FileMode, FileAccess, FileShare, Stream> FileOpen
		{
			get { return _fileOpen ?? ((path, mode, access, share) => new FileStream(path, mode, access, share)); }
			set { _fileOpen = value; }
		}
		private Func<string, FileMode, FileAccess, FileShare, Stream> _fileOpen;

		private Stream OpenFile(
			string filePath,
			DiagnosticBag diagnostics,
			FileMode mode = FileMode.Open,
			FileAccess access = FileAccess.ReadWrite,
			FileShare share = FileShare.None)
		{
			try
			{
				return FileOpen(filePath, mode, access, share);
			}
			catch (Exception e)
			{
				MessageProvider.ReportStreamWriteException(e, filePath, diagnostics);
				return null;
			}
		}

		// public for testing
		public static Stream GetWin32ResourcesInternal(
			CommonMessageProvider messageProvider,
			CommandLineArguments arguments,
			Compilation compilation,
			out IEnumerable<DiagnosticInfo> errors)
		{
			var diagnostics = DiagnosticBag.GetInstance();
			var stream = GetWin32Resources(messageProvider, arguments, compilation, diagnostics);
			var s = diagnostics.ToReadOnlyAndFree();
			var r = new DiagnosticInfo[s.Length];
			for (int i = 0; i < s.Length; i++)
				r[i] = new DiagnosticInfo(messageProvider, s[i].IsWarningAsError, s[i].Code, (object[])s[i].Arguments);
			errors = r;
			return stream;
		}

		private static Stream GetWin32Resources(
			CommonMessageProvider messageProvider,
			CommandLineArguments arguments,
			Compilation compilation,
			DiagnosticBag diagnostics)
		{
			if (arguments.Win32ResourceFile != null)
			{
				return OpenStream(messageProvider, arguments.Win32ResourceFile, arguments.BaseDirectory, messageProvider.ERR_CantOpenWin32Resource, diagnostics);
			}

			using (Stream manifestStream = OpenManifestStream(messageProvider, compilation.Options.OutputKind, arguments, diagnostics))
			{
				using (Stream iconStream = OpenStream(messageProvider, arguments.Win32Icon, arguments.BaseDirectory, messageProvider.ERR_CantOpenWin32Icon, diagnostics))
				{
					try
					{
						return compilation.CreateDefaultWin32Resources(true, arguments.NoWin32Manifest, manifestStream, iconStream);
					}
					catch (Exception ex)
					{
						diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_ErrorBuildingWin32Resource, Location.None, ex.Message));
					}
				}
			}

			return null;
		}

		private static Stream OpenManifestStream(CommonMessageProvider messageProvider, OutputKind outputKind, CommandLineArguments arguments, DiagnosticBag diagnostics)
		{
			return outputKind.IsNetModule()
				? null
				: OpenStream(messageProvider, arguments.Win32Manifest, arguments.BaseDirectory, messageProvider.ERR_CantOpenWin32Manifest, diagnostics);
		}

		private static Stream OpenStream(CommonMessageProvider messageProvider, string path, string baseDirectory, int errorCode, DiagnosticBag diagnostics)
		{
			if (path == null)
			{
				return null;
			}

			string fullPath = ResolveRelativePath(messageProvider, path, baseDirectory, diagnostics);
			if (fullPath == null)
			{
				return null;
			}

			try
			{
				return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
			}
			catch (Exception ex)
			{
				diagnostics.Add(messageProvider.CreateDiagnostic(errorCode, Location.None, fullPath, ex.Message));
			}

			return null;
		}

		private static string ResolveRelativePath(CommonMessageProvider messageProvider, string path, string baseDirectory, DiagnosticBag diagnostics)
		{
			string fullPath = FileUtilities.ResolveRelativePath(path, baseDirectory);
			if (fullPath == null)
			{
				diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.FTL_InvalidInputFileName, Location.None, path));
			}

			return fullPath;
		}

		public static bool TryGetCompilerDiagnosticCode(string diagnosticId, string expectedPrefix, out uint code)
		{
			code = 0;
			return diagnosticId.StartsWith(expectedPrefix, StringComparison.Ordinal) && uint.TryParse(diagnosticId.Substring(expectedPrefix.Length), out code);
		}
	}
}
