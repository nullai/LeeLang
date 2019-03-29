using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public class SyntaxTokenWithValue<T> : SyntaxToken
	{
		protected readonly string TextField;
		protected readonly T ValueField;

		public SyntaxTokenWithValue(SyntaxKind kind, string text, T value)
			: base(kind, text.Length)
		{
			this.TextField = text;
			this.ValueField = value;
		}

		public SyntaxTokenWithValue(SyntaxKind kind, string text, T value, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(kind, text.Length, diagnostics, annotations)
		{
			this.TextField = text;
			this.ValueField = value;
		}

		public override string Text
		{
			get
			{
				return this.TextField;
			}
		}

		public override object Value
		{
			get
			{
				return this.ValueField;
			}
		}

		public override string ValueText
		{
			get
			{
				return Convert.ToString(this.ValueField);
			}
		}

		public override SyntaxToken TokenWithLeadingTrivia(GreenNode trivia)
		{
			return new SyntaxTokenWithValueAndTrivia<T>(this.Kind, this.TextField, this.ValueField, trivia, null, this.GetDiagnostics(), this.GetAnnotations());
		}

		public override SyntaxToken TokenWithTrailingTrivia(GreenNode trivia)
		{
			return new SyntaxTokenWithValueAndTrivia<T>(this.Kind, this.TextField, this.ValueField, null, trivia, this.GetDiagnostics(), this.GetAnnotations());
		}

		public override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new SyntaxTokenWithValue<T>(this.Kind, this.TextField, this.ValueField, diagnostics, this.GetAnnotations());
		}

		public override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new SyntaxTokenWithValue<T>(this.Kind, this.TextField, this.ValueField, this.GetDiagnostics(), annotations);
		}
	}
}
