using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public class SyntaxIdentifierWithTrivia : SyntaxIdentifierExtended
	{
		private readonly GreenNode _leading;
		private readonly GreenNode _trailing;

		public SyntaxIdentifierWithTrivia(
			SyntaxKind contextualKind,
			string text,
			string valueText,
			GreenNode leading,
			GreenNode trailing)
			: base(contextualKind, text, valueText)
		{
			if (leading != null)
			{
				this.AdjustFlagsAndWidth(leading);
				_leading = leading;
			}
			if (trailing != null)
			{
				this.AdjustFlagsAndWidth(trailing);
				_trailing = trailing;
			}
		}

		public SyntaxIdentifierWithTrivia(
			SyntaxKind contextualKind,
			string text,
			string valueText,
			GreenNode leading,
			GreenNode trailing,
			DiagnosticInfo[] diagnostics,
			SyntaxAnnotation[] annotations)
			: base(contextualKind, text, valueText, diagnostics, annotations)
		{
			if (leading != null)
			{
				this.AdjustFlagsAndWidth(leading);
				_leading = leading;
			}
			if (trailing != null)
			{
				this.AdjustFlagsAndWidth(trailing);
				_trailing = trailing;
			}
		}

		public override GreenNode GetLeadingTrivia()
		{
			return _leading;
		}

		public override GreenNode GetTrailingTrivia()
		{
			return _trailing;
		}

		public override SyntaxToken TokenWithLeadingTrivia(GreenNode trivia)
		{
			return new SyntaxIdentifierWithTrivia(this.contextualKind, this.TextField, this.valueText, trivia, _trailing, this.GetDiagnostics(), this.GetAnnotations());
		}

		public override SyntaxToken TokenWithTrailingTrivia(GreenNode trivia)
		{
			return new SyntaxIdentifierWithTrivia(this.contextualKind, this.TextField, this.valueText, _leading, trivia, this.GetDiagnostics(), this.GetAnnotations());
		}

		public override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new SyntaxIdentifierWithTrivia(this.contextualKind, this.TextField, this.valueText, _leading, _trailing, diagnostics, this.GetAnnotations());
		}

		public override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new SyntaxIdentifierWithTrivia(this.contextualKind, this.TextField, this.valueText, _leading, _trailing, this.GetDiagnostics(), annotations);
		}
	}
}
