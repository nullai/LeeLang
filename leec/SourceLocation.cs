﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class SourceLocation : Location, IEquatable<SourceLocation>
	{
		private readonly SyntaxTree _syntaxTree;
		private readonly TextSpan _span;

		public SourceLocation(SyntaxTree syntaxTree, TextSpan span)
		{
			_syntaxTree = syntaxTree;
			_span = span;
		}

		public SourceLocation(LeeSyntaxNode node)
			: this(node.SyntaxTree, node.Span)
		{
		}

		public SourceLocation(SyntaxToken token)
			: this(token.SyntaxTree, token.Span)
		{
		}

		public SourceLocation(SyntaxTrivia trivia)
			: this(trivia.SyntaxTree, trivia.Span)
		{
		}

		public override LocationKind Kind
		{
			get
			{
				return LocationKind.SourceFile;
			}
		}

		public override TextSpan SourceSpan
		{
			get
			{
				return _span;
			}
		}

		public override SyntaxTree SourceTree
		{
			get
			{
				return _syntaxTree;
			}
		}

		public override FileLinePositionSpan GetLineSpan()
		{
			// If there's no syntax tree (e.g. because we're binding speculatively),
			// then just return an invalid span.
			if (_syntaxTree == null)
			{
				FileLinePositionSpan result = default(FileLinePositionSpan);
				Debug.Assert(!result.IsValid);
				return result;
			}

			return _syntaxTree.GetLineSpan(_span);
		}

		public override FileLinePositionSpan GetMappedLineSpan()
		{
			// If there's no syntax tree (e.g. because we're binding speculatively),
			// then just return an invalid span.
			if (_syntaxTree == null)
			{
				FileLinePositionSpan result = default(FileLinePositionSpan);
				Debug.Assert(!result.IsValid);
				return result;
			}

			return _syntaxTree.GetMappedLineSpan(_span);
		}

		public bool Equals(SourceLocation other)
		{
			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return other != null && other._syntaxTree == _syntaxTree && other._span == _span;
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as SourceLocation);
		}

		public override int GetHashCode()
		{
			return Hash.Combine(_syntaxTree, _span.GetHashCode());
		}

		protected override string GetDebuggerDisplay()
		{
			return base.GetDebuggerDisplay() + "\"" + _syntaxTree.ToString().Substring(_span.Start, _span.Length) + "\"";
		}
	}
}
