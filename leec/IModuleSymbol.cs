﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IModuleSymbol : ISymbol
	{
		/// <summary>
		/// Returns a NamespaceSymbol representing the global (root) namespace, with
		/// module extent, that can be used to browse all of the symbols defined in this module.
		/// </summary>
		INamespaceSymbol GlobalNamespace { get; }

		/// <summary>
		/// Given a namespace symbol, returns the corresponding module specific namespace symbol
		/// </summary>
		INamespaceSymbol GetModuleNamespace(INamespaceSymbol namespaceSymbol);

		/// <summary>
		/// Returns an array of assembly identities for assemblies referenced by this module.
		/// Items at the same position from ReferencedAssemblies and from ReferencedAssemblySymbols 
		/// correspond to each other.
		/// </summary>
		AssemblyIdentity[] ReferencedAssemblies { get; }

		/// <summary>
		/// Returns an array of AssemblySymbol objects corresponding to assemblies referenced 
		/// by this module. Items at the same position from ReferencedAssemblies and 
		/// from ReferencedAssemblySymbols correspond to each other.
		/// </summary>
		IAssemblySymbol[] ReferencedAssemblySymbols { get; }

		/// <summary>
		/// If this symbol represents a metadata module returns the underlying <see cref="ModuleMetadata"/>.
		/// 
		/// Otherwise, this returns <see langword="null"/>.
		/// </summary>
		ModuleMetadata GetMetadata();
	}
}
