using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public class SyntaxIdentifierWithTrailingTrivia : SyntaxIdentifier
	{
		private readonly GreenNode _trailing;

		public SyntaxIdentifierWithTrailingTrivia(string text, GreenNode trailing)
			: base(text)
		{
			if (trailing != null)
			{
				this.AdjustFlagsAndWidth(trailing);
				_trailing = trailing;
			}
		}

		public SyntaxIdentifierWithTrailingTrivia(string text, GreenNode trailing, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(text, diagnostics, annotations)
		{
			if (trailing != null)
			{
				this.AdjustFlagsAndWidth(trailing);
				_trailing = trailing;
			}
		}

		public override GreenNode GetTrailingTrivia()
		{
			return _trailing;
		}

		public override SyntaxToken TokenWithLeadingTrivia(GreenNode trivia)
		{
			return new SyntaxIdentifierWithTrivia(this.Kind, this.TextField, this.TextField, trivia, _trailing, this.GetDiagnostics(), this.GetAnnotations());
		}

		public override SyntaxToken TokenWithTrailingTrivia(GreenNode trivia)
		{
			return new SyntaxIdentifierWithTrailingTrivia(this.TextField, trivia, this.GetDiagnostics(), this.GetAnnotations());
		}

		public override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new SyntaxIdentifierWithTrailingTrivia(this.TextField, _trailing, diagnostics, this.GetAnnotations());
		}

		public override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new SyntaxIdentifierWithTrailingTrivia(this.TextField, _trailing, this.GetDiagnostics(), annotations);
		}
	}
}
