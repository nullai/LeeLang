using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public struct DeclarationInfo
	{
		private readonly GreenNode _declaredNode;
		private readonly GreenNode[] _executableCodeBlocks;
		private readonly ISymbol _declaredSymbol;

		internal DeclarationInfo(GreenNode declaredNode, GreenNode[] executableCodeBlocks, ISymbol declaredSymbol)
		{
			Debug.Assert(declaredNode != null);
			Debug.Assert(executableCodeBlocks != null);

			// TODO: Below assert has been commented out as is not true for VB field decls where multiple variables can share same initializer.
			// Declared node is the identifier, which doesn't contain the initializer. Can we tweak the assert somehow to handle this case?
			// Debug.Assert(executableCodeBlocks.All(n => n.Ancestors().Contains(declaredNode)));

			_declaredNode = declaredNode;
			_executableCodeBlocks = executableCodeBlocks;
			_declaredSymbol = declaredSymbol;
		}

		/// <summary>
		/// Topmost syntax node for this declaration.
		/// </summary>
		public GreenNode DeclaredNode => _declaredNode;

		/// <summary>
		/// Syntax nodes for executable code blocks (method body, initializers, etc.) associated with this declaration.
		/// </summary>
		public GreenNode[] ExecutableCodeBlocks => _executableCodeBlocks;

		/// <summary>
		/// Symbol declared by this declaration.
		/// </summary>
		public ISymbol DeclaredSymbol => _declaredSymbol;
	}
}
