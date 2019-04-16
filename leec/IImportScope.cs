using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IImportScope
	{
		/// <summary>
		/// Zero or more used namespaces. These correspond to using directives in C# or Imports syntax in VB.
		/// Multiple invocations return the same array instance.
		/// </summary>
		UsedNamespaceOrType[] GetUsedNamespaces();

		/// <summary>
		/// Parent import scope, or null.
		/// </summary>
		IImportScope Parent { get; }
	}
}
