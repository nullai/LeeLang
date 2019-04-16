using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace leec
{
	public abstract class LeeSyntaxTree : SyntaxTree
	{
		/// <summary>
		/// The options used by the parser to produce the syntax tree.
		/// </summary>
		public new abstract ParseOptions Options { get; }

		/// <summary>
		/// Gets the root node of the syntax tree.
		/// </summary>
		public new abstract LeeSyntaxNode GetRoot(CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Gets the root node of the syntax tree if it is already available.
		/// </summary>
		public abstract bool TryGetRoot(out LeeSyntaxNode root);
		
		/// <summary>
		/// Gets the root of the syntax tree statically typed as <see cref="CompilationUnitSyntax"/>.
		/// </summary>
		/// <remarks>
		/// Ensure that <see cref="SyntaxTree.HasCompilationUnitRoot"/> is true for this tree prior to invoking this method.
		/// </remarks>
		/// <exception cref="InvalidCastException">Throws this exception if <see cref="SyntaxTree.HasCompilationUnitRoot"/> is false.</exception>
		public CompilationUnitSyntax GetCompilationUnitRoot(CancellationToken cancellationToken = default(CancellationToken))
		{
			return (CompilationUnitSyntax)this.GetRoot(cancellationToken);
		}


		#region Preprocessor Symbols
		private bool _hasDirectives;
		private DirectiveStack _directives;

		public void SetDirectiveStack(DirectiveStack directives)
		{
			_directives = directives;
			_hasDirectives = true;
		}

		private DirectiveStack GetDirectives()
		{
			if (!_hasDirectives)
			{
				var stack = this.GetRoot().ApplyDirectives(default(DirectiveStack));
				SetDirectiveStack(stack);
			}

			return _directives;
		}

		public bool IsAnyPreprocessorSymbolDefined(string[] conditionalSymbols)
		{
			Debug.Assert(conditionalSymbols != null);

			foreach (string conditionalSymbol in conditionalSymbols)
			{
				if (IsPreprocessorSymbolDefined(conditionalSymbol))
				{
					return true;
				}
			}

			return false;
		}

		public bool IsPreprocessorSymbolDefined(string symbolName)
		{
			return IsPreprocessorSymbolDefined(GetDirectives(), symbolName);
		}

		private bool IsPreprocessorSymbolDefined(DirectiveStack directives, string symbolName)
		{
			switch (directives.IsDefined(symbolName))
			{
				case DefineState.Defined:
					return true;
				case DefineState.Undefined:
					return false;
				default:
					return this.Options.PreprocessorSymbols.Contains(symbolName);
			}
		}
		
		#endregion

		#region Factories
		
		/// <summary>
		/// Produces a syntax tree by parsing the source text.
		/// </summary>
		public static LeeSyntaxTree ParseText(string text, ParseOptions options, string path = "")
		{
			return ParseText(SourceText.From(text), options, path);
		}

		public static LeeSyntaxTree ParseText(SourceText text, ParseOptions options, string path = "")
		{
			if (text == null)
			{
				throw new ArgumentNullException(nameof(text));
			}

			using (var lexer = new Lexer(text, options))
			{
				using (var parser = new LanguageParser(lexer))
				{
					var compilationUnit = (CompilationUnitSyntax)parser.ParseCompilationUnit();
					var tree = new ParsedSyntaxTree(text, path, options, compilationUnit, parser.Directives);
					return tree;
				}
			}
		}

		#endregion
		
		#region LinePositions and Locations

		/// <summary>
		/// Gets the location in terms of path, line and column for a given span.
		/// </summary>
		/// <param name="span">Span within the tree.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// <see cref="FileLinePositionSpan"/> that contains path, line and column information.
		/// </returns>
		/// <remarks>The values are not affected by line mapping directives (<c>#line</c>).</remarks>
		public override FileLinePositionSpan GetLineSpan(TextSpan span, CancellationToken cancellationToken = default(CancellationToken))
		{
			return new FileLinePositionSpan(this.FilePath, GetLinePosition(span.Start), GetLinePosition(span.End));
		}

		/// <summary>
		/// Gets the location in terms of path, line and column after applying source line mapping directives (<c>#line</c>). 
		/// </summary>
		/// <param name="span">Span within the tree.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// <para>A valid <see cref="FileLinePositionSpan"/> that contains path, line and column information.</para>
		/// <para>
		/// If the location path is mapped the resulting path is the path specified in the corresponding <c>#line</c>,
		/// otherwise it's <see cref="SyntaxTree.FilePath"/>.
		/// </para>
		/// <para>
		/// A location path is considered mapped if the first <c>#line</c> directive that precedes it and that
		/// either specifies an explicit file path or is <c>#line default</c> exists and specifies an explicit path.
		/// </para>
		/// </returns>
		public override FileLinePositionSpan GetMappedLineSpan(TextSpan span, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (_lazyLineDirectiveMap == null)
			{
				// Create the line directive map on demand.
				Interlocked.CompareExchange(ref _lazyLineDirectiveMap, new LeeLineDirectiveMap(this), null);
			}

			return _lazyLineDirectiveMap.TranslateSpan(this.GetText(cancellationToken), this.FilePath, span);
		}

		public override LineVisibility GetLineVisibility(int position, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (_lazyLineDirectiveMap == null)
			{
				// Create the line directive map on demand.
				Interlocked.CompareExchange(ref _lazyLineDirectiveMap, new LeeLineDirectiveMap(this), null);
			}

			return _lazyLineDirectiveMap.GetLineVisibility(this.GetText(cancellationToken), position);
		}

		/// <summary>
		/// Gets a <see cref="FileLinePositionSpan"/> for a <see cref="TextSpan"/>. FileLinePositionSpans are used
		/// primarily for diagnostics and source locations.
		/// </summary>
		/// <param name="span">The source <see cref="TextSpan" /> to convert.</param>
		/// <param name="isHiddenPosition">When the method returns, contains a boolean value indicating whether this span is considered hidden or not.</param>
		/// <returns>A resulting <see cref="FileLinePositionSpan"/>.</returns>
		public override FileLinePositionSpan GetMappedLineSpanAndVisibility(TextSpan span, out bool isHiddenPosition)
		{
			if (_lazyLineDirectiveMap == null)
			{
				// Create the line directive map on demand.
				Interlocked.CompareExchange(ref _lazyLineDirectiveMap, new LeeLineDirectiveMap(this), null);
			}

			return _lazyLineDirectiveMap.TranslateSpanAndVisibility(this.GetText(), this.FilePath, span, out isHiddenPosition);
		}

		/// <summary>
		/// Given the error code and the source location, get the warning state based on <c>#pragma warning</c> directives.
		/// </summary>
		/// <param name="id">Error code.</param>
		/// <param name="position">Source location.</param>
		public PragmaWarningState GetPragmaDirectiveWarningState(string id, int position)
		{
			if (_lazyPragmaWarningStateMap == null)
			{
				// Create the warning state map on demand.
				Interlocked.CompareExchange(ref _lazyPragmaWarningStateMap, new LeePragmaWarningStateMap(this, IsGeneratedCode()), null);
			}

			return _lazyPragmaWarningStateMap.GetWarningState(id, position);
		}

		/// <summary>
		/// Returns true if the `#nullable` directive preceding the position is
		/// `enable` or `safeonly`, false if `disable`, and null if no preceding directive,
		/// or directive preceding the position is `restore`.
		/// </summary>
		public bool? GetNullableDirectiveState(int position)
		{
			if (_lazyNullableDirectiveMap == null)
			{
				// Create the #nullable directive map on demand.
				Interlocked.CompareExchange(ref _lazyNullableDirectiveMap, NullableDirectiveMap.Create(this, IsGeneratedCode()), null);
			}

			return _lazyNullableDirectiveMap.GetDirectiveState(position);
		}

		public bool IsGeneratedCode()
		{
			return false;
		}

		private LeeLineDirectiveMap _lazyLineDirectiveMap;
		private LeePragmaWarningStateMap _lazyPragmaWarningStateMap;
		private NullableDirectiveMap _lazyNullableDirectiveMap;

		private LinePosition GetLinePosition(int position)
		{
			throw new Exception();
			//return this.GetText().Lines.GetLinePosition(position);
		}

		/// <summary>
		/// Gets a <see cref="Location"/> for the specified text <paramref name="span"/>.
		/// </summary>
		public override Location GetLocation(TextSpan span)
		{
			return new SourceLocation(this, span);
		}

		#endregion

		#region Diagnostics

		/// <summary>
		/// Gets a list of all the diagnostics in the sub tree that has the specified node as its root.
		/// </summary>
		/// <remarks>
		/// This method does not filter diagnostics based on <c>#pragma</c>s and compiler options
		/// like /nowarn, /warnaserror etc.
		/// </remarks>
		public override IEnumerable<Diagnostic> GetDiagnostics(GreenNode node)
		{
			if (node == null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			return GetDiagnostics(node, node.Position);
		}

		private IEnumerable<Diagnostic> GetDiagnostics(GreenNode greenNode, int position)
		{
			if (greenNode == null)
			{
				throw new InvalidOperationException();
			}

			if (greenNode.ContainsDiagnostics)
			{
				return EnumerateDiagnostics(greenNode, position);
			}

			return new Diagnostic[0];
		}

		private IEnumerable<Diagnostic> EnumerateDiagnostics(GreenNode node, int position)
		{
			throw new Exception();
			/*
			var enumerator = new SyntaxTreeDiagnosticEnumerator(this, node, position);
			while (enumerator.MoveNext())
			{
				yield return enumerator.Current;
			}*/
		}

		/// <summary>
		/// Gets a list of all the diagnostics associated with the token and any related trivia.
		/// </summary>
		/// <remarks>
		/// This method does not filter diagnostics based on <c>#pragma</c>s and compiler options
		/// like /nowarn, /warnaserror etc.
		/// </remarks>
		public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxToken token)
		{
			return GetDiagnostics(token, token.Position);
		}

		/// <summary>
		/// Gets a list of all the diagnostics associated with the trivia.
		/// </summary>
		/// <remarks>
		/// This method does not filter diagnostics based on <c>#pragma</c>s and compiler options
		/// like /nowarn, /warnaserror etc.
		/// </remarks>
		public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxTrivia trivia)
		{
			return GetDiagnostics(trivia, trivia.Position);
		}

		/// <summary>
		/// Gets a list of all the diagnostics in the syntax tree.
		/// </summary>
		/// <remarks>
		/// This method does not filter diagnostics based on <c>#pragma</c>s and compiler options
		/// like /nowarn, /warnaserror etc.
		/// </remarks>
		public override IEnumerable<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default(CancellationToken))
		{
			return this.GetDiagnostics(this.GetRoot(cancellationToken));
		}

		#endregion

		#region SyntaxTree

		protected override GreenNode GetRootCore(CancellationToken cancellationToken)
		{
			return this.GetRoot(cancellationToken);
		}

		protected override ParseOptions OptionsCore
		{
			get
			{
				return this.Options;
			}
		}

		#endregion
	}
}
