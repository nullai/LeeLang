using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public class SyntaxTokenWithTrivia : SyntaxToken
	{
		protected readonly GreenNode LeadingField;
		protected readonly GreenNode TrailingField;

		public SyntaxTokenWithTrivia(SyntaxKind kind, GreenNode leading, GreenNode trailing)
			: base(kind)
		{
			if (leading != null)
			{
				this.AdjustFlagsAndWidth(leading);
				this.LeadingField = leading;
			}
			if (trailing != null)
			{
				this.AdjustFlagsAndWidth(trailing);
				this.TrailingField = trailing;
			}
		}

		public SyntaxTokenWithTrivia(SyntaxKind kind, GreenNode leading, GreenNode trailing, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(kind, diagnostics, annotations)
		{
			if (leading != null)
			{
				this.AdjustFlagsAndWidth(leading);
				this.LeadingField = leading;
			}
			if (trailing != null)
			{
				this.AdjustFlagsAndWidth(trailing);
				this.TrailingField = trailing;
			}
		}

		public override GreenNode GetLeadingTrivia()
		{
			return this.LeadingField;
		}

		public override GreenNode GetTrailingTrivia()
		{
			return this.TrailingField;
		}

		public override SyntaxToken TokenWithLeadingTrivia(GreenNode trivia)
		{
			return new SyntaxTokenWithTrivia(this.Kind, trivia, this.TrailingField, this.GetDiagnostics(), this.GetAnnotations());
		}

		public override SyntaxToken TokenWithTrailingTrivia(GreenNode trivia)
		{
			return new SyntaxTokenWithTrivia(this.Kind, this.LeadingField, trivia, this.GetDiagnostics(), this.GetAnnotations());
		}

		public override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new SyntaxTokenWithTrivia(this.Kind, this.LeadingField, this.TrailingField, diagnostics, this.GetAnnotations());
		}

		public override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new SyntaxTokenWithTrivia(this.Kind, this.LeadingField, this.TrailingField, this.GetDiagnostics(), annotations);
		}
	}
}
