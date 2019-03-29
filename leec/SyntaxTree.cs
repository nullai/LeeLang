using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace leec
{
	public abstract class SyntaxTree
	{
		/// <summary>
		/// The path of the source document file.
		/// </summary>
		/// <remarks>
		/// If this syntax tree is not associated with a file, this value can be empty.
		/// The path shall not be null.
		/// 
		/// The file doesn't need to exist on disk. The path is opaque to the compiler.
		/// The only requirement on the path format is that the implementations of 
		/// <see cref="SourceReferenceResolver"/>, <see cref="XmlReferenceResolver"/> and <see cref="MetadataReferenceResolver"/> 
		/// passed to the compilation that contains the tree understand it.
		/// 
		/// Clients must also not assume that the values of this property are unique
		/// within a Compilation.
		/// 
		/// The path is used as follows:
		///    - When debug information is emitted, this path is embedded in the debug information.
		///    - When resolving and normalizing relative paths in #r, #load, #line/#ExternalSource, 
		///      #pragma checksum, #ExternalChecksum directives, XML doc comment include elements, etc.
		/// </remarks>
		public abstract string FilePath { get; }

		/// <summary>
		/// Returns true if this syntax tree has a root with SyntaxKind "CompilationUnit".
		/// </summary>
		public abstract bool HasCompilationUnitRoot { get; }

		public ParseOptions Options
		{
			get
			{
				return this.OptionsCore;
			}
		}

		protected abstract ParseOptions OptionsCore { get; }

		/// <summary>
		/// The length of the text of the syntax tree.
		/// </summary>
		public abstract int Length { get; }

		/// <summary>
		/// Gets the syntax tree's text if it is available.
		/// </summary>
		public abstract bool TryGetText(out SourceText text);

		/// <summary>
		/// Gets the text of the source document.
		/// </summary>
		public abstract SourceText GetText(CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// The text encoding of the source document.
		/// </summary>
		public abstract Encoding Encoding { get; }

		/// <summary>
		/// Gets the root of the syntax tree if it is available.
		/// </summary>
		public bool TryGetRoot(out GreenNode root)
		{
			return TryGetRootCore(out root);
		}

		/// <summary>
		/// Gets the root of the syntax tree if it is available.
		/// </summary>
		protected abstract bool TryGetRootCore(out GreenNode root);

		/// <summary>
		/// Gets the root node of the syntax tree, causing computation if necessary.
		/// </summary>
		public GreenNode GetRoot(CancellationToken cancellationToken = default(CancellationToken))
		{
			return GetRootCore(cancellationToken);
		}

		/// <summary>
		/// Gets the root node of the syntax tree, causing computation if necessary.
		/// </summary>
		protected abstract GreenNode GetRootCore(CancellationToken cancellationToken);

		/// <summary>
		/// Gets a list of all the diagnostics in the syntax tree.
		/// This method does not filter diagnostics based on #pragmas and compiler options
		/// like nowarn, warnaserror etc.
		/// </summary>
		public abstract IEnumerable<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Gets a list of all the diagnostics in the sub tree that has the specified node as its root.
		/// This method does not filter diagnostics based on #pragmas and compiler options
		/// like nowarn, warnaserror etc.
		/// </summary>
		public abstract IEnumerable<Diagnostic> GetDiagnostics(GreenNode node);

		/// <summary>
		/// Gets a list of all the diagnostics associated with the token and any related trivia.
		/// This method does not filter diagnostics based on #pragmas and compiler options
		/// like nowarn, warnaserror etc.
		/// </summary>
		public abstract IEnumerable<Diagnostic> GetDiagnostics(SyntaxToken token);

		/// <summary>
		/// Gets a list of all the diagnostics associated with the trivia.
		/// This method does not filter diagnostics based on #pragmas and compiler options
		/// like nowarn, warnaserror etc.
		/// </summary>
		public abstract IEnumerable<Diagnostic> GetDiagnostics(SyntaxTrivia trivia);

		/// <summary>
		/// Gets the location in terms of path, line and column for a given span.
		/// </summary>
		/// <param name="span">Span within the tree.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// A valid <see cref="FileLinePositionSpan"/> that contains path, line and column information.
		/// The values are not affected by line mapping directives (<c>#line</c>).
		/// </returns>
		public abstract FileLinePositionSpan GetLineSpan(TextSpan span, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Gets the location in terms of path, line and column after applying source line mapping directives 
		/// (<c>#line</c> in C# or <c>#ExternalSource</c> in VB). 
		/// </summary>
		/// <param name="span">Span within the tree.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>
		/// A valid <see cref="FileLinePositionSpan"/> that contains path, line and column information.
		/// 
		/// If the location path is mapped the resulting path is the path specified in the corresponding <c>#line</c>,
		/// otherwise it's <see cref="SyntaxTree.FilePath"/>.
		/// 
		/// A location path is considered mapped if the first <c>#line</c> directive that precedes it and that 
		/// either specifies an explicit file path or is <c>#line default</c> exists and specifies an explicit path.
		/// </returns>
		public abstract FileLinePositionSpan GetMappedLineSpan(TextSpan span, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Returns the visibility for the line at the given position.
		/// </summary>
		/// <param name="position">The position to check.</param>
		/// <param name="cancellationToken">The cancellation token.</param> 
		public virtual LineVisibility GetLineVisibility(int position, CancellationToken cancellationToken = default(CancellationToken))
		{
			return LineVisibility.Visible;
		}

		/// <summary>
		/// Gets a FileLinePositionSpan for a TextSpan and the information whether this span is considered to be hidden or not. 
		/// FileLinePositionSpans are used primarily for diagnostics and source locations.
		/// This method combines a call to GetLineSpan and IsHiddenPosition.
		/// </summary>
		/// <param name="span"></param>
		/// <param name="isHiddenPosition">Returns a boolean indicating whether this span is considered hidden or not.</param>
		/// <remarks>This function is being called only in the context of sequence point creation and therefore interprets the 
		/// LineVisibility accordingly (BeforeFirstRemappingDirective -> Visible).</remarks>
		public virtual FileLinePositionSpan GetMappedLineSpanAndVisibility(TextSpan span, out bool isHiddenPosition)
		{
			isHiddenPosition = GetLineVisibility(span.Start) == LineVisibility.Hidden;
			return GetMappedLineSpan(span);
		}

		/// <summary>
		/// Returns a path for particular location in source that is presented to the user. 
		/// </summary>
		/// <remarks>
		/// Used for implementation of <see cref="System.Runtime.CompilerServices.CallerFilePathAttribute"/> 
		/// or for embedding source paths in error messages.
		/// 
		/// Unlike Dev12 we do account for #line and #ExternalSource directives when determining value for 
		/// <see cref="System.Runtime.CompilerServices.CallerFilePathAttribute"/>.
		/// </remarks>
		public string GetDisplayPath(TextSpan span)
		{
			var mappedSpan = GetMappedLineSpan(span);
			return mappedSpan.Path;
		}

		/// <summary>
		/// Returns a line number for particular location in source that is presented to the user. 
		/// </summary>
		/// <remarks>
		/// Used for implementation of <see cref="System.Runtime.CompilerServices.CallerLineNumberAttribute"/> 
		/// or for embedding source line numbers in error messages.
		/// 
		/// Unlike Dev12 we do account for #line and #ExternalSource directives when determining value for 
		/// <see cref="System.Runtime.CompilerServices.CallerLineNumberAttribute"/>.
		/// </remarks>
		public int GetDisplayLineNumber(TextSpan span)
		{
			// display line numbers are 1-based
			return GetMappedLineSpan(span).StartLinePosition.Line + 1;
		}

		/// <summary>
		/// Are there any hidden regions in the tree?
		/// </summary>
		/// <returns>True if there is at least one hidden region.</returns>
		public abstract bool HasHiddenRegions();

		/// <summary>
		/// Returns a list of the changed regions between this tree and the specified tree. The list is conservative for
		/// performance reasons. It may return larger regions than what has actually changed.
		/// </summary>
		public abstract IList<TextSpan> GetChangedSpans(SyntaxTree syntaxTree);

		/// <summary>
		/// Gets a location for the specified text span.
		/// </summary>
		public abstract Location GetLocation(TextSpan span);

		/// <summary>
		/// Determines if two trees are the same, disregarding trivia differences.
		/// </summary>
		/// <param name="tree">The tree to compare against.</param>
		/// <param name="topLevel"> If true then the trees are equivalent if the contained nodes and tokens declaring
		/// metadata visible symbolic information are equivalent, ignoring any differences of nodes inside method bodies
		/// or initializer expressions, otherwise all nodes and tokens must be equivalent. 
		/// </param>
		public abstract bool IsEquivalentTo(SyntaxTree tree, bool topLevel = false);

		/// <summary>
		/// Returns a new tree whose <see cref="FilePath"/> is the specified node and other properties are copied from the current tree.
		/// </summary>
		public abstract SyntaxTree WithFilePath(string path);

		/// <summary>
		/// Returns a <see cref="String" /> that represents the entire source text of this <see cref="SyntaxTree"/>.
		/// </summary>
		public override string ToString()
		{
			return this.GetText(CancellationToken.None).ToString();
		}

		public virtual bool SupportsLocations
		{
			get { return this.HasCompilationUnitRoot; }
		}
	}
}
