using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace leec
{
    using MetadataOrDiagnostic = System.Object;
	public struct ReferencedAssemblieInfo
	{
		public IAssemblySymbol assemblie;
		public string[] names;
	}
	public abstract class CommonReferenceManager
	{
		/// <summary>
		/// Must be acquired whenever the following data are about to be modified:
		/// - Compilation.lazyAssemblySymbol
		/// - Compilation.referenceManager
		/// - ReferenceManager state
		/// - <see cref="AssemblyMetadata.CachedSymbols"/>
		/// - <see cref="Compilation.RetargetingAssemblySymbols"/>
		/// 
		/// All the above data should be updated at once while holding this lock.
		/// Once lazyAssemblySymbol is set the Compilation.referenceManager field and ReferenceManager
		/// state should not change.
		/// </summary>
		public static object SymbolCacheAndReferenceManagerStateGuard = new object();

		/// <summary>
		/// Enumerates all referenced assemblies.
		/// </summary>
		public abstract IEnumerable<KeyValuePair<MetadataReference, IAssemblySymbol>> GetReferencedAssemblies();

		/// <summary>
		/// Enumerates all referenced assemblies and their aliases.
		/// </summary>
		public abstract IEnumerable<ReferencedAssemblieInfo> GetReferencedAssemblyAliases();

		public abstract MetadataReference GetMetadataReference(IAssemblySymbol assemblySymbol);
		public abstract MetadataReference[] ExplicitReferences { get; }
		public abstract MetadataReference[] ImplicitReferences { get; }
		public abstract IEnumerable<KeyValuePair<AssemblyIdentity, PortableExecutableReference>> GetImplicitlyResolvedAssemblyReferences();
	}

	public abstract partial class CommonReferenceManager<TCompilation, TAssemblySymbol> : CommonReferenceManager
		where TCompilation : Compilation
		where TAssemblySymbol : class, IAssemblySymbolInternal
	{


		public struct BoundInputAssembly
		{
			/// <summary>
			/// Suitable AssemblySymbol instance for the corresponding assembly, 
			/// null reference if none is available/found.
			/// </summary>
			public TAssemblySymbol AssemblySymbol;

			/// <summary>
			/// For each AssemblyRef of this AssemblyDef specifies which AssemblyDef matches the reference.
			/// </summary>
			/// <remarks>
			/// Result of resolving assembly references of the corresponding assembly 
			/// against provided set of assemblies. Essentially, this is an array returned by
			/// AssemblyData.BindAssemblyReferences method. 
			/// 
			/// Each element describes the assembly the corresponding reference of the input assembly 
			/// is bound to.
			/// </remarks>
			public AssemblyReferenceBinding[] ReferenceBinding;

			private string GetDebuggerDisplay()
			{
				return AssemblySymbol == null ? "?" : AssemblySymbol.ToString();
			}
		}

		public struct AssemblyReferenceBinding
		{
			private readonly AssemblyIdentity _referenceIdentity;
			private readonly int _definitionIndex;
			private readonly int _versionDifference;

			/// <summary>
			/// Failed binding.
			/// </summary>
			public AssemblyReferenceBinding(AssemblyIdentity referenceIdentity)
			{
				Debug.Assert(referenceIdentity != null);

				_referenceIdentity = referenceIdentity;
				_definitionIndex = -1;
				_versionDifference = 0;
			}

			/// <summary>
			/// Successful binding.
			/// </summary>
			public AssemblyReferenceBinding(AssemblyIdentity referenceIdentity, int definitionIndex, int versionDifference = 0)
			{
				Debug.Assert(referenceIdentity != null);
				Debug.Assert(definitionIndex >= 0);
				Debug.Assert(versionDifference >= -1 && versionDifference <= +1);

				_referenceIdentity = referenceIdentity;
				_definitionIndex = definitionIndex;
				_versionDifference = versionDifference;
			}

			/// <summary>
			/// Returns true if the reference was matched with the identity of the assembly being built.
			/// </summary>
			public bool BoundToAssemblyBeingBuilt
			{
				get { return _definitionIndex == 0; }
			}

			/// <summary>
			/// True if the definition index is available (reference was successfully matched with the definition).
			/// </summary>
			public bool IsBound
			{
				get
				{
					return _definitionIndex >= 0;
				}
			}

			/// <summary>
			///  0 if the reference is equivalent to the definition.
			/// -1 if version of the matched definition is lower than version of the reference, but the reference otherwise matches the definition.
			/// +1 if version of the matched definition is higher than version of the reference, but the reference otherwise matches the definition.
			///   
			/// Undefined unless <see cref="IsBound"/> is true.
			/// </summary>
			public int VersionDifference
			{
				get
				{
					Debug.Assert(IsBound);
					return _versionDifference;
				}
			}

			/// <summary>
			/// Index into assembly definition list.
			/// Undefined unless <see cref="IsBound"/> is true.
			/// </summary>
			public int DefinitionIndex
			{
				get
				{
					Debug.Assert(IsBound);
					return _definitionIndex;
				}
			}

			public AssemblyIdentity ReferenceIdentity
			{
				get
				{
					return _referenceIdentity;
				}
			}

			private string GetDebuggerDisplay()
			{
				return IsBound ? ReferenceIdentity.GetDisplayName() + " -> #" + DefinitionIndex + (VersionDifference != 0 ? " VersionDiff=" + VersionDifference : "") : "unbound";
			}
		}

		public abstract class AssemblyData
		{
			/// <summary>
			/// Identity of the assembly.
			/// </summary>
			public abstract AssemblyIdentity Identity { get; }

			/// <summary>
			/// Identity of assemblies referenced by this assembly.
			/// References should always be returned in the same order.
			/// </summary>
			public abstract AssemblyIdentity[] AssemblyReferences { get; }

			/// <summary>
			/// The sequence of AssemblySymbols the Binder can choose from.
			/// </summary>
			public abstract TAssemblySymbol[] AvailableSymbols { get; }

			/// <summary>
			/// Check if provided AssemblySymbol is created for assembly described by this instance. 
			/// This method is expected to return true for every AssemblySymbol returned by 
			/// AvailableSymbols property.
			/// </summary>
			/// <param name="assembly">
			/// The AssemblySymbol to check.
			/// </param>
			/// <returns>Boolean.</returns>
			public abstract bool IsMatchingAssembly(TAssemblySymbol assembly);

			/// <summary>
			/// Resolve assembly references against assemblies described by provided AssemblyData objects. 
			/// In other words, match assembly identities returned by AssemblyReferences property against 
			/// assemblies described by provided AssemblyData objects.
			/// </summary>
			/// <param name="assemblies">An array of AssemblyData objects to match against.</param>
			/// <param name="assemblyIdentityComparer">Used to compare assembly identities.</param>
			/// <returns>
			/// For each assembly referenced by this assembly (<see cref="AssemblyReferences"/>) 
			/// a description of how it binds to one of the input assemblies.
			/// </returns>
			public abstract AssemblyReferenceBinding[] BindAssemblyReferences(AssemblyData[] assemblies);

			public abstract bool ContainsNoPiaLocalTypes { get; }

			public abstract bool IsLinked { get; }

			public abstract bool DeclaresTheObjectClass { get; }

			/// <summary>
			/// Get the source compilation backing this assembly, if one exists.
			/// Returns null otherwise.
			/// </summary>
			public abstract Compilation SourceCompilation { get; }

			private string GetDebuggerDisplay() => $"{GetType().Name}: [{Identity.GetDisplayName()}]";
		}

		/// <summary>
		/// If the compilation being built represents an assembly its assembly name.
		/// If the compilation being built represents a module, the name of the 
		/// containing assembly or <see cref="Compilation.UnspecifiedModuleAssemblyName"/>
		/// if not specified (/moduleassemblyname command line option).
		/// </summary>
		public readonly string SimpleAssemblyName;

		/// <summary>
		/// Metadata observed by the compiler.
		/// May be shared across multiple Reference Managers.
		/// Access only under lock(<see cref="ObservedMetadata"/>).
		/// </summary>
		public readonly Dictionary<MetadataReference, MetadataOrDiagnostic> ObservedMetadata;

		/// <summary>
		/// Once this is non-zero the state of the manager is fully initialized and immutable.
		/// </summary>
		private int _isBound;

		/// <summary>
		/// True if the compilation has a reference that refers back to the assembly being compiled.
		/// </summary>
		/// <remarks>
		/// If we have a circular reference the bound references can't be shared with other compilations.
		/// </remarks>
		private ThreeState _lazyHasCircularReference;

		/// <summary>
		/// A map from a metadata reference to an index to <see cref="_lazyReferencedAssemblies"/> array. Do not access
		/// directly, use <see cref="_lazyReferencedAssembliesMap"/> property instead.
		/// </summary>
		private Dictionary<MetadataReference, int> _lazyReferencedAssembliesMap;

		/// <summary>
		/// A map from a net-module metadata reference to the index of the corresponding module
		/// symbol in the source assembly symbol for the current compilation.
		/// </summary>
		/// <remarks>
		/// Subtract one from the index (for the manifest module) to find the corresponding elements
		/// of <see cref="_lazyReferencedModules"/> and <see cref="_lazyReferencedModulesReferences"/>.
		/// </remarks>
		private Dictionary<MetadataReference, int> _lazyReferencedModuleIndexMap;

		/// <summary>
		/// Maps (containing syntax tree file name, reference string) of #r directive to a resolved metadata reference.
		/// If multiple #r's in the same tree use the same value as a reference the resolved metadata reference is the same as well.
		/// </summary>
		private IDictionary<string, MetadataReference> _lazyReferenceDirectiveMap;

		/// <summary>
		/// Array of unique bound #r references.
		/// </summary>
		/// <remarks>
		/// The references are in the order they appear in syntax trees. This order is currently preserved 
		/// as syntax trees are added or removed, but we might decide to share reference manager between compilations
		/// with different order of #r's. It doesn't seem this would be an issue since all #r's within the compilation
		/// have the same "priority" with respect to each other.
		/// </remarks>
		private MetadataReference[] _lazyDirectiveReferences;

		private MetadataReference[] _lazyExplicitReferences;
		private MetadataReference[] _lazyImplicitReferences;

		/// <summary>
		/// Diagnostics produced during reference resolution and binding.
		/// </summary>
		/// <remarks>
		/// When reporting diagnostics be sure not to include any information that can't be shared among 
		/// compilations that share the same reference manager (such as full identity of the compilation, 
		/// simple assembly name is ok).
		/// </remarks>
		private Diagnostic[] _lazyDiagnostics;

		/// <summary>
		/// COR library symbol, or null if the compilation itself is the COR library.
		/// </summary>
		/// <remarks>
		/// If the compilation being built is the COR library we don't want to store its source assembly symbol 
		/// here since we wouldn't be able to share the state among subsequent compilations that are derived from it
		/// (each of them has its own source assembly symbol).
		/// </remarks>
		private TAssemblySymbol _lazyCorLibraryOpt;

		/// <summary>
		/// Standalone modules referenced by the compilation (doesn't include the manifest module of the compilation).
		/// </summary>
		/// <remarks>
		/// <see cref="_lazyReferencedModules"/>[i] corresponds to <see cref="_lazyReferencedModulesReferences"/>[i].
		/// </remarks>
		private PEModule[] _lazyReferencedModules;

		/// <summary>
		/// References of standalone modules referenced by the compilation (doesn't include the manifest module of the compilation).
		/// </summary>
		/// <remarks>
		/// <see cref="_lazyReferencedModules"/>[i] corresponds to <see cref="_lazyReferencedModulesReferences"/>[i].
		/// </remarks>
		private ModuleReferences<TAssemblySymbol>[] _lazyReferencedModulesReferences;

		/// <summary>
		/// Assemblies referenced directly by the source module of the compilation.
		/// </summary>
		private TAssemblySymbol[] _lazyReferencedAssemblies;

		/// <summary>
		/// Assemblies referenced directly by the source module of the compilation.
		/// </summary>
		/// <remarks>
		/// Aliases <see cref="_lazyAliasesOfReferencedAssemblies"/>[i] are of an assembly <see cref="_lazyReferencedAssemblies"/>[i].
		/// </remarks>
		private string[][] _lazyAliasesOfReferencedAssemblies;

		/// <summary>
		/// Unified assemblies referenced directly by the source module of the compilation.
		/// </summary>
		private UnifiedAssembly<TAssemblySymbol>[] _lazyUnifiedAssemblies;

		public CommonReferenceManager(string simpleAssemblyName, Dictionary<MetadataReference, MetadataOrDiagnostic> observedMetadata)
		{
			Debug.Assert(simpleAssemblyName != null);

			this.SimpleAssemblyName = simpleAssemblyName;
			this.ObservedMetadata = observedMetadata ?? new Dictionary<MetadataReference, MetadataOrDiagnostic>();
		}

		public Diagnostic[] Diagnostics
		{
			get
			{
				AssertBound();
				return _lazyDiagnostics;
			}
		}

		public bool HasCircularReference
		{
			get
			{
				AssertBound();
				return _lazyHasCircularReference == ThreeState.True;
			}
		}

		public Dictionary<MetadataReference, int> ReferencedAssembliesMap
		{
			get
			{
				AssertBound();
				return _lazyReferencedAssembliesMap;
			}
		}

		public Dictionary<MetadataReference, int> ReferencedModuleIndexMap
		{
			get
			{
				AssertBound();
				return _lazyReferencedModuleIndexMap;
			}
		}

		public IDictionary<string, MetadataReference> ReferenceDirectiveMap
		{
			get
			{
				AssertBound();
				return _lazyReferenceDirectiveMap;
			}
		}

		public MetadataReference[] DirectiveReferences
		{
			get
			{
				AssertBound();
				return _lazyDirectiveReferences;
			}
		}

		public override MetadataReference[] ImplicitReferences
		{
			get
			{
				AssertBound();
				return _lazyImplicitReferences;
			}
		}

		public override MetadataReference[] ExplicitReferences
		{
			get
			{
				AssertBound();
				return _lazyExplicitReferences;
			}
		}

		#region Symbols necessary to set up source assembly and module

		public TAssemblySymbol CorLibraryOpt
		{
			get
			{
				AssertBound();
				return _lazyCorLibraryOpt;
			}
		}

		public PEModule[] ReferencedModules
		{
			get
			{
				AssertBound();
				return _lazyReferencedModules;
			}
		}

		public ModuleReferences<TAssemblySymbol>[] ReferencedModulesReferences
		{
			get
			{
				AssertBound();
				return _lazyReferencedModulesReferences;
			}
		}

		public TAssemblySymbol[] ReferencedAssemblies
		{
			get
			{
				AssertBound();
				return _lazyReferencedAssemblies;
			}
		}

		public string[][] AliasesOfReferencedAssemblies
		{
			get
			{
				AssertBound();
				return _lazyAliasesOfReferencedAssemblies;
			}
		}

		public UnifiedAssembly<TAssemblySymbol>[] UnifiedAssemblies
		{
			get
			{
				AssertBound();
				return _lazyUnifiedAssemblies;
			}
		}

		#endregion

		/// <summary>
		/// Call only while holding <see cref="CommonReferenceManager.SymbolCacheAndReferenceManagerStateGuard"/>.
		/// </summary>
		[Conditional("DEBUG")]
		public void AssertUnbound()
		{
			Debug.Assert(_isBound == 0);
			Debug.Assert(_lazyHasCircularReference == ThreeState.Unknown);
			Debug.Assert(_lazyReferencedAssembliesMap == null);
			Debug.Assert(_lazyReferencedModuleIndexMap == null);
			Debug.Assert(_lazyReferenceDirectiveMap == null);
			Debug.Assert(_lazyDirectiveReferences == null);
			Debug.Assert(_lazyImplicitReferences == null);
			Debug.Assert(_lazyExplicitReferences == null);
			Debug.Assert(_lazyReferencedModules == null);
			Debug.Assert(_lazyReferencedModulesReferences == null);
			Debug.Assert(_lazyReferencedAssemblies == null);
			Debug.Assert(_lazyAliasesOfReferencedAssemblies == null);
			Debug.Assert(_lazyUnifiedAssemblies == null);
			Debug.Assert(_lazyCorLibraryOpt == null);
		}

		[Conditional("DEBUG")]
		public void AssertBound()
		{
			Debug.Assert(_isBound != 0);
			Debug.Assert(_lazyHasCircularReference != ThreeState.Unknown);
			Debug.Assert(_lazyReferencedAssembliesMap != null);
			Debug.Assert(_lazyReferencedModuleIndexMap != null);
			Debug.Assert(_lazyReferenceDirectiveMap != null);
			Debug.Assert(_lazyDirectiveReferences != null);
			Debug.Assert(_lazyImplicitReferences != null);
			Debug.Assert(_lazyExplicitReferences != null);
			Debug.Assert(_lazyReferencedModules != null);
			Debug.Assert(_lazyReferencedModulesReferences != null);
			Debug.Assert(_lazyReferencedAssemblies != null);
			Debug.Assert(_lazyAliasesOfReferencedAssemblies != null);
			Debug.Assert(_lazyUnifiedAssemblies != null);

			// lazyCorLibrary is null if the compilation is corlib
			Debug.Assert(_lazyReferencedAssemblies.Length == 0 || _lazyCorLibraryOpt != null);
		}

		[Conditional("DEBUG")]
		public void AssertCanReuseForCompilation(TCompilation compilation)
		{
			Debug.Assert(compilation.MakeSourceAssemblySimpleName() == this.SimpleAssemblyName);
		}

		public bool IsBound
		{
			get
			{
				return _isBound != 0;
			}
		}

		/// <summary>
		/// Call only while holding <see cref="CommonReferenceManager.SymbolCacheAndReferenceManagerStateGuard"/>.
		/// </summary>
		public void InitializeNoLock(
			Dictionary<MetadataReference, int> referencedAssembliesMap,
			Dictionary<MetadataReference, int> referencedModulesMap,
			IDictionary<string, MetadataReference> boundReferenceDirectiveMap,
			MetadataReference[] directiveReferences,
			MetadataReference[] explicitReferences,
			MetadataReference[] implicitReferences,
			bool containsCircularReferences,
			Diagnostic[] diagnostics,
			TAssemblySymbol corLibraryOpt,
			PEModule[] referencedModules,
			ModuleReferences<TAssemblySymbol>[] referencedModulesReferences,
			TAssemblySymbol[] referencedAssemblies,
			string[][] aliasesOfReferencedAssemblies,
			UnifiedAssembly<TAssemblySymbol>[] unifiedAssemblies)
		{
			AssertUnbound();

			Debug.Assert(referencedModules.Length == referencedModulesReferences.Length);
			Debug.Assert(referencedModules.Length == referencedModulesMap.Count);
			Debug.Assert(referencedAssemblies.Length == aliasesOfReferencedAssemblies.Length);

			_lazyReferencedAssembliesMap = referencedAssembliesMap;
			_lazyReferencedModuleIndexMap = referencedModulesMap;
			_lazyDiagnostics = diagnostics;
			_lazyReferenceDirectiveMap = boundReferenceDirectiveMap;
			_lazyDirectiveReferences = directiveReferences;
			_lazyExplicitReferences = explicitReferences;
			_lazyImplicitReferences = implicitReferences;

			_lazyCorLibraryOpt = corLibraryOpt;
			_lazyReferencedModules = referencedModules;
			_lazyReferencedModulesReferences = referencedModulesReferences;
			_lazyReferencedAssemblies = referencedAssemblies;
			_lazyAliasesOfReferencedAssemblies = aliasesOfReferencedAssemblies;
			_lazyUnifiedAssemblies = unifiedAssemblies;
			_lazyHasCircularReference = containsCircularReferences.ToThreeState();

			// once we flip this bit the state of the manager is immutable and available to any readers:
			_isBound = 1;
		}

		/// <summary>
		/// Global namespaces of assembly references that have been superseded by an assembly reference with a higher version are 
		/// hidden behind <see cref="s_supersededAlias"/> to avoid ambiguity when they are accessed from source.
		/// All existing aliases of a superseded assembly are discarded.
		/// </summary>
		private static readonly string[] s_supersededAlias = { "<superseded>" };

		protected static void BuildReferencedAssembliesAndModulesMaps(
			BoundInputAssembly[] bindingResult,
			MetadataReference[] references,
			ResolvedReference[] referenceMap,
			int referencedModuleCount,
			int explicitlyReferencedAssemblyCount,
			Dictionary<string, List<ReferencedAssemblyIdentity>> assemblyReferencesBySimpleName,
			bool supersedeLowerVersions,
			out Dictionary<MetadataReference, int> referencedAssembliesMap,
			out Dictionary<MetadataReference, int> referencedModulesMap,
			out string[][] aliasesOfReferencedAssemblies)
		{
			referencedAssembliesMap = new Dictionary<MetadataReference, int>(referenceMap.Length);
			referencedModulesMap = new Dictionary<MetadataReference, int>(referencedModuleCount);
			var aliasesOfReferencedAssembliesBuilder = new List<string[]>(referenceMap.Length - referencedModuleCount);
			bool hasRecursiveAliases = false;

			for (int i = 0; i < referenceMap.Length; i++)
			{
				if (referenceMap[i].IsSkipped)
				{
					continue;
				}

				if (referenceMap[i].Kind == MetadataImageKind.Module)
				{
					// add 1 for the manifest module:
					int moduleIndex = 1 + referenceMap[i].Index;
					referencedModulesMap.Add(references[i], moduleIndex);
				}
				else
				{
					// index into assembly data array
					int assemblyIndex = referenceMap[i].Index;
					Debug.Assert(aliasesOfReferencedAssembliesBuilder.Count == assemblyIndex);

					referencedAssembliesMap.Add(references[i], assemblyIndex);
					aliasesOfReferencedAssembliesBuilder.Add(referenceMap[i].AliasesOpt);

					hasRecursiveAliases |= referenceMap[i].RecursiveAliasesOpt != null;
				}
			}

			if (hasRecursiveAliases)
			{
				PropagateRecursiveAliases(bindingResult, referenceMap, aliasesOfReferencedAssembliesBuilder);
			}

			Debug.Assert(!aliasesOfReferencedAssembliesBuilder.Any(a => a == null));

			if (supersedeLowerVersions)
			{
				foreach (var assemblyReference in assemblyReferencesBySimpleName)
				{
					// the item in the list is the highest version, by construction
					for (int i = 1; i < assemblyReference.Value.Count; i++)
					{
						int assemblyIndex = assemblyReference.Value[i].GetAssemblyIndex(explicitlyReferencedAssemblyCount);
						aliasesOfReferencedAssembliesBuilder[assemblyIndex] = s_supersededAlias;
					}
				}
			}

			aliasesOfReferencedAssemblies = aliasesOfReferencedAssembliesBuilder.ToArray();
		}

		/// <summary>
		/// Calculates map from the identities of specified symbols to the corresponding identities in the original EnC baseline metadata.
		/// The map only includes an entry for identities that differ, i.e. for symbols representing assembly references of the current compilation that have different identities 
		/// than the corresponding identity in baseline metadata AssemblyRef table. The key comparer of the map ignores build and revision parts of the version number, 
		/// since these might change if the original version included wildcard.
		/// </summary>
		/// <param name="symbols">Assembly symbols for references of the current compilation.</param>
		/// <param name="originalIdentities">Identities in the baseline. <paramref name="originalIdentities"/>[i] corresponds to <paramref name="symbols"/>[i].</param>
		public static Dictionary<AssemblyIdentity, AssemblyIdentity> GetAssemblyReferenceIdentityBaselineMap(TAssemblySymbol[] symbols, AssemblyIdentity[] originalIdentities)
		{
			Debug.Assert(originalIdentities.Length == symbols.Length);

			Dictionary<AssemblyIdentity, AssemblyIdentity> lazyBuilder = new Dictionary<AssemblyIdentity, AssemblyIdentity>();
			for (int i = 0; i < originalIdentities.Length; i++)
			{
				var symbolIdentity = symbols[i].Identity;
				var versionPattern = symbols[i].AssemblyVersionPattern;
				var originalIdentity = originalIdentities[i];

				if ((object)versionPattern != null)
				{
					Debug.Assert(versionPattern.Build == ushort.MaxValue || versionPattern.Revision == ushort.MaxValue);

					var sourceIdentity = symbolIdentity.WithVersion(versionPattern);

					if (lazyBuilder.ContainsKey(sourceIdentity))
					{
						// The compilation references multiple assemblies whose versions only differ in auto-generated build and/or revision numbers.
						throw new NotSupportedException("@CompilationReferencesAssembliesWithDifferentAutoGeneratedVersion");
					}

					lazyBuilder.Add(sourceIdentity, originalIdentity);
				}
				else
				{
					// by construction of the arguments:
					Debug.Assert(originalIdentity == symbolIdentity);
				}
			}

			return lazyBuilder;
		}

		public static bool CompareVersionPartsSpecifiedInSource(Version version, Version candidateVersion, TAssemblySymbol candidateSymbol)
		{
			// major and minor parts must match exactly

			if (version.Major != candidateVersion.Major || version.Minor != candidateVersion.Minor)
			{
				return false;
			}

			// build and revision parts can differ only if the corresponding source versions were auto-generated:
			var versionPattern = candidateSymbol.AssemblyVersionPattern;
			Debug.Assert((object)versionPattern == null || versionPattern.Build == ushort.MaxValue || versionPattern.Revision == ushort.MaxValue);

			if (((object)versionPattern == null || versionPattern.Build < ushort.MaxValue) && version.Build != candidateVersion.Build)
			{
				return false;
			}

			if ((object)versionPattern == null && version.Revision != candidateVersion.Revision)
			{
				return false;
			}

			return true;
		}

		// #r references are recursive, their aliases should be merged into all their dependencies.
		//
		// For example, if a compilation has a reference to LibA with alias A and the user #r's LibB with alias B,
		// which references LibA, LibA should be available under both aliases A and B. B is usually "global",
		// which means LibA namespaces should become available to the compilation without any qualification when #r LibB 
		// is encountered.
		// 
		// Pairs: (assembly index -- index into bindingResult array; index of the #r reference in referenceMap array).
		private static void PropagateRecursiveAliases(
			BoundInputAssembly[] bindingResult,
			ResolvedReference[] referenceMap,
			List<string[]> aliasesOfReferencedAssembliesBuilder)
		{
			var assemblyIndicesToProcess = new List<int>();
			var visitedAssemblies = BitVector.Create(bindingResult.Length);

			// +1 for assembly being built
			Debug.Assert(bindingResult.Length == aliasesOfReferencedAssembliesBuilder.Count + 1);

			foreach (ResolvedReference reference in referenceMap)
			{
				if (!reference.IsSkipped && reference.RecursiveAliasesOpt != null)
				{
					var recursiveAliases = reference.RecursiveAliasesOpt;

					Debug.Assert(reference.Kind == MetadataImageKind.Assembly);
					visitedAssemblies.Clear();

					Debug.Assert(assemblyIndicesToProcess.Count == 0);
					assemblyIndicesToProcess.Add(reference.Index);

					while (assemblyIndicesToProcess.Count > 0)
					{
						int assemblyIndex = assemblyIndicesToProcess[assemblyIndicesToProcess.Count - 1];
						assemblyIndicesToProcess.RemoveAt(assemblyIndicesToProcess.Count - 1);
						visitedAssemblies[assemblyIndex] = true;

						// merge aliases:
						aliasesOfReferencedAssembliesBuilder[assemblyIndex] = MergedAliases.Merge(aliasesOfReferencedAssembliesBuilder[assemblyIndex], recursiveAliases);

						// push dependencies onto the stack:
						// +1 for the assembly being built:
						foreach (var binding in bindingResult[assemblyIndex + 1].ReferenceBinding)
						{
							if (binding.IsBound)
							{
								// -1 for the assembly being built:
								int dependentAssemblyIndex = binding.DefinitionIndex - 1;
								if (!visitedAssemblies[dependentAssemblyIndex])
								{
									assemblyIndicesToProcess.Add(dependentAssemblyIndex);
								}
							}
						}
					}
				}
			}

			for (int i = 0; i < aliasesOfReferencedAssembliesBuilder.Count; i++)
			{
				if (aliasesOfReferencedAssembliesBuilder[i] == null)
				{
					aliasesOfReferencedAssembliesBuilder[i] = new string[0];
				}
			}
		}

		#region Compilation APIs Implementation

		// for testing purposes
		public IEnumerable<string> ExternAliases => AliasesOfReferencedAssemblies.SelectMany(aliases => aliases);

		public sealed override IEnumerable<KeyValuePair<MetadataReference, IAssemblySymbol>> GetReferencedAssemblies()
		{
			return ReferencedAssembliesMap.Select(ra => KeyValuePairUtil.Create(ra.Key, (IAssemblySymbol)ReferencedAssemblies[ra.Value]));
		}

		public TAssemblySymbol GetReferencedAssemblySymbol(MetadataReference reference)
		{
			int index;
			return ReferencedAssembliesMap.TryGetValue(reference, out index) ? ReferencedAssemblies[index] : null;
		}

		public int GetReferencedModuleIndex(MetadataReference reference)
		{
			int index;
			return ReferencedModuleIndexMap.TryGetValue(reference, out index) ? index : -1;
		}

		/// <summary>
		/// Gets the <see cref="MetadataReference"/> that corresponds to the assembly symbol. 
		/// </summary>
		public override MetadataReference GetMetadataReference(IAssemblySymbol assemblySymbol)
		{
			foreach (var entry in ReferencedAssembliesMap)
			{
				if ((object)ReferencedAssemblies[entry.Value] == assemblySymbol)
				{
					return entry.Key;
				}
			}

			return null;
		}

		public override IEnumerable<ReferencedAssemblieInfo> GetReferencedAssemblyAliases()
		{
			for (int i = 0; i < ReferencedAssemblies.Length; i++)
			{
				ReferencedAssemblieInfo info = new ReferencedAssemblieInfo();
				info.assemblie = (IAssemblySymbol)ReferencedAssemblies[i];
				info.names = AliasesOfReferencedAssemblies[i];
				yield return info;
			}
		}

		public bool DeclarationsAccessibleWithoutAlias(int referencedAssemblyIndex)
		{
			var aliases = AliasesOfReferencedAssemblies[referencedAssemblyIndex];
			return aliases.Length == 0 || aliases.IndexOf(MetadataReferenceProperties.GlobalAlias) >= 0;
		}

		public override IEnumerable<KeyValuePair<AssemblyIdentity, PortableExecutableReference>> GetImplicitlyResolvedAssemblyReferences()
		{
			foreach (PortableExecutableReference reference in ImplicitReferences)
			{
				yield return KeyValuePairUtil.Create(ReferencedAssemblies[ReferencedAssembliesMap[reference]].Identity, reference);
			}
		}

		#endregion

		protected abstract CommonMessageProvider MessageProvider { get; }

		protected abstract AssemblyData CreateAssemblyDataForFile(
			PEAssembly assembly,
			List<IAssemblySymbol> cachedSymbols,
			string sourceAssemblySimpleName,
			MetadataImportOptions importOptions,
			bool embedInteropTypes);

		protected abstract AssemblyData CreateAssemblyDataForCompilation(
			CompilationReference compilationReference);

		/// <summary>
		/// Checks if the properties of <paramref name="duplicateReference"/> are compatible with properties of <paramref name="primaryReference"/>.
		/// Reports inconsistencies to the given diagnostic bag.
		/// </summary>
		/// <returns>True if the properties are compatible and hence merged, false if the duplicate reference should not merge it's properties with primary reference.</returns>
		protected abstract bool CheckPropertiesConsistency(MetadataReference primaryReference, MetadataReference duplicateReference, DiagnosticBag diagnostics);

		/// <summary>
		/// Called to compare two weakly named identities with the same name.
		/// </summary>
		protected abstract bool WeakIdentityPropertiesEquivalent(AssemblyIdentity identity1, AssemblyIdentity identity2);

		[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
		protected struct ResolvedReference
		{
			private readonly MetadataImageKind _kind;
			private readonly int _index;
			private readonly string[] _aliasesOpt;
			private readonly string[] _recursiveAliasesOpt;

			// uninitialized aliases
			public ResolvedReference(int index, MetadataImageKind kind)
			{
				Debug.Assert(index >= 0);
				_index = index + 1;
				_kind = kind;
				_aliasesOpt = null;
				_recursiveAliasesOpt = null;
			}

			// initialized aliases
			public ResolvedReference(int index, MetadataImageKind kind, string[] aliasesOpt, string[] recursiveAliasesOpt)
				: this(index, kind)
			{
				// We have to have non-default aliases (empty are ok). We can have both recursive and non-recursive aliases if two references were merged.
				Debug.Assert(aliasesOpt != null || recursiveAliasesOpt != null);

				_aliasesOpt = aliasesOpt;
				_recursiveAliasesOpt = recursiveAliasesOpt;
			}

			private bool IsUninitialized => _aliasesOpt == null && _recursiveAliasesOpt == null;

			/// <summary>
			/// Aliases that should be applied to the referenced assembly. 
			/// Empty array means {"global"} (all namespaces and types in the global namespace of the assembly are accessible without qualification).
			/// Null if not applicable (the reference only has recursive aliases).
			/// </summary>
			public string[] AliasesOpt
			{
				get
				{
					Debug.Assert(!IsUninitialized);
					return _aliasesOpt;
				}
			}

			/// <summary>
			/// Aliases that should be applied recursively to all dependent assemblies. 
			/// Empty array means {"global"} (all namespaces and types in the global namespace of the assembly are accessible without qualification).
			/// Null if not applicable (the reference only has simple aliases).
			/// </summary>
			public string[] RecursiveAliasesOpt
			{
				get
				{
					Debug.Assert(!IsUninitialized);
					return _recursiveAliasesOpt;
				}
			}

			/// <summary>
			/// default(<see cref="ResolvedReference"/>) is considered skipped.
			/// </summary>
			public bool IsSkipped
			{
				get
				{
					return _index == 0;
				}
			}

			public MetadataImageKind Kind
			{
				get
				{
					Debug.Assert(!IsSkipped);
					return _kind;
				}
			}

			/// <summary>
			/// Index into an array of assemblies (not including the assembly being built) or an array of modules, depending on <see cref="Kind"/>.
			/// </summary>
			public int Index
			{
				get
				{
					Debug.Assert(!IsSkipped);
					return _index - 1;
				}
			}

			private string GetDebuggerDisplay()
			{
				return IsSkipped ? "<skipped>" : $"{(_kind == MetadataImageKind.Assembly ? "A" : "M")}[{Index}]:{DisplayAliases(_aliasesOpt, "aliases")}{DisplayAliases(_recursiveAliasesOpt, "recursive-aliases")}";
			}

			private static string DisplayAliases(string[] aliasesOpt, string name)
			{
				return aliasesOpt == null ? "" : $" {name} = '{string.Join("','", aliasesOpt)}'";
			}
		}

		protected struct ReferencedAssemblyIdentity
		{
			public readonly AssemblyIdentity Identity;
			public readonly MetadataReference Reference;

			/// <summary>
			/// non-negative: Index into the array of all (explicitly and implicitly) referenced assemblies.
			/// negative: ExplicitlyReferencedAssemblies.Count + RelativeAssemblyIndex is an index into the array of assemblies.
			/// </summary>
			public readonly int RelativeAssemblyIndex;

			public int GetAssemblyIndex(int explicitlyReferencedAssemblyCount) =>
				RelativeAssemblyIndex >= 0 ? RelativeAssemblyIndex : explicitlyReferencedAssemblyCount + RelativeAssemblyIndex;

			public ReferencedAssemblyIdentity(AssemblyIdentity identity, MetadataReference reference, int relativeAssemblyIndex)
			{
				Identity = identity;
				Reference = reference;
				RelativeAssemblyIndex = relativeAssemblyIndex;
			}
		}

		/// <summary>
		/// Resolves given metadata references to assemblies and modules.
		/// </summary>
		/// <param name="compilation">The compilation whose references are being resolved.</param>
		/// <param name="assemblyReferencesBySimpleName">
		/// Used to filter out assemblies that have the same strong or weak identity.
		/// Maps simple name to a list of identities. The highest version of each name is the first.
		/// </param>
		/// <param name="references">List where to store resolved references. References from #r directives will follow references passed to the compilation constructor.</param>
		/// <param name="boundReferenceDirectiveMap">Maps #r values to successfully resolved metadata references. Does not contain values that failed to resolve.</param>
		/// <param name="boundReferenceDirectives">Unique metadata references resolved from #r directives.</param>
		/// <param name="assemblies">List where to store information about resolved assemblies to.</param>
		/// <param name="modules">List where to store information about resolved modules to.</param>
		/// <param name="diagnostics">Diagnostic bag where to report resolution errors.</param>
		/// <returns>
		/// Maps index to <paramref name="references"/> to an index of a resolved assembly or module in <paramref name="assemblies"/> or <paramref name="modules"/>, respectively.
		///</returns>
		protected ResolvedReference[] ResolveMetadataReferences(
			TCompilation compilation,
			Dictionary<string, List<ReferencedAssemblyIdentity>> assemblyReferencesBySimpleName,
			out MetadataReference[] references,
			out IDictionary<string, MetadataReference> boundReferenceDirectiveMap,
			out MetadataReference[] boundReferenceDirectives,
			out AssemblyData[] assemblies,
			out PEModule[] modules,
			DiagnosticBag diagnostics)
		{
			// Locations of all #r directives in the order they are listed in the references list.
			Location[] referenceDirectiveLocations;
			GetCompilationReferences(compilation, diagnostics, out references, out boundReferenceDirectiveMap, out referenceDirectiveLocations);

			// References originating from #r directives precede references supplied as arguments of the compilation.
			int referenceCount = references.Length;
			int referenceDirectiveCount = (referenceDirectiveLocations != null ? referenceDirectiveLocations.Length : 0);

			var referenceMap = new ResolvedReference[referenceCount];

			// Maps references that were added to the reference set (i.e. not filtered out as duplicates) to a set of names that 
			// can be used to alias these references. Duplicate assemblies contribute their aliases into this set.
			Dictionary<MetadataReference, MergedAliases> lazyAliasMap = null;

			// Used to filter out duplicate references that reference the same file (resolve to the same full normalized path).
			var boundReferences = new Dictionary<MetadataReference, MetadataReference>(MetadataReferenceEqualityComparer.Instance);

			List<MetadataReference> uniqueDirectiveReferences = (referenceDirectiveLocations != null) ? new List<MetadataReference>() : null;
			var assembliesBuilder = new List<AssemblyData>();
			List<PEModule> lazyModulesBuilder = null;

			// When duplicate references with conflicting EmbedInteropTypes flag are encountered,
			// VB uses the flag from the last one, C# reports an error. We need to enumerate in reverse order
			// so that we find the one that matters first.
			for (int referenceIndex = referenceCount - 1; referenceIndex >= 0; referenceIndex--)
			{
				var boundReference = references[referenceIndex];
				if (boundReference == null)
				{
					continue;
				}

				// add bound reference if it doesn't exist yet, merging aliases:
				MetadataReference existingReference;
				if (boundReferences.TryGetValue(boundReference, out existingReference))
				{
					// merge properties of compilation-based references if the underlying compilations are the same
					if ((object)boundReference != existingReference)
					{
						MergeReferenceProperties(existingReference, boundReference, diagnostics, ref lazyAliasMap);
					}

					continue;
				}

				boundReferences.Add(boundReference, boundReference);

				Location location;
				if (referenceIndex < referenceDirectiveCount)
				{
					location = referenceDirectiveLocations[referenceIndex];
					uniqueDirectiveReferences.Add(boundReference);
				}
				else
				{
					location = Location.None;
				}

				// compilation reference

				var compilationReference = boundReference as CompilationReference;
				if (compilationReference != null)
				{
					switch (compilationReference.Properties.Kind)
					{
						case MetadataImageKind.Assembly:
							existingReference = TryAddAssembly(
								compilationReference.Compilation.Assembly.Identity,
								boundReference,
								-assembliesBuilder.Count - 1,
								diagnostics,
								location,
								assemblyReferencesBySimpleName);

							if (existingReference != null)
							{
								MergeReferenceProperties(existingReference, boundReference, diagnostics, ref lazyAliasMap);
								continue;
							}

							// Note, if SourceAssemblySymbol hasn't been created for 
							// compilationAssembly.Compilation yet, we want this to happen 
							// right now. Conveniently, this constructor will trigger creation of the 
							// SourceAssemblySymbol.
							var asmData = CreateAssemblyDataForCompilation(compilationReference);
							AddAssembly(asmData, referenceIndex, referenceMap, assembliesBuilder);
							break;

						default:
							throw ExceptionUtilities.UnexpectedValue(compilationReference.Properties.Kind);
					}

					continue;
				}

				// PE reference

				var peReference = (PortableExecutableReference)boundReference;
				Metadata metadata = GetMetadata(peReference, MessageProvider, location, diagnostics);
				Debug.Assert(metadata != null || diagnostics.HasAnyErrors());

				if (metadata != null)
				{
					Debug.Assert(metadata != null);

					switch (peReference.Properties.Kind)
					{
						case MetadataImageKind.Assembly:
							var assemblyMetadata = (AssemblyMetadata)metadata;
							List<IAssemblySymbol> cachedSymbols = assemblyMetadata.CachedSymbols;

							if (assemblyMetadata.IsValidAssembly())
							{
								PEAssembly assembly = assemblyMetadata.GetAssembly();
								existingReference = TryAddAssembly(
									assembly.Identity,
									peReference,
									-assembliesBuilder.Count - 1,
									diagnostics,
									location,
									assemblyReferencesBySimpleName);

								if (existingReference != null)
								{
									MergeReferenceProperties(existingReference, boundReference, diagnostics, ref lazyAliasMap);
									continue;
								}

								var asmData = CreateAssemblyDataForFile(
									assembly,
									cachedSymbols,
									SimpleAssemblyName,
									compilation.Options.MetadataImportOptions,
									peReference.Properties.EmbedInteropTypes);

								AddAssembly(asmData, referenceIndex, referenceMap, assembliesBuilder);
							}
							else
							{
								diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_MetadataFileNotAssembly, location, peReference.Display));
							}

							// asmData keeps strong ref after this point
							GC.KeepAlive(assemblyMetadata);
							break;

						case MetadataImageKind.Module:
							var moduleMetadata = (ModuleMetadata)metadata;
							if (moduleMetadata.Module.IsLinkedModule)
							{
								// We don't support netmodules since some checks in the compiler need information from the full PE image
								// (Machine, Bit32Required, PE image hash).
								if (!moduleMetadata.Module.IsEntireImageAvailable)
								{
									diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_LinkedNetmoduleMetadataMustProvideFullPEImage, location, peReference.Display));
								}

								AddModule(moduleMetadata.Module, referenceIndex, referenceMap, ref lazyModulesBuilder);
							}
							else
							{
								diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_MetadataFileNotModule, location, peReference.Display));
							}
							break;

						default:
							throw ExceptionUtilities.UnexpectedValue(peReference.Properties.Kind);
					}
				}
			}

			if (uniqueDirectiveReferences != null)
			{
				uniqueDirectiveReferences.Reverse();
				boundReferenceDirectives = uniqueDirectiveReferences.ToArray();
			}
			else
			{
				boundReferenceDirectives = new MetadataReference[0];
			}

			// We enumerated references in reverse order in the above code
			// and thus assemblies and modules in the builders are reversed.
			// Fix up all the indices and reverse the builder content now to get 
			// the ordering matching the references.
			// 
			// Also fills in aliases.

			for (int i = 0; i < referenceMap.Length; i++)
			{
				if (!referenceMap[i].IsSkipped)
				{
					int count = (referenceMap[i].Kind == MetadataImageKind.Assembly) ? assembliesBuilder.Count : lazyModulesBuilder?.Count ?? 0;

					int reversedIndex = count - 1 - referenceMap[i].Index;
					referenceMap[i] = GetResolvedReferenceAndFreePropertyMapEntry(references[i], reversedIndex, referenceMap[i].Kind, lazyAliasMap);
				}
			}

			assembliesBuilder.Reverse();
			assemblies = assembliesBuilder.ToArray();

			if (lazyModulesBuilder == null)
			{
				modules = new PEModule[0];
			}
			else
			{
				lazyModulesBuilder.Reverse();
				modules = lazyModulesBuilder.ToArray();
			}

			return referenceMap;
		}

		private static ResolvedReference GetResolvedReferenceAndFreePropertyMapEntry(MetadataReference reference, int index, MetadataImageKind kind, Dictionary<MetadataReference, MergedAliases> propertyMapOpt)
		{
			string[] aliasesOpt, recursiveAliasesOpt;

			MergedAliases mergedProperties;
			if (propertyMapOpt != null && propertyMapOpt.TryGetValue(reference, out mergedProperties))
			{
				aliasesOpt = mergedProperties.AliasesOpt?.ToArray() ?? null;
				recursiveAliasesOpt = mergedProperties.RecursiveAliasesOpt?.ToArray() ?? null;
			}
			else if (reference.Properties.HasRecursiveAliases)
			{
				aliasesOpt = null;
				recursiveAliasesOpt = reference.Properties.Aliases;
			}
			else
			{
				aliasesOpt = reference.Properties.Aliases;
				recursiveAliasesOpt = null;
			}

			return new ResolvedReference(index, kind, aliasesOpt, recursiveAliasesOpt);
		}

		/// <summary>
		/// Creates or gets metadata for PE reference.
		/// </summary>
		/// <remarks>
		/// If any of the following exceptions: <see cref="BadImageFormatException"/>, <see cref="FileNotFoundException"/>, <see cref="IOException"/>,
		/// are thrown while reading the metadata file, the exception is caught and an appropriate diagnostic stored in <paramref name="diagnostics"/>.
		/// </remarks>
		private Metadata GetMetadata(PortableExecutableReference peReference, CommonMessageProvider messageProvider, Location location, DiagnosticBag diagnostics)
		{
			Metadata existingMetadata;

			lock (ObservedMetadata)
			{
				if (TryGetObservedMetadata(peReference, diagnostics, out existingMetadata))
				{
					return existingMetadata;
				}
			}

			Metadata newMetadata;
			Diagnostic newDiagnostic = null;
			try
			{
				newMetadata = peReference.GetMetadataNoCopy();

				// make sure basic structure of the PE image is valid:
				var assemblyMetadata = newMetadata as AssemblyMetadata;
				if (assemblyMetadata != null)
				{
					bool dummy = assemblyMetadata.IsValidAssembly();
				}
				else
				{
					bool dummy = ((ModuleMetadata)newMetadata).Module.IsLinkedModule;
				}
			}
			catch (Exception e) when (e is BadImageFormatException || e is IOException)
			{
				newDiagnostic = PortableExecutableReference.ExceptionToDiagnostic(e, messageProvider, location, peReference.Display, peReference.Properties.Kind);
				newMetadata = null;
			}

			lock (ObservedMetadata)
			{
				if (TryGetObservedMetadata(peReference, diagnostics, out existingMetadata))
				{
					return existingMetadata;
				}

				if (newDiagnostic != null)
				{
					diagnostics.Add(newDiagnostic);
				}

				ObservedMetadata.Add(peReference, (MetadataOrDiagnostic)newMetadata ?? newDiagnostic);
				return newMetadata;
			}
		}

		private bool TryGetObservedMetadata(PortableExecutableReference peReference, DiagnosticBag diagnostics, out Metadata metadata)
		{
			MetadataOrDiagnostic existing;
			if (ObservedMetadata.TryGetValue(peReference, out existing))
			{
				Debug.Assert(existing is Metadata || existing is Diagnostic);

				metadata = existing as Metadata;
				if (metadata == null)
				{
					diagnostics.Add((Diagnostic)existing);
				}

				return true;
			}

			metadata = null;
			return false;
		}

		/// <summary>
		/// Determines whether references are the same. Compilation references are the same if they refer to the same compilation.
		/// Otherwise, references are represented by their object identities.
		/// </summary>
		public sealed class MetadataReferenceEqualityComparer : IEqualityComparer<MetadataReference>
		{
			public static readonly MetadataReferenceEqualityComparer Instance = new MetadataReferenceEqualityComparer();

			public bool Equals(MetadataReference x, MetadataReference y)
			{
				if (ReferenceEquals(x, y))
				{
					return true;
				}

				var cx = x as CompilationReference;
				if (cx != null)
				{
					var cy = y as CompilationReference;
					if (cy != null)
					{
						return (object)cx.Compilation == cy.Compilation;
					}
				}

				return false;
			}

			public int GetHashCode(MetadataReference reference)
			{
				var compilationReference = reference as CompilationReference;
				if (compilationReference != null)
				{
					return RuntimeHelpers.GetHashCode(compilationReference.Compilation);
				}

				return RuntimeHelpers.GetHashCode(reference);
			}
		}

		/// <summary>
		/// Merges aliases of the first observed reference (<paramref name="primaryReference"/>) with aliases specified for an equivalent reference (<paramref name="newReference"/>).
		/// Empty alias list is considered to be the same as a list containing "global", since in both cases C# allows unqualified access to the symbols.
		/// </summary>
		private void MergeReferenceProperties(MetadataReference primaryReference, MetadataReference newReference, DiagnosticBag diagnostics, ref Dictionary<MetadataReference, MergedAliases> lazyAliasMap)
		{
			if (!CheckPropertiesConsistency(newReference, primaryReference, diagnostics))
			{
				return;
			}

			if (lazyAliasMap == null)
			{
				lazyAliasMap = new Dictionary<MetadataReference, MergedAliases>();
			}

			MergedAliases mergedAliases;
			if (!lazyAliasMap.TryGetValue(primaryReference, out mergedAliases))
			{
				mergedAliases = new MergedAliases();
				lazyAliasMap.Add(primaryReference, mergedAliases);
				mergedAliases.Merge(primaryReference);
			}

			mergedAliases.Merge(newReference);
		}

		/// <remarks>
		/// Caller is responsible for freeing any allocated ArrayBuilders.
		/// </remarks>
		private static void AddAssembly(AssemblyData data, int referenceIndex, ResolvedReference[] referenceMap, List<AssemblyData> assemblies)
		{
			// aliases will be filled in later:
			referenceMap[referenceIndex] = new ResolvedReference(assemblies.Count, MetadataImageKind.Assembly);
			assemblies.Add(data);
		}

		/// <remarks>
		/// Caller is responsible for freeing any allocated ArrayBuilders.
		/// </remarks>
		private static void AddModule(PEModule module, int referenceIndex, ResolvedReference[] referenceMap, ref List<PEModule> modules)
		{
			if (modules == null)
			{
				modules = new List<PEModule>();
			}

			referenceMap[referenceIndex] = new ResolvedReference(modules.Count, MetadataImageKind.Module);
			modules.Add(module);
		}

		/// <summary>
		/// Returns null if an assembly of an equivalent identity has not been added previously, otherwise returns the reference that added it.
		/// Two identities are considered equivalent if
		/// - both assembly names are strong (have keys) and are either equal or FX unified 
		/// - both assembly names are weak (no keys) and have the same simple name.
		/// </summary>
		private MetadataReference TryAddAssembly(
			AssemblyIdentity identity,
			MetadataReference reference,
			int assemblyIndex,
			DiagnosticBag diagnostics,
			Location location,
			Dictionary<string, List<ReferencedAssemblyIdentity>> referencesBySimpleName)
		{
			var referencedAssembly = new ReferencedAssemblyIdentity(identity, reference, assemblyIndex);

			List<ReferencedAssemblyIdentity> sameSimpleNameIdentities;
			if (!referencesBySimpleName.TryGetValue(identity.Name, out sameSimpleNameIdentities))
			{
				referencesBySimpleName.Add(identity.Name, new List<ReferencedAssemblyIdentity> { referencedAssembly });
				return null;
			}

			ReferencedAssemblyIdentity equivalent = default(ReferencedAssemblyIdentity);
			foreach (var other in sameSimpleNameIdentities)
			{
				// only compare weak with weak
				if (WeakIdentityPropertiesEquivalent(identity, other.Identity))
				{
					equivalent = other;
					break;
				}
			}

			if (equivalent.Identity == null)
			{
				sameSimpleNameIdentities.Add(referencedAssembly);
				return null;
			}

			// Dev12 reports an error for all weak-named assemblies, even if the versions are the same.
			// We treat assemblies with the same name and version equal even if they don't have a strong name.
			// This change allows us to de-duplicate #r references based on identities rather than full paths,
			// and is closer to platforms that don't support strong names and consider name and version enough
			// to identify an assembly. An identity without version is considered to have version 0.0.0.0.

			if (identity != equivalent.Identity)
			{
				MessageProvider.ReportDuplicateMetadataReferenceWeak(diagnostics, location, reference, identity, equivalent.Reference, equivalent.Identity);
			}

			Debug.Assert(equivalent.Reference != null);
			return equivalent.Reference;
		}

		protected void GetCompilationReferences(
			TCompilation compilation,
			DiagnosticBag diagnostics,
			out MetadataReference[] references,
			out IDictionary<string, MetadataReference> boundReferenceDirectives,
			out Location[] referenceDirectiveLocations)
		{
			boundReferenceDirectives = new Dictionary<string, MetadataReference>();

			List<MetadataReference> referencesBuilder = new List<MetadataReference>();
			List<Location> referenceDirectiveLocationsBuilder = new List<Location>();

			foreach (var referenceDirective in compilation.ReferenceDirectives)
			{
				if (compilation.Options.MetadataReferenceResolver == null)
				{
					diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_MetadataReferencesNotSupported, referenceDirective.Location));
					break;
				}

				// we already successfully bound #r with the same value:
				if (boundReferenceDirectives != null && boundReferenceDirectives.ContainsKey(referenceDirective.Location.SourceTree.FilePath + referenceDirective.File))
				{
					continue;
				}

				MetadataReference boundReference = ResolveReferenceDirective(referenceDirective.File, referenceDirective.Location, compilation);
				if (boundReference == null)
				{
					diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_MetadataFileNotFound, referenceDirective.Location, referenceDirective.File));
					continue;
				}

				referencesBuilder.Add(boundReference);
				referenceDirectiveLocationsBuilder.Add(referenceDirective.Location);
				boundReferenceDirectives.Add(referenceDirective.Location.SourceTree.FilePath + referenceDirective.File, boundReference);
			}

			// add external reference at the end, so that they are processed first:
			referencesBuilder.AddRange(compilation.ExternalReferences);

			references = referencesBuilder.ToArray();
			referenceDirectiveLocations = referenceDirectiveLocationsBuilder.ToArray();
		}

		/// <summary>
		/// For each given directive return a bound PE reference, or null if the binding fails.
		/// </summary>
		private static PortableExecutableReference ResolveReferenceDirective(string reference, Location location, TCompilation compilation)
		{
			var tree = location.SourceTree;
			string basePath = (tree != null && tree.FilePath.Length > 0) ? tree.FilePath : null;

			// checked earlier:
			Debug.Assert(compilation.Options.MetadataReferenceResolver != null);

			var references = compilation.Options.MetadataReferenceResolver.ResolveReference(reference, basePath, MetadataReferenceProperties.Assembly.WithRecursiveAliases(true));
			if (references.Length == 0)
			{
				return null;
			}

			if (references.Length > 1)
			{
				// TODO: implement
				throw new NotSupportedException();
			}

			return references[0];
		}

		public static AssemblyReferenceBinding[] ResolveReferencedAssemblies(
			AssemblyIdentity[] references,
			AssemblyData[] definitions,
			int definitionStartIndex)
		{
			var boundReferences = new AssemblyReferenceBinding[references.Length];
			for (int j = 0; j < references.Length; j++)
			{
				boundReferences[j] = ResolveReferencedAssembly(references[j], definitions, definitionStartIndex);
			}

			return boundReferences;
		}

		/// <summary>
		/// Used to match AssemblyRef with AssemblyDef.
		/// </summary>
		/// <param name="definitions">Array of definition identities to match against.</param>
		/// <param name="definitionStartIndex">An index of the first definition to consider, <paramref name="definitions"/> preceding this index are ignored.</param>
		/// <param name="reference">Reference identity to resolve.</param>
		/// <param name="assemblyIdentityComparer">Assembly identity comparer.</param>
		/// <returns>
		/// Returns an index the reference is bound.
		/// </returns>
		public static AssemblyReferenceBinding ResolveReferencedAssembly(
			AssemblyIdentity reference,
			AssemblyData[] definitions,
			int definitionStartIndex)
		{
			// Dev11 C# compiler allows the versions to not match exactly, assuming that a newer library may be used instead of an older version.
			// For a given reference it finds a definition with the lowest version that is higher then or equal to the reference version.
			// If match.Version != reference.Version a warning is reported.

			// definition with the lowest version higher than reference version, unless exact version found
			int minHigherVersionDefinition = -1;
			int maxLowerVersionDefinition = -1;

			// Skip assembly being built for now; it will be considered at the very end:
			bool resolveAgainstAssemblyBeingBuilt = definitionStartIndex == 0;
			definitionStartIndex = Math.Max(definitionStartIndex, 1);

			for (int i = definitionStartIndex; i < definitions.Length; i++)
			{
				AssemblyIdentity definition = definitions[i].Identity;

				if (reference.Name == definition.Name)
				{
					return new AssemblyReferenceBinding(reference, i);
				}
			}

			// we haven't found definition that matches the reference

			if (minHigherVersionDefinition != -1)
			{
				return new AssemblyReferenceBinding(reference, minHigherVersionDefinition, versionDifference: +1);
			}

			if (maxLowerVersionDefinition != -1)
			{
				return new AssemblyReferenceBinding(reference, maxLowerVersionDefinition, versionDifference: -1);
			}

			// As in the native compiler (see IMPORTER::MapAssemblyRefToAid), we compare against the
			// compilation (i.e. source) assembly as a last resort.  We follow the native approach of
			// skipping the public key comparison since we have yet to compute it.
			if (resolveAgainstAssemblyBeingBuilt && reference.Name ==  definitions[0].Identity.Name)
			{
				return new AssemblyReferenceBinding(reference, 0);
			}

			return new AssemblyReferenceBinding(reference);
		}
	}
}
