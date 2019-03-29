using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public class SyntaxTokenWithValueAndTrivia<T> : SyntaxTokenWithValue<T>
	{
		private readonly GreenNode _leading;
		private readonly GreenNode _trailing;

		public SyntaxTokenWithValueAndTrivia(SyntaxKind kind, string text, T value, GreenNode leading, GreenNode trailing)
			: base(kind, text, value)
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

		public SyntaxTokenWithValueAndTrivia(
			SyntaxKind kind,
			string text,
			T value,
			GreenNode leading,
			GreenNode trailing,
			DiagnosticInfo[] diagnostics,
			SyntaxAnnotation[] annotations)
			: base(kind, text, value, diagnostics, annotations)
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
			return new SyntaxTokenWithValueAndTrivia<T>(this.Kind, this.TextField, this.ValueField, trivia, _trailing, this.GetDiagnostics(), this.GetAnnotations());
		}

		public override SyntaxToken TokenWithTrailingTrivia(GreenNode trivia)
		{
			return new SyntaxTokenWithValueAndTrivia<T>(this.Kind, this.TextField, this.ValueField, _leading, trivia, this.GetDiagnostics(), this.GetAnnotations());
		}

		public override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			return new SyntaxTokenWithValueAndTrivia<T>(this.Kind, this.TextField, this.ValueField, _leading, _trailing, diagnostics, this.GetAnnotations());
		}

		public override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new SyntaxTokenWithValueAndTrivia<T>(this.Kind, this.TextField, this.ValueField, _leading, _trailing, this.GetDiagnostics(), annotations);
		}
	}
}
