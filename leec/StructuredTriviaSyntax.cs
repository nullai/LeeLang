using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public abstract class StructuredTriviaSyntax : LeeSyntaxNode
	{
		public StructuredTriviaSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics = null, SyntaxAnnotation[] annotations = null)
			: base(kind, diagnostics, annotations)
		{
			this.Initialize();
		}

		private void Initialize()
		{
			this.flags |= NodeFlags.ContainsStructuredTrivia;

			if (this.Kind == SyntaxKind.SkippedTokensTrivia)
			{
				this.flags |= NodeFlags.ContainsSkippedText;
			}
		}
	}
}
