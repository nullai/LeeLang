using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace leec
{
	public sealed class AdditionalTextFile : AdditionalText
	{
		private readonly CommandLineSourceFile _sourceFile;
		private readonly CommonCompiler _compiler;
		private SourceText _text;
		private IList<DiagnosticInfo> _diagnostics;

		private readonly object _lockObject = new object();

		public AdditionalTextFile(CommandLineSourceFile sourceFile, CommonCompiler compiler)
		{
			if (compiler == null)
			{
				throw new ArgumentNullException(nameof(compiler));
			}

			_sourceFile = sourceFile;
			_compiler = compiler;
			_diagnostics = new List<DiagnosticInfo>();
		}

		/// <summary>
		/// Path to the file.
		/// </summary>
		public override string Path => _sourceFile.Path;

		/// <summary>
		/// Returns a <see cref="SourceText"/> with the contents of this file, or <c>null</c> if
		/// there were errors reading the file.
		/// </summary>
		public override SourceText GetText(CancellationToken cancellationToken = default(CancellationToken))
		{
			lock (_lockObject)
			{
				if (_text == null)
				{
					var diagnostics = new List<DiagnosticInfo>();
					_text = _compiler.TryReadFileContent(_sourceFile, diagnostics);
					_diagnostics = diagnostics;
				}
			}

			return _text;
		}

		/// <summary>
		/// Errors encountered when trying to read the additional file. Always empty if
		/// <see cref="GetText(CancellationToken)"/> has not been called.
		/// </summary>
		public IList<DiagnosticInfo> Diagnostics => _diagnostics;
	}
}
