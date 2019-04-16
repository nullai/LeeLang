using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace leec
{
	public class ParsedSyntaxTree : LeeSyntaxTree
	{
		private readonly ParseOptions _options;
		private readonly string _path;
		private readonly LeeSyntaxNode _root;
		private readonly bool _hasCompilationUnitRoot;
		private SourceText _lazyText;

		public ParsedSyntaxTree(SourceText textOpt, string path, ParseOptions options, LeeSyntaxNode root, DirectiveStack directives)
		{
			Debug.Assert(root != null);
			Debug.Assert(options != null);
			Debug.Assert(textOpt != null);

			_lazyText = textOpt;
			_options = options;
			_path = path ?? string.Empty;
			_root = root;
			_hasCompilationUnitRoot = root.Kind == SyntaxKind.CompilationUnit;
			this.SetDirectiveStack(directives);
		}

		public override string FilePath
		{
			get { return _path; }
		}

		public override SourceText GetText(CancellationToken cancellationToken)
		{
			return _lazyText;
		}

		public override bool TryGetText(out SourceText text)
		{
			text = _lazyText;
			return text != null;
		}

		public override int Length
		{
			get { return _root.FullSpan.Length; }
		}

		public override LeeSyntaxNode GetRoot(CancellationToken cancellationToken)
		{
			return _root;
		}

		public override bool TryGetRoot(out LeeSyntaxNode root)
		{
			root = _root;
			return true;
		}

		protected override bool TryGetRootCore(out GreenNode root)
		{
			root = _root;
			return _hasCompilationUnitRoot;
		}

		public override bool HasCompilationUnitRoot
		{
			get
			{
				return _hasCompilationUnitRoot;
			}
		}

		public override ParseOptions Options => _options;
	}
}
