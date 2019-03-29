using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public static partial class SyntaxFactory
	{
		private const string CrLf = "\r\n";
		public static readonly SyntaxTrivia CarriageReturnLineFeed = EndOfLine(CrLf);
		public static readonly SyntaxTrivia LineFeed = EndOfLine("\n");
		public static readonly SyntaxTrivia CarriageReturn = EndOfLine("\r");
		public static readonly SyntaxTrivia Space = Whitespace(" ");
		public static readonly SyntaxTrivia Tab = Whitespace("\t");

		public static readonly SyntaxTrivia ElasticCarriageReturnLineFeed = EndOfLine(CrLf, elastic: true);
		public static readonly SyntaxTrivia ElasticLineFeed = EndOfLine("\n", elastic: true);
		public static readonly SyntaxTrivia ElasticCarriageReturn = EndOfLine("\r", elastic: true);
		public static readonly SyntaxTrivia ElasticSpace = Whitespace(" ", elastic: true);
		public static readonly SyntaxTrivia ElasticTab = Whitespace("\t", elastic: true);

		public static readonly SyntaxTrivia ElasticZeroSpace = Whitespace(string.Empty, elastic: true);


		// NOTE: it would be nice to have constants for OmittedArraySizeException and OmittedTypeArgument,
		// but it's non-trivial to introduce such constants, since they would make this class take a dependency
		// on the static fields of SyntaxToken (specifically, TokensWithNoTrivia via SyntaxToken.Create).  That
		// could cause unpredictable behavior, since SyntaxToken's static constructor already depends on the 
		// static fields of this class (specifically, ElasticZeroSpace).

		public static SyntaxTrivia EndOfLine(string text, bool elastic = false)
		{
			SyntaxTrivia trivia = null;

			// use predefined trivia
			switch (text)
			{
				case "\r":
					trivia = elastic ? SyntaxFactory.ElasticCarriageReturn : SyntaxFactory.CarriageReturn;
					break;
				case "\n":
					trivia = elastic ? SyntaxFactory.ElasticLineFeed : SyntaxFactory.LineFeed;
					break;
				case "\r\n":
					trivia = elastic ? SyntaxFactory.ElasticCarriageReturnLineFeed : SyntaxFactory.CarriageReturnLineFeed;
					break;
			}

			// note: predefined trivia might not yet be defined during initialization
			if (trivia != null)
			{
				return trivia;
			}

			trivia = SyntaxTrivia.Create(SyntaxKind.EndOfLineTrivia, text);
			if (!elastic)
			{
				return trivia;
			}

			return trivia;
		}

		public static SyntaxTrivia Whitespace(string text, bool elastic = false)
		{
			var trivia = SyntaxTrivia.Create(SyntaxKind.WhitespaceTrivia, text);
			if (!elastic)
			{
				return trivia;
			}

			return trivia;
		}

		public static SyntaxTrivia Comment(string text)
		{
			if (text.StartsWith("/*", StringComparison.Ordinal))
			{
				return SyntaxTrivia.Create(SyntaxKind.MultiLineCommentTrivia, text);
			}
			else
			{
				return SyntaxTrivia.Create(SyntaxKind.SingleLineCommentTrivia, text);
			}
		}

		public static SyntaxTrivia ConflictMarker(string text)
			=> SyntaxTrivia.Create(SyntaxKind.ConflictMarkerTrivia, text);

		public static SyntaxTrivia DisabledText(string text)
		{
			return SyntaxTrivia.Create(SyntaxKind.DisabledTextTrivia, text);
		}

		public static SyntaxTrivia PreprocessingMessage(string text)
		{
			return SyntaxTrivia.Create(SyntaxKind.PreprocessingMessageTrivia, text);
		}

		public static SyntaxToken Token(SyntaxKind kind)
		{
			return SyntaxToken.Create(kind);
		}

		public static SyntaxToken Token(GreenNode leading, SyntaxKind kind, GreenNode trailing)
		{
			return SyntaxToken.Create(kind, leading, trailing);
		}

		public static SyntaxToken Token(GreenNode leading, SyntaxKind kind, string text, string valueText, GreenNode trailing)
		{
			Debug.Assert(SyntaxFacts.IsAnyToken(kind));
			Debug.Assert(kind != SyntaxKind.IdentifierToken);
			Debug.Assert(kind != SyntaxKind.CharacterLiteralToken);
			Debug.Assert(kind != SyntaxKind.NumericLiteralToken);

			string defaultText = SyntaxFacts.GetText(kind);
			return kind >= SyntaxToken.FirstTokenWithWellKnownText && kind <= SyntaxToken.LastTokenWithWellKnownText && text == defaultText && valueText == defaultText
				? Token(leading, kind, trailing)
				: SyntaxToken.WithValue(kind, leading, text, valueText, trailing);
		}

		public static SyntaxToken MissingToken(SyntaxKind kind)
		{
			return SyntaxToken.CreateMissing(kind, null, null);
		}

		public static SyntaxToken MissingToken(GreenNode leading, SyntaxKind kind, GreenNode trailing)
		{
			return SyntaxToken.CreateMissing(kind, leading, trailing);
		}

		public static SyntaxToken Identifier(string text)
		{
			return Identifier(SyntaxKind.IdentifierToken, null, text, text, null);
		}

		public static SyntaxToken Identifier(GreenNode leading, string text, GreenNode trailing)
		{
			return Identifier(SyntaxKind.IdentifierToken, leading, text, text, trailing);
		}

		public static SyntaxToken Identifier(SyntaxKind contextualKind, GreenNode leading, string text, string valueText, GreenNode trailing)
		{
			return SyntaxToken.Identifier(contextualKind, leading, text, valueText, trailing);
		}

		public static SyntaxToken Literal(GreenNode leading, string text, int value, GreenNode trailing)
		{
			return SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);
		}

		public static SyntaxToken Literal(GreenNode leading, string text, uint value, GreenNode trailing)
		{
			return SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);
		}

		public static SyntaxToken Literal(GreenNode leading, string text, long value, GreenNode trailing)
		{
			return SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);
		}

		public static SyntaxToken Literal(GreenNode leading, string text, ulong value, GreenNode trailing)
		{
			return SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);
		}

		public static SyntaxToken Literal(GreenNode leading, string text, float value, GreenNode trailing)
		{
			return SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);
		}

		public static SyntaxToken Literal(GreenNode leading, string text, double value, GreenNode trailing)
		{
			return SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);
		}

		public static SyntaxToken Literal(GreenNode leading, string text, decimal value, GreenNode trailing)
		{
			return SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);
		}

		public static SyntaxToken Literal(GreenNode leading, string text, string value, GreenNode trailing)
		{
			return SyntaxToken.WithValue(SyntaxKind.StringLiteralToken, leading, text, value, trailing);
		}

		public static SyntaxToken Literal(GreenNode leading, string text, SyntaxKind kind, string value, GreenNode trailing)
		{
			return SyntaxToken.WithValue(kind, leading, text, value, trailing);
		}

		public static SyntaxToken Literal(GreenNode leading, string text, char value, GreenNode trailing)
		{
			return SyntaxToken.WithValue(SyntaxKind.CharacterLiteralToken, leading, text, value, trailing);
		}

		public static SyntaxToken BadToken(GreenNode leading, string text, GreenNode trailing)
		{
			return SyntaxToken.WithValue(SyntaxKind.BadToken, leading, text, text, trailing);
		}

		public static SyntaxTrivia DocumentationCommentExteriorTrivia(string text)
		{
			return SyntaxTrivia.Create(SyntaxKind.DocumentationCommentExteriorTrivia, text);
		}

		public static SyntaxList<TNode> List<TNode>() where TNode : LeeSyntaxNode
		{
			return default(SyntaxList<TNode>);
		}

		public static SyntaxList<TNode> List<TNode>(TNode node) where TNode : LeeSyntaxNode
		{
			return new SyntaxList<TNode>(SyntaxList.List(node));
		}

		public static SyntaxList<TNode> List<TNode>(TNode node0, TNode node1) where TNode : LeeSyntaxNode
		{
			return new SyntaxList<TNode>(SyntaxList.List(node0, node1));
		}

		public static GreenNode ListNode(LeeSyntaxNode node0, LeeSyntaxNode node1)
		{
			return SyntaxList.List(node0, node1);
		}

		public static SyntaxList<TNode> List<TNode>(TNode node0, TNode node1, TNode node2) where TNode : LeeSyntaxNode
		{
			return new SyntaxList<TNode>(SyntaxList.List(node0, node1, node2));
		}

		public static GreenNode ListNode(LeeSyntaxNode node0, LeeSyntaxNode node1, LeeSyntaxNode node2)
		{
			return SyntaxList.List(node0, node1, node2);
		}

		public static SyntaxList<TNode> List<TNode>(params TNode[] nodes) where TNode : LeeSyntaxNode
		{
			if (nodes != null)
			{
				return new SyntaxList<TNode>(SyntaxList.List(nodes));
			}

			return default(SyntaxList<TNode>);
		}

		public static GreenNode ListNode(params GreenNode[] nodes)
		{
			return SyntaxList.List(nodes);
		}

		public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(TNode node) where TNode : LeeSyntaxNode
		{
			return new SeparatedSyntaxList<TNode>(new SyntaxList<LeeSyntaxNode>(node));
		}

		public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(SyntaxToken token) where TNode : LeeSyntaxNode
		{
			return new SeparatedSyntaxList<TNode>(new SyntaxList<LeeSyntaxNode>(token));
		}

		public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(TNode node1, SyntaxToken token, TNode node2) where TNode : LeeSyntaxNode
		{
			return new SeparatedSyntaxList<TNode>(new SyntaxList<LeeSyntaxNode>(SyntaxList.List(node1, token, node2)));
		}

		public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(params LeeSyntaxNode[] nodes) where TNode : LeeSyntaxNode
		{
			if (nodes != null)
			{
				return new SeparatedSyntaxList<TNode>(SyntaxList.List(nodes));
			}

			return default(SeparatedSyntaxList<TNode>);
		}

		public static IEnumerable<SyntaxTrivia> GetWellKnownTrivia()
		{
			yield return CarriageReturnLineFeed;
			yield return LineFeed;
			yield return CarriageReturn;
			yield return Space;
			yield return Tab;

			yield return ElasticCarriageReturnLineFeed;
			yield return ElasticLineFeed;
			yield return ElasticCarriageReturn;
			yield return ElasticSpace;
			yield return ElasticTab;

			yield return ElasticZeroSpace;
		}

		public static IEnumerable<SyntaxToken> GetWellKnownTokens()
		{
			return SyntaxToken.GetWellKnownTokens();
		}

		public static ExpressionSyntax GetStandaloneExpression(ExpressionSyntax expression)
		{
			return SyntaxFactory.GetStandaloneNode(expression) as ExpressionSyntax ?? expression;
		}

		public static LeeSyntaxNode GetStandaloneNode(LeeSyntaxNode node)
		{
			if (node == null || !(node is ExpressionSyntax || node is CrefSyntax))
			{
				return node;
			}

			switch (node.Kind)
			{
				case SyntaxKind.IdentifierName:
				case SyntaxKind.GenericName:
				case SyntaxKind.NameMemberCref:
				case SyntaxKind.IndexerMemberCref:
				case SyntaxKind.OperatorMemberCref:
				case SyntaxKind.ConversionOperatorMemberCref:
				case SyntaxKind.ArrayType:
				case SyntaxKind.NullableType:
					// Adjustment may be required.
					break;
				default:
					return node;
			}

			var parent = (LeeSyntaxNode)node.Parent;

			if (parent == null)
			{
				return node;
			}

			switch (parent.Kind)
			{
				case SyntaxKind.QualifiedName:
					if (((QualifiedNameSyntax)parent).Right == node)
					{
						return parent;
					}

					break;
				case SyntaxKind.AliasQualifiedName:
					if (((AliasQualifiedNameSyntax)parent).Name == node)
					{
						return parent;
					}

					break;
				case SyntaxKind.SimpleMemberAccessExpression:
				case SyntaxKind.PointerMemberAccessExpression:
					if (((MemberAccessExpressionSyntax)parent).Name == node)
					{
						return parent;
					}

					break;

				case SyntaxKind.MemberBindingExpression:
					{
						if (((MemberBindingExpressionSyntax)parent).Name == node)
						{
							return parent;
						}

						break;
					}

				// Only care about name member crefs because the other cref members
				// are identifier by keywords, not syntax nodes.
				case SyntaxKind.NameMemberCref:
					if (((NameMemberCrefSyntax)parent).Name == node)
					{
						LeeSyntaxNode grandparent = (LeeSyntaxNode)parent.Parent;
						return grandparent != null && grandparent.Kind == SyntaxKind.QualifiedCref
							? grandparent
							: parent;
					}

					break;

				case SyntaxKind.QualifiedCref:
					if (((QualifiedCrefSyntax)parent).Member == node)
					{
						return parent;
					}

					break;

				case SyntaxKind.ArrayCreationExpression:
					if (((ArrayCreationExpressionSyntax)parent).Type == node)
					{
						return parent;
					}

					break;

				case SyntaxKind.ObjectCreationExpression:
					if (node.Kind == SyntaxKind.NullableType && ((ObjectCreationExpressionSyntax)parent).Type == node)
					{
						return parent;
					}

					break;

				case SyntaxKind.StackAllocArrayCreationExpression:
					if (((StackAllocArrayCreationExpressionSyntax)parent).Type == node)
					{
						return parent;
					}

					break;

				case SyntaxKind.NameColon:
					if (parent.Parent.IsKind(SyntaxKind.Subpattern))
					{
						return (LeeSyntaxNode)parent.Parent;
					}

					break;
			}

			return node;
		}
	}
}
