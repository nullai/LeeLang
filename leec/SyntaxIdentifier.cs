using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public class SyntaxIdentifier : SyntaxToken
	{
		protected readonly string TextField;

		public SyntaxIdentifier(string text)
			: base(SyntaxKind.IdentifierToken, text.Length)
		{
			this.TextField = text;
		}

		public SyntaxIdentifier(string text, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(SyntaxKind.IdentifierToken, text.Length, diagnostics, annotations)
		{
			this.TextField = text;
		}

		public override string Text
		{
			get { return this.TextField; }
		}

		public override object Value
		{
			get { return this.TextField; }
		}

		public override string ValueText
		{
			get { return this.TextField; }
		}

		public override SyntaxToken TokenWithLeadingTrivia(GreenNode trivia)
		{
			return new SyntaxIdentifierWithTrivia(this.Kind, this.TextField, this.TextField, trivia, null, this.GetDiagnostics(), this.GetAnnotations());
		}

		public override SyntaxToken TokenWithTrailingTrivia(GreenNode trivia)
		{
			return new SyntaxIdentifierWithTrivia(this.Kind, this.TextField, this.TextField, null, trivia, this.GetDiagnostics(), this.GetAnnotations());
		}

		public override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new SyntaxIdentifier(this.Text, diagnostics, this.GetAnnotations());
		}

		public override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new SyntaxIdentifier(this.Text, this.GetDiagnostics(), annotations);
		}
	}
}
