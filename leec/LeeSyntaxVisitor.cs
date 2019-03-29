using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public abstract partial class LeeSyntaxVisitor<TResult>
	{
		public virtual TResult Visit(LeeSyntaxNode node)
		{
			if (node == null)
			{
				return default(TResult);
			}

			return node.Accept(this);
		}

		public virtual TResult VisitToken(SyntaxToken token)
		{
			return this.DefaultVisit(token);
		}

		public virtual TResult VisitTrivia(SyntaxTrivia trivia)
		{
			return this.DefaultVisit(trivia);
		}

		protected virtual TResult DefaultVisit(LeeSyntaxNode node)
		{
			return default(TResult);
		}
	}

	public abstract partial class LeeSyntaxVisitor
	{
		public virtual void Visit(LeeSyntaxNode node)
		{
			if (node == null)
			{
				return;
			}

			node.Accept(this);
		}

		public virtual void VisitToken(SyntaxToken token)
		{
			this.DefaultVisit(token);
		}

		public virtual void VisitTrivia(SyntaxTrivia trivia)
		{
			this.DefaultVisit(trivia);
		}

		public virtual void DefaultVisit(LeeSyntaxNode node)
		{
		}
	}
}
