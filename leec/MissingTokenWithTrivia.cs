using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public class MissingTokenWithTrivia : SyntaxTokenWithTrivia
	{
		public MissingTokenWithTrivia(SyntaxKind kind, GreenNode leading, GreenNode trailing)
			: base(kind, leading, trailing)
		{
			this.flags &= ~NodeFlags.IsNotMissing;
		}

		public MissingTokenWithTrivia(SyntaxKind kind, GreenNode leading, GreenNode trailing, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(kind, leading, trailing, diagnostics, annotations)
		{
			this.flags &= ~NodeFlags.IsNotMissing;
		}

		public override string Text
		{
			get { return string.Empty; }
		}

		public override object Value
		{
			get
			{
				switch (this.Kind)
				{
					case SyntaxKind.IdentifierToken:
						return string.Empty;
					default:
						return null;
				}
			}
		}

		public override SyntaxToken TokenWithLeadingTrivia(GreenNode trivia)
		{
			return new MissingTokenWithTrivia(this.Kind, trivia, this.TrailingField, this.GetDiagnostics(), this.GetAnnotations());
		}

		public override SyntaxToken TokenWithTrailingTrivia(GreenNode trivia)
		{
			return new MissingTokenWithTrivia(this.Kind, this.LeadingField, trivia, this.GetDiagnostics(), this.GetAnnotations());
		}

		public override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new MissingTokenWithTrivia(this.Kind, this.LeadingField, this.TrailingField, diagnostics, this.GetAnnotations());
		}

		public override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new MissingTokenWithTrivia(this.Kind, this.LeadingField, this.TrailingField, this.GetDiagnostics(), annotations);
		}
	}
}
