using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class ModuleReferences<TAssemblySymbol>
		where TAssemblySymbol : class, IAssemblySymbol
	{
		/// <summary>
		/// Identities of referenced assemblies (those that are or will be emitted to metadata).
		/// </summary>
		/// <remarks>
		/// Names[i] is the identity of assembly Symbols[i].
		/// </remarks>
		public readonly AssemblyIdentity[] Identities;

		/// <summary>
		/// Assembly symbols that the identities are resolved against.
		/// </summary>
		/// <remarks>
		/// Names[i] is the identity of assembly Symbols[i].
		/// Unresolved references are represented as MissingAssemblySymbols.
		/// </remarks>
		public readonly TAssemblySymbol[] Symbols;

		/// <summary>
		/// A subset of <see cref="Symbols"/> that correspond to references with non-matching (unified) 
		/// version along with unification details.
		/// </summary>
		public readonly UnifiedAssembly<TAssemblySymbol>[] UnifiedAssemblies;

		public ModuleReferences(
			AssemblyIdentity[] identities,
			TAssemblySymbol[] symbols,
			UnifiedAssembly<TAssemblySymbol>[] unifiedAssemblies)
		{
			Debug.Assert(identities != null);
			Debug.Assert(symbols != null);
			Debug.Assert(identities.Length == symbols.Length);
			Debug.Assert(unifiedAssemblies != null);

			this.Identities = identities;
			this.Symbols = symbols;
			this.UnifiedAssemblies = unifiedAssemblies;
		}
	}
}
