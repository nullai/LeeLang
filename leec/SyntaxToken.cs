using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public class SyntaxToken : LeeSyntaxNode
	{
		public SyntaxToken(SyntaxKind kind)
			: base(kind)
		{
			FullWidth = this.Text.Length;
			this.flags |= NodeFlags.IsNotMissing; //note: cleared by subclasses representing missing tokens
		}

		public SyntaxToken(SyntaxKind kind, DiagnosticInfo[] diagnostics)
			: base(kind, diagnostics)
		{
			FullWidth = this.Text.Length;
			this.flags |= NodeFlags.IsNotMissing; //note: cleared by subclasses representing missing tokens
		}

		public SyntaxToken(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(kind, diagnostics, annotations)
		{
			FullWidth = this.Text.Length;
			this.flags |= NodeFlags.IsNotMissing; //note: cleared by subclasses representing missing tokens
		}

		public SyntaxToken(SyntaxKind kind, int fullWidth)
			: base(kind, fullWidth)
		{
			this.flags |= NodeFlags.IsNotMissing; //note: cleared by subclasses representing missing tokens
		}

		public SyntaxToken(SyntaxKind kind, int fullWidth, DiagnosticInfo[] diagnostics)
			: base(kind, diagnostics, fullWidth)
		{
			this.flags |= NodeFlags.IsNotMissing; //note: cleared by subclasses representing missing tokens
		}

		public SyntaxToken(SyntaxKind kind, int fullWidth, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base(kind, diagnostics, annotations, fullWidth)
		{
			this.flags |= NodeFlags.IsNotMissing; //note: cleared by subclasses representing missing tokens
		}

        public override bool IsToken => true;


		public override GreenNode GetSlot(int index)
		{
			throw ExceptionUtilities.Unreachable;
		}

		public static SyntaxToken Create(SyntaxKind kind)
		{
			if (kind > LastTokenWithWellKnownText)
			{
				if (!SyntaxFacts.IsAnyToken(kind))
				{
					throw new ArgumentException(string.Format("@@ThisMethodCanOnlyBeUsedToCreateTokens", kind), nameof(kind));
				}

				return CreateMissing(kind, null, null);
			}

			return s_tokensWithNoTrivia[(int)kind];
		}

		public static SyntaxToken Create(SyntaxKind kind, GreenNode leading, GreenNode trailing)
		{
			if (kind > LastTokenWithWellKnownText)
			{
				if (!SyntaxFacts.IsAnyToken(kind))
				{
					throw new ArgumentException(string.Format("@@ThisMethodCanOnlyBeUsedToCreateTokens", kind), nameof(kind));
				}

				return CreateMissing(kind, leading, trailing);
			}

			if (leading == null)
			{
				if (trailing == null)
				{
					return s_tokensWithNoTrivia[(int)kind];
				}
				else if (trailing == SyntaxFactory.Space)
				{
					return s_tokensWithSingleTrailingSpace[(int)kind];
				}
				else if (trailing == SyntaxFactory.CarriageReturnLineFeed)
				{
					return s_tokensWithSingleTrailingCRLF[(int)kind];
				}
			}

			if (leading == SyntaxFactory.ElasticZeroSpace && trailing == SyntaxFactory.ElasticZeroSpace)
			{
				return s_tokensWithElasticTrivia[(int)kind];
			}

			return new SyntaxTokenWithTrivia(kind, leading, trailing);
		}

		public static SyntaxToken CreateMissing(SyntaxKind kind, GreenNode leading, GreenNode trailing)
		{
			return new MissingTokenWithTrivia(kind, leading, trailing);
		}

		public const SyntaxKind FirstTokenWithWellKnownText = SyntaxKind.TildeToken;
		public const SyntaxKind LastTokenWithWellKnownText = SyntaxKind.EndOfFileToken;

		// TODO: eliminate the blank space before the first interesting element?
		private static readonly SyntaxToken[] s_tokensWithNoTrivia = new SyntaxToken[(int)LastTokenWithWellKnownText + 1];
		private static readonly SyntaxToken[] s_tokensWithElasticTrivia = new SyntaxToken[(int)LastTokenWithWellKnownText + 1];
		private static readonly SyntaxToken[] s_tokensWithSingleTrailingSpace = new SyntaxToken[(int)LastTokenWithWellKnownText + 1];
		private static readonly SyntaxToken[] s_tokensWithSingleTrailingCRLF = new SyntaxToken[(int)LastTokenWithWellKnownText + 1];

		static SyntaxToken()
		{
			for (var kind = FirstTokenWithWellKnownText; kind <= LastTokenWithWellKnownText; kind++)
			{
				s_tokensWithNoTrivia[(int)kind] = new SyntaxToken(kind);
				s_tokensWithElasticTrivia[(int)kind] = new SyntaxTokenWithTrivia(kind, SyntaxFactory.ElasticZeroSpace, SyntaxFactory.ElasticZeroSpace);
				s_tokensWithSingleTrailingSpace[(int)kind] = new SyntaxTokenWithTrivia(kind, null, SyntaxFactory.Space);
				s_tokensWithSingleTrailingCRLF[(int)kind] = new SyntaxTokenWithTrivia(kind, null, SyntaxFactory.CarriageReturnLineFeed);
			}
		}

		public static IEnumerable<SyntaxToken> GetWellKnownTokens()
		{
			foreach (var element in s_tokensWithNoTrivia)
			{
				if (element.Value != null)
				{
					yield return element;
				}
			}

			foreach (var element in s_tokensWithElasticTrivia)
			{
				if (element.Value != null)
				{
					yield return element;
				}
			}

			foreach (var element in s_tokensWithSingleTrailingSpace)
			{
				if (element.Value != null)
				{
					yield return element;
				}
			}

			foreach (var element in s_tokensWithSingleTrailingCRLF)
			{
				if (element.Value != null)
				{
					yield return element;
				}
			}
		}

		public static SyntaxToken Identifier(string text)
		{
			return new SyntaxIdentifier(text);
		}

		public static SyntaxToken Identifier(GreenNode leading, string text, GreenNode trailing)
		{
			if (leading == null)
			{
				if (trailing == null)
				{
					return Identifier(text);
				}
				else
				{
					return new SyntaxIdentifierWithTrailingTrivia(text, trailing);
				}
			}

			return new SyntaxIdentifierWithTrivia(SyntaxKind.IdentifierToken, text, text, leading, trailing);
		}

		public static SyntaxToken Identifier(SyntaxKind contextualKind, GreenNode leading, string text, string valueText, GreenNode trailing)
		{
			if (contextualKind == SyntaxKind.IdentifierToken && valueText == text)
			{
				return Identifier(leading, text, trailing);
			}

			return new SyntaxIdentifierWithTrivia(contextualKind, text, valueText, leading, trailing);
		}

		public static SyntaxToken WithValue<T>(SyntaxKind kind, string text, T value)
		{
			return new SyntaxTokenWithValue<T>(kind, text, value);
		}

		public static SyntaxToken WithValue<T>(SyntaxKind kind, GreenNode leading, string text, T value, GreenNode trailing)
		{
			return new SyntaxTokenWithValueAndTrivia<T>(kind, text, value, leading, trailing);
		}

		public static SyntaxToken StringLiteral(string text)
		{
			return new SyntaxTokenWithValue<string>(SyntaxKind.StringLiteralToken, text, text);
		}

		public static SyntaxToken StringLiteral(LeeSyntaxNode leading, string text, LeeSyntaxNode trailing)
		{
			return new SyntaxTokenWithValueAndTrivia<string>(SyntaxKind.StringLiteralToken, text, text, leading, trailing);
		}

		public virtual string Text
		{
			get { return SyntaxFacts.GetText(this.Kind); }
		}

		/// <summary>
		/// Returns the string representation of this token, not including its leading and trailing trivia.
		/// </summary>
		/// <returns>The string representation of this token, not including its leading and trailing trivia.</returns>
		/// <remarks>The length of the returned string is always the same as Span.Length</remarks>
		public override string ToString()
		{
			return this.Text;
		}

		public virtual object Value
		{
			get
			{
				switch (this.Kind)
				{
					case SyntaxKind.TrueKeyword:
						return true;
					case SyntaxKind.FalseKeyword:
						return false;
					case SyntaxKind.NullKeyword:
						return null;
					default:
						return this.Text;
				}
			}
		}

		public override object GetValue()
		{
			return this.Value;
		}

		public virtual string ValueText
		{
			get { return this.Text; }
		}

		public override string GetValueText()
		{
			return this.ValueText;
		}

		public override int Width
		{
			get { return this.Text.Length; }
		}

		public override int GetLeadingTriviaWidth()
		{
			var leading = this.GetLeadingTrivia();
			return leading != null ? leading.FullWidth : 0;
		}

		public override int GetTrailingTriviaWidth()
		{
			var trailing = this.GetTrailingTrivia();
			return trailing != null ? trailing.FullWidth : 0;
		}
		public sealed override GreenNode WithLeadingTrivia(GreenNode trivia)
		{
			return TokenWithLeadingTrivia(trivia);
		}

		public virtual SyntaxToken TokenWithLeadingTrivia(GreenNode trivia)
		{
			return new SyntaxTokenWithTrivia(this.Kind, trivia, null, this.GetDiagnostics(), this.GetAnnotations());
		}

		public sealed override GreenNode WithTrailingTrivia(GreenNode trivia)
		{
			return TokenWithTrailingTrivia(trivia);
		}

		public virtual SyntaxToken TokenWithTrailingTrivia(GreenNode trivia)
		{
			return new SyntaxTokenWithTrivia(this.Kind, null, trivia, this.GetDiagnostics(), this.GetAnnotations());
		}

		public override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
		{
			System.Diagnostics.Debug.Assert(this.GetType() == typeof(SyntaxToken));
			return new SyntaxToken(this.Kind, this.FullWidth, diagnostics, this.GetAnnotations());
		}

		public override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			System.Diagnostics.Debug.Assert(this.GetType() == typeof(SyntaxToken));
			return new SyntaxToken(this.Kind, this.FullWidth, this.GetDiagnostics(), annotations);
		}

		public override DirectiveStack ApplyDirectives(DirectiveStack stack)
		{
			if (this.ContainsDirectives)
			{
				stack = ApplyDirectivesToTrivia(this.GetLeadingTrivia(), stack);
				stack = ApplyDirectivesToTrivia(this.GetTrailingTrivia(), stack);
			}

			return stack;
		}

		private static DirectiveStack ApplyDirectivesToTrivia(GreenNode triviaList, DirectiveStack stack)
		{
			if (triviaList != null && triviaList.ContainsDirectives)
			{
				return ApplyDirectivesToListOrNode(triviaList, stack);
			}

			return stack;
		}

		public override TResult Accept<TResult>(LeeSyntaxVisitor<TResult> visitor)
		{
			return visitor.VisitToken(this);
		}

		public override void Accept(LeeSyntaxVisitor visitor)
		{
			visitor.VisitToken(this);
		}

		public override bool IsEquivalentTo(GreenNode other)
		{
			if (!base.IsEquivalentTo(other))
			{
				return false;
			}

			var otherToken = (SyntaxToken)other;

			if (this.Text != otherToken.Text)
			{
				return false;
			}

			var thisLeading = this.GetLeadingTrivia();
			var otherLeading = otherToken.GetLeadingTrivia();
			if (thisLeading != otherLeading)
			{
				if (thisLeading == null || otherLeading == null)
				{
					return false;
				}

				if (!thisLeading.IsEquivalentTo(otherLeading))
				{
					return false;
				}
			}

			var thisTrailing = this.GetTrailingTrivia();
			var otherTrailing = otherToken.GetTrailingTrivia();
			if (thisTrailing != otherTrailing)
			{
				if (thisTrailing == null || otherTrailing == null)
				{
					return false;
				}

				if (!thisTrailing.IsEquivalentTo(otherTrailing))
				{
					return false;
				}
			}

			return true;
		}
	}
}
