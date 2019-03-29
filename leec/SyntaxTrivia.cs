using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public class SyntaxTrivia : LeeSyntaxNode
	{
		public readonly string Text;

		public SyntaxTrivia(SyntaxKind kind, string text, DiagnosticInfo[] diagnostics = null, SyntaxAnnotation[] annotations = null)
			: base(kind, diagnostics, annotations, text.Length)
		{
			this.Text = text;
			if (kind == SyntaxKind.PreprocessingMessageTrivia)
			{
				this.flags |= NodeFlags.ContainsSkippedText;
			}
		}

		public override bool IsTrivia => true;

		public static SyntaxTrivia Create(SyntaxKind kind, string text)
		{
			return new SyntaxTrivia(kind, text);
		}

		public override string ToFullString()
		{
			return this.Text;
		}

		public override string ToString()
		{
			return this.Text;
		}

		public override GreenNode GetSlot(int index)
		{
			throw new Exception();
		}

		public override int Width
		{
			get
			{
				Debug.Assert(this.FullWidth == this.Text.Length);
				return this.FullWidth;
			}
		}

		public override int GetLeadingTriviaWidth()
		{
			return 0;
		}

		public override int GetTrailingTriviaWidth()
		{
			return 0;
		}

		public override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new SyntaxTrivia(this.Kind, this.Text, diagnostics, GetAnnotations());
		}

		public override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new SyntaxTrivia(this.Kind, this.Text, GetDiagnostics(), annotations);
		}

		public override TResult Accept<TResult>(LeeSyntaxVisitor<TResult> visitor)
		{
			return visitor.VisitTrivia(this);
		}

		public override void Accept(LeeSyntaxVisitor visitor)
		{
			visitor.VisitTrivia(this);
		}

		public override bool IsEquivalentTo(GreenNode other)
		{
			if (!base.IsEquivalentTo(other))
			{
				return false;
			}

			if (this.Text != ((SyntaxTrivia)other).Text)
			{
				return false;
			}

			return true;
		}
	}
}
