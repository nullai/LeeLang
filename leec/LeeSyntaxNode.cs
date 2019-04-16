using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public abstract class LeeSyntaxNode : GreenNode
	{
		public LeeSyntaxNode(SyntaxKind kind)
			: base((ushort)kind)
		{
		}

		public LeeSyntaxNode(SyntaxKind kind, int fullWidth)
			: base((ushort)kind, fullWidth)
		{
		}

		public LeeSyntaxNode(SyntaxKind kind, DiagnosticInfo[] diagnostics)
			: base((ushort)kind, diagnostics)
		{
		}

		public LeeSyntaxNode(SyntaxKind kind, DiagnosticInfo[] diagnostics, int fullWidth)
			: base((ushort)kind, diagnostics, fullWidth)
		{
		}

		public LeeSyntaxNode(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
			: base((ushort)kind, diagnostics, annotations)
		{
		}

		public LeeSyntaxNode(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations, int fullWidth)
			: base((ushort)kind, diagnostics, annotations, fullWidth)
		{
		}

		public override string Language
		{
			get { return "LeeLang"; }
		}

		public override string KindText => this.Kind.ToString();


		public override int RawContextualKind
		{
			get
			{
				return this.RawKind;
			}
		}

		public override bool IsStructuredTrivia
		{
			get
			{
				return this is StructuredTriviaSyntax;
			}
		}

		public override bool IsDirective
		{
			get
			{
				return this is DirectiveTriviaSyntax;
			}
		}

		public override bool IsSkippedTokensTrivia => this.Kind == SyntaxKind.SkippedTokensTrivia;

		public override int GetSlotOffset(int index)
		{
			// This implementation should not support arbitrary
			// length lists since the implementation is O(n).
			System.Diagnostics.Debug.Assert(index < 11); // Max. slots 11 (TypeDeclarationSyntax)

			int offset = 0;
			for (int i = 0; i < index; i++)
			{
				var child = this.GetSlot(i);
				if (child != null)
				{
					offset += child.FullWidth;
				}
			}

			return offset;
		}

		public SyntaxToken GetFirstToken()
		{
			return (SyntaxToken)this.GetFirstTerminal();
		}

		public SyntaxToken GetLastToken()
		{
			return (SyntaxToken)this.GetLastTerminal();
		}

		public SyntaxToken GetLastNonmissingToken()
		{
			return (SyntaxToken)this.GetLastNonmissingTerminal();
		}

		public virtual GreenNode GetLeadingTrivia()
		{
			return null;
		}

		public override GreenNode GetLeadingTriviaCore()
		{
			return this.GetLeadingTrivia();
		}

		public virtual GreenNode GetTrailingTrivia()
		{
			return null;
		}

		public override GreenNode GetTrailingTriviaCore()
		{
			return this.GetTrailingTrivia();
		}

		public abstract TResult Accept<TResult>(LeeSyntaxVisitor<TResult> visitor);

		public abstract void Accept(LeeSyntaxVisitor visitor);

		public virtual DirectiveStack ApplyDirectives(DirectiveStack stack)
		{
			return ApplyDirectives(this, stack);
		}

		public static DirectiveStack ApplyDirectives(GreenNode node, DirectiveStack stack)
		{
			if (node.ContainsDirectives)
			{
				for (int i = 0, n = node.SlotCount; i < n; i++)
				{
					var child = node.GetSlot(i);
					if (child != null)
					{
						stack = ApplyDirectivesToListOrNode(child, stack);
					}
				}
			}

			return stack;
		}

		public static DirectiveStack ApplyDirectivesToListOrNode(GreenNode listOrNode, DirectiveStack stack)
		{
			// If we have a list of trivia, then that node is not actually a LeeSyntaxNode.
			// Just defer to our standard ApplyDirectives helper as it will do the appropriate
			// walking of this list to ApplyDirectives to the children.
			if (listOrNode.RawKind == GreenNode.ListKind)
			{
				return ApplyDirectives(listOrNode, stack);
			}
			else
			{
				// Otherwise, we must have an actual piece of C# trivia.  Just apply the stack
				// to that node directly.
				return ((LeeSyntaxNode)listOrNode).ApplyDirectives(stack);
			}
		}

		public virtual IList<DirectiveTriviaSyntax> GetDirectives()
		{
			if ((this.flags & NodeFlags.ContainsDirectives) != 0)
			{
				var list = new List<DirectiveTriviaSyntax>(32);
				GetDirectives(this, list);
				return list;
			}

			return new DirectiveTriviaSyntax[0];
		}

		private static void GetDirectives(GreenNode node, List<DirectiveTriviaSyntax> directives)
		{
			if (node != null && node.ContainsDirectives)
			{
				var d = node as DirectiveTriviaSyntax;
				if (d != null)
				{
					directives.Add(d);
				}
				else
				{
					var t = node as SyntaxToken;
					if (t != null)
					{
						GetDirectives(t.GetLeadingTrivia(), directives);
						GetDirectives(t.GetTrailingTrivia(), directives);
					}
					else
					{
						for (int i = 0, n = node.SlotCount; i < n; i++)
						{
							GetDirectives(node.GetSlot(i), directives);
						}
					}
				}
			}
		}

		/// <summary>
		/// Should only be called during construction.
		/// </summary>
		/// <remarks>
		/// This should probably be an extra constructor parameter, but we don't need more constructor overloads.
		/// </remarks>
		protected void SetFactoryContext(SyntaxFactoryContext context)
		{
			if (context.IsInAsync)
			{
				this.flags |= NodeFlags.FactoryContextIsInAsync;
			}

			if (context.IsInQuery)
			{
				this.flags |= NodeFlags.FactoryContextIsInQuery;
			}
		}

		public static NodeFlags SetFactoryContext(NodeFlags flags, SyntaxFactoryContext context)
		{
			if (context.IsInAsync)
			{
				flags |= NodeFlags.FactoryContextIsInAsync;
			}

			if (context.IsInQuery)
			{
				flags |= NodeFlags.FactoryContextIsInQuery;
			}

			return flags;
		}

		public override bool IsTriviaWithEndOfLine()
		{
			return this.Kind == SyntaxKind.EndOfLineTrivia
				|| this.Kind == SyntaxKind.SingleLineCommentTrivia;
		}
	}
}
