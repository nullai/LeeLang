using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace leec
{
	public abstract class CommonPEModuleBuilder : IUnit, IModuleReference
	{
		public readonly IEnumerable<ResourceDescription> ManifestResources;
		public readonly ModulePropertiesForSerialization SerializationProperties;
		public readonly OutputKind OutputKind;
		public IEnumerable<IWin32Resource> Win32Resources;
		public ResourceSection Win32ResourceSection;
		public Stream SourceLinkStreamOpt;

		public IMethodReference PEEntryPoint;
		public IMethodReference DebugEntryPoint;

		private readonly Dictionary<IMethodSymbol, IMethodBody> _methodBodyMap;
		private readonly TokenMap<IReference> _referencesInILMap = new TokenMap<IReference>();
		private readonly ItemTokenMap<string> _stringsInILMap = new ItemTokenMap<string>();

		private AssemblyReferenceAlias[] _lazyAssemblyReferenceAliases;

		public CommonPEModuleBuilder(
			IEnumerable<ResourceDescription> manifestResources,
			EmitOptions emitOptions,
			OutputKind outputKind,
			ModulePropertiesForSerialization serializationProperties,
			Compilation compilation)
		{
			Debug.Assert(manifestResources != null);
			Debug.Assert(serializationProperties != null);
			Debug.Assert(compilation != null);

			ManifestResources = manifestResources;
			OutputKind = outputKind;
			SerializationProperties = serializationProperties;
			_methodBodyMap = new Dictionary<IMethodSymbol, IMethodBody>();
		}

		/// <summary>
		/// EnC generation.
		/// </summary>
		public abstract int CurrentGenerationOrdinal { get; }

		/// <summary>
		/// If this module represents an assembly, name of the assembly used in AssemblyDef table. Otherwise name of the module same as <see cref="ModuleName"/>.
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		/// Name of the module. Used in ModuleDef table.
		/// </summary>
		public abstract string ModuleName { get; }

		public abstract IAssemblyReference Translate(IAssemblySymbol symbol, DiagnosticBag diagnostics);
		public abstract ITypeReference Translate(ITypeSymbol symbol, GreenNode syntaxOpt, DiagnosticBag diagnostics);
		public abstract IMethodReference Translate(IMethodSymbol symbol, DiagnosticBag diagnostics, bool needDeclaration);
		public abstract bool SupportsPrivateImplClass { get; }
		public abstract INamespaceTypeDefinition[] GetAnonymousTypes(EmitContext context);
		public abstract Compilation CommonCompilation { get; }
		public abstract IModuleSymbol CommonSourceModule { get; }
		public abstract IAssemblySymbol CommonCorLibrary { get; }
		public abstract CommonModuleCompilationState CommonModuleCompilationState { get; }
		public abstract void CompilationFinished();
		public abstract Dictionary<ITypeDefinition, ITypeDefinitionMember[]> GetSynthesizedMembers();
		public abstract CommonEmbeddedTypesManager CommonEmbeddedTypesManagerOpt { get; }
		public abstract ITypeReference EncTranslateType(ITypeSymbol type, DiagnosticBag diagnostics);
		public abstract IEnumerable<ICustomAttribute> GetSourceAssemblyAttributes(bool isRefAssembly);
		public abstract IEnumerable<SecurityAttribute> GetSourceAssemblySecurityAttributes();
		public abstract IEnumerable<ICustomAttribute> GetSourceModuleAttributes();
		public abstract ICustomAttribute SynthesizeAttribute(WellKnownMember attributeConstructor);

		/// <summary>
		/// Public types defined in other modules making up this assembly and to which other assemblies may refer to via this assembly
		/// followed by types forwarded to another assembly.
		/// </summary>
		public abstract ExportedType[] GetExportedTypes(DiagnosticBag diagnostics);

		/// <summary>
		/// Used to distinguish which style to pick while writing native PDB information.
		/// </summary>
		/// <remarks>
		/// The PDB content for custom debug information is different between Visual Basic and CSharp.
		/// E.g. C# always includes a CustomMetadata Header (MD2) that contains the namespace scope counts, where 
		/// as VB only outputs namespace imports into the namespace scopes. 
		/// C# defines forwards in that header, VB includes them into the scopes list.
		/// 
		/// Currently the compiler doesn't allow mixing C# and VB method bodies. Thus this flag can be per module.
		/// It is possible to move this flag to per-method basis but native PDB CDI forwarding would need to be adjusted accordingly.
		/// </remarks>
		public abstract bool GenerateVisualBasicStylePdb { get; }

		/// <summary>
		/// Linked assembly names to be stored to native PDB (VB only).
		/// </summary>
		public abstract IEnumerable<string> LinkedAssembliesDebugInfo { get; }

		/// <summary>
		/// Project level imports (VB only, TODO: C# scripts).
		/// </summary>
		public abstract UsedNamespaceOrType[] GetImports();

		/// <summary>
		/// Default namespace (VB only).
		/// </summary>
		public abstract string DefaultNamespace { get; }

		protected abstract IAssemblyReference GetCorLibraryReferenceToEmit(EmitContext context);
		protected abstract IEnumerable<IAssemblyReference> GetAssemblyReferencesFromAddedModules(DiagnosticBag diagnostics);
		public abstract ITypeReference GetPlatformType(PlatformType platformType, EmitContext context);
		public abstract bool IsPlatformType(ITypeReference typeRef, PlatformType platformType);
		public abstract IEnumerable<INamespaceTypeDefinition> GetTopLevelTypes(EmitContext context);

		/// <summary>
		/// A list of the files that constitute the assembly. Empty for netmodule. These are not the source language files that may have been
		/// used to compile the assembly, but the files that contain constituent modules of a multi-module assembly as well
		/// as any external resources. It corresponds to the File table of the .NET assembly file format.
		/// </summary>
		public abstract IEnumerable<IFileReference> GetFiles(EmitContext context);

		public void Dispatch(MetadataVisitor visitor) => visitor.Visit(this);

		IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context) => new ICustomAttribute[0];

		IDefinition IReference.AsDefinition(EmitContext context)
		{
			Debug.Assert(ReferenceEquals(context.Module, this));
			return this;
		}

		public abstract ISourceAssemblySymbolInternal SourceAssemblyOpt { get; }

		/// <summary>
		/// An approximate number of method definitions that can
		/// provide a basis for approximating the capacities of
		/// various databases used during Emit.
		/// </summary>
		public int HintNumberOfMethodDefinitions
			// Try to guess at the size of tables to prevent re-allocation. The method body
			// map is pretty close, but unfortunately it tends to undercount. x1.5 seems like
			// a healthy amount of room based on compiling Roslyn.
			=> (int)(_methodBodyMap.Count * 1.5);

		public IMethodBody GetMethodBody(IMethodSymbol methodSymbol)
		{
			Debug.Assert(methodSymbol.ContainingModule == CommonSourceModule);
			Debug.Assert(methodSymbol.IsDefinition);
			Debug.Assert(methodSymbol.PartialDefinitionPart == null); // Must be definition.

			IMethodBody body;

			if (_methodBodyMap.TryGetValue(methodSymbol, out body))
			{
				return body;
			}

			return null;
		}

		public void SetMethodBody(IMethodSymbol methodSymbol, IMethodBody body)
		{
			Debug.Assert(methodSymbol.ContainingModule == CommonSourceModule);
			Debug.Assert(methodSymbol.IsDefinition);
			Debug.Assert(methodSymbol.PartialDefinitionPart == null); // Must be definition.
			Debug.Assert(body == null || (object)methodSymbol == body.MethodDefinition);

			_methodBodyMap.Add(methodSymbol, body);
		}

		public void SetPEEntryPoint(IMethodSymbol method, DiagnosticBag diagnostics)
		{
			Debug.Assert(method == null || IsSourceDefinition(method));
			Debug.Assert(OutputKind.IsApplication());

			PEEntryPoint = Translate(method, diagnostics, needDeclaration: true);
		}

		public void SetDebugEntryPoint(IMethodSymbol method, DiagnosticBag diagnostics)
		{
			Debug.Assert(method == null || IsSourceDefinition(method));

			DebugEntryPoint = Translate(method, diagnostics, needDeclaration: true);
		}

		private bool IsSourceDefinition(IMethodSymbol method)
		{
			return method.ContainingModule == CommonSourceModule && method.IsDefinition;
		}

		/// <summary>
		/// CorLibrary assembly referenced by this module.
		/// </summary>
		public IAssemblyReference GetCorLibrary(EmitContext context)
		{
			return Translate(CommonCorLibrary, context.Diagnostics);
		}

		public IAssemblyReference GetContainingAssembly(EmitContext context)
		{
			return OutputKind == OutputKind.NetModule ? null : (IAssemblyReference)this;
		}

		/// <summary>
		/// Returns User Strings referenced from the IL in the module. 
		/// </summary>
		public IEnumerable<string> GetStrings()
		{
			return _stringsInILMap.GetAllItems();
		}

		public uint GetFakeSymbolTokenForIL(IReference symbol, GreenNode syntaxNode, DiagnosticBag diagnostics)
		{
			bool added;
			uint token = _referencesInILMap.GetOrAddTokenFor(symbol, out added);
			if (added)
			{
				ReferenceDependencyWalker.VisitReference(symbol, new EmitContext(this, syntaxNode, diagnostics, metadataOnly: false, includePrivateMembers: true));
			}
			return token;
		}

		public IReference GetReferenceFromToken(uint token)
		{
			return _referencesInILMap.GetItem(token);
		}

		public uint GetFakeStringTokenForIL(string str)
		{
			return _stringsInILMap.GetOrAddTokenFor(str);
		}

		public string GetStringFromToken(uint token)
		{
			return _stringsInILMap.GetItem(token);
		}

		public IEnumerable<IReference> ReferencesInIL(out int count)
		{
			return _referencesInILMap.GetAllItemsAndCount(out count);
		}

		/// <summary>
		/// Assembly reference aliases (C# only).
		/// </summary>
		public AssemblyReferenceAlias[] GetAssemblyReferenceAliases(EmitContext context)
		{
			if (_lazyAssemblyReferenceAliases == null)
			{
				_lazyAssemblyReferenceAliases = CalculateAssemblyReferenceAliases(context);
			}

			return _lazyAssemblyReferenceAliases;
		}

		private AssemblyReferenceAlias[] CalculateAssemblyReferenceAliases(EmitContext context)
		{
			var result = new List<AssemblyReferenceAlias>();

			foreach (var assemblyAndAliases in CommonCompilation.GetBoundReferenceManager().GetReferencedAssemblyAliases())
			{
				var assembly = assemblyAndAliases.assemblie;
				var aliases = assemblyAndAliases.names;

				for (int i = 0; i < aliases.Length; i++)
				{
					string alias = aliases[i];

					// filter out duplicates and global aliases:
					if (alias != MetadataReferenceProperties.GlobalAlias && aliases.IndexOf(alias, 0, i) < 0)
					{
						result.Add(new AssemblyReferenceAlias(alias, Translate(assembly, context.Diagnostics)));
					}
				}
			}

			return result.ToArray();
		}

		public IEnumerable<IAssemblyReference> GetAssemblyReferences(EmitContext context)
		{
			IAssemblyReference corLibrary = GetCorLibraryReferenceToEmit(context);

			// Only add Cor Library reference explicitly, PeWriter will add
			// other references implicitly on as needed basis.
			if (corLibrary != null)
			{
				yield return corLibrary;
			}

			if (OutputKind != OutputKind.NetModule)
			{
				// Explicitly add references from added modules
				foreach (var aRef in GetAssemblyReferencesFromAddedModules(context.Diagnostics))
				{
					yield return aRef;
				}
			}
		}
	}

	/// <summary>
	/// Common base class for C# and VB PE module builder.
	/// </summary>
	public abstract class PEModuleBuilder<TCompilation, TSourceModuleSymbol, TAssemblySymbol, TTypeSymbol, TNamedTypeSymbol, TMethodSymbol, TGreenNode, TEmbeddedTypesManager, TModuleCompilationState> : CommonPEModuleBuilder, ITokenDeferral
		where TCompilation : Compilation
		where TSourceModuleSymbol : class, IModuleSymbol
		where TAssemblySymbol : class, IAssemblySymbol
		where TTypeSymbol : class
		where TNamedTypeSymbol : class, TTypeSymbol, INamespaceTypeDefinition
		where TMethodSymbol : class, IMethodDefinition
		where TGreenNode : GreenNode
		where TEmbeddedTypesManager : CommonEmbeddedTypesManager
		where TModuleCompilationState : ModuleCompilationState<TNamedTypeSymbol, TMethodSymbol>
	{
		private readonly RootModuleType _rootModuleType = new RootModuleType();

		public readonly TSourceModuleSymbol SourceModule;
		public readonly TCompilation Compilation;

		private PrivateImplementationDetails _privateImplementationDetails;
		private ArrayMethods _lazyArrayMethods;
		private HashSet<string> _namesOfTopLevelTypes;

		public readonly TModuleCompilationState CompilationState;

		public abstract TEmbeddedTypesManager EmbeddedTypesManagerOpt { get; }

		protected PEModuleBuilder(
			TCompilation compilation,
			TSourceModuleSymbol sourceModule,
			ModulePropertiesForSerialization serializationProperties,
			IEnumerable<ResourceDescription> manifestResources,
			OutputKind outputKind,
			EmitOptions emitOptions,
			TModuleCompilationState compilationState)
			: base(manifestResources, emitOptions, outputKind, serializationProperties, compilation)
		{
			Debug.Assert(sourceModule != null);
			Debug.Assert(serializationProperties != null);

			Compilation = compilation;
			SourceModule = sourceModule;
			this.CompilationState = compilationState;
		}

		public sealed override void CompilationFinished()
		{
			this.CompilationState.Freeze();
		}

		public override IAssemblySymbol CommonCorLibrary => CorLibrary;
		public abstract TAssemblySymbol CorLibrary { get; }

		public abstract INamedTypeReference GetSystemType(TGreenNode syntaxOpt, DiagnosticBag diagnostics);
		public abstract INamedTypeReference GetSpecialType(SpecialType specialType, TGreenNode syntaxNodeOpt, DiagnosticBag diagnostics);

		public sealed override ITypeReference EncTranslateType(ITypeSymbol type, DiagnosticBag diagnostics)
		{
			return EncTranslateLocalVariableType((TTypeSymbol)type, diagnostics);
		}

		public virtual ITypeReference EncTranslateLocalVariableType(TTypeSymbol type, DiagnosticBag diagnostics)
		{
			return Translate(type, null, diagnostics);
		}

		protected bool HaveDeterminedTopLevelTypes
		{
			get { return _namesOfTopLevelTypes != null; }
		}

		protected bool ContainsTopLevelType(string fullEmittedName)
		{
			return _namesOfTopLevelTypes.Contains(fullEmittedName);
		}

		public abstract IEnumerable<INamespaceTypeDefinition> GetTopLevelTypesCore(EmitContext context);

		/// <summary>
		/// Returns all top-level (not nested) types defined in the module. 
		/// </summary>
		public override IEnumerable<INamespaceTypeDefinition> GetTopLevelTypes(EmitContext context)
		{
			TypeReferenceIndexer typeReferenceIndexer = null;
			HashSet<string> names;

			// First time through, we need to collect emitted names of all top level types.
			if (_namesOfTopLevelTypes == null)
			{
				names = new HashSet<string>();
			}
			else
			{
				names = null;
			}

			// First time through, we need to push things through TypeReferenceIndexer
			// to make sure we collect all to be embedded NoPia types and members.
			if (EmbeddedTypesManagerOpt != null && !EmbeddedTypesManagerOpt.IsFrozen)
			{
				typeReferenceIndexer = new TypeReferenceIndexer(context);
				Debug.Assert(names != null);

				// Run this reference indexer on the assembly- and module-level attributes first.
				// We'll run it on all other types below.
				// The purpose is to trigger Translate on all types.
				this.Dispatch(typeReferenceIndexer);
			}

			AddTopLevelType(names, _rootModuleType);
			VisitTopLevelType(typeReferenceIndexer, _rootModuleType);
			yield return _rootModuleType;

			foreach (var type in this.GetAnonymousTypes(context))
			{
				AddTopLevelType(names, type);
				VisitTopLevelType(typeReferenceIndexer, type);
				yield return type;
			}

			foreach (var type in this.GetTopLevelTypesCore(context))
			{
				AddTopLevelType(names, type);
				VisitTopLevelType(typeReferenceIndexer, type);
				yield return type;
			}

			var privateImpl = this.PrivateImplClass;
			if (privateImpl != null)
			{
				AddTopLevelType(names, privateImpl);
				VisitTopLevelType(typeReferenceIndexer, privateImpl);
				yield return privateImpl;
			}

			if (EmbeddedTypesManagerOpt != null)
			{
				foreach (var embedded in EmbeddedTypesManagerOpt.GetTypes(context.Diagnostics, names))
				{
					AddTopLevelType(names, embedded);
					yield return embedded;
				}
			}

			if (names != null)
			{
				Debug.Assert(_namesOfTopLevelTypes == null);
				_namesOfTopLevelTypes = names;
			}
		}

		public abstract IAssemblyReference Translate(TAssemblySymbol symbol, DiagnosticBag diagnostics);
		public abstract ITypeReference Translate(TTypeSymbol symbol, TGreenNode syntaxNodeOpt, DiagnosticBag diagnostics);
		public abstract IMethodReference Translate(TMethodSymbol symbol, DiagnosticBag diagnostics, bool needDeclaration);

		public sealed override IAssemblyReference Translate(IAssemblySymbol symbol, DiagnosticBag diagnostics)
		{
			return Translate((TAssemblySymbol)symbol, diagnostics);
		}

		public sealed override ITypeReference Translate(ITypeSymbol symbol, GreenNode syntaxNodeOpt, DiagnosticBag diagnostics)
		{
			return Translate((TTypeSymbol)symbol, (TGreenNode)syntaxNodeOpt, diagnostics);
		}

		public sealed override IMethodReference Translate(IMethodSymbol symbol, DiagnosticBag diagnostics, bool needDeclaration)
		{
			return Translate((TMethodSymbol)symbol, diagnostics, needDeclaration);
		}

		public sealed override IModuleSymbol CommonSourceModule => SourceModule;
		public sealed override Compilation CommonCompilation => Compilation;
		public sealed override CommonModuleCompilationState CommonModuleCompilationState => CompilationState;
		public sealed override CommonEmbeddedTypesManager CommonEmbeddedTypesManagerOpt => EmbeddedTypesManagerOpt;

		public MetadataConstant CreateConstant(
			TTypeSymbol type,
			object value,
			TGreenNode syntaxNodeOpt,
			DiagnosticBag diagnostics)
		{
			return new MetadataConstant(Translate(type, syntaxNodeOpt, diagnostics), value);
		}

		private static void AddTopLevelType(HashSet<string> names, INamespaceTypeDefinition type)
		{
			names?.Add(MetadataHelpers.BuildQualifiedName(type.NamespaceName, MetadataWriter.GetMangledName(type)));
		}

		private static void VisitTopLevelType(TypeReferenceIndexer noPiaIndexer, INamespaceTypeDefinition type)
		{
			noPiaIndexer?.Visit((ITypeDefinition)type);
		}

		public IFieldReference GetModuleVersionId(ITypeReference mvidType, TGreenNode syntaxOpt, DiagnosticBag diagnostics)
		{
			PrivateImplementationDetails details = GetPrivateImplClass(syntaxOpt, diagnostics);
			EnsurePrivateImplementationDetailsStaticConstructor(details, syntaxOpt, diagnostics);

			return details.GetModuleVersionId(mvidType);
		}

		public IFieldReference GetInstrumentationPayloadRoot(int analysisKind, ITypeReference payloadType, TGreenNode syntaxOpt, DiagnosticBag diagnostics)
		{
			PrivateImplementationDetails details = GetPrivateImplClass(syntaxOpt, diagnostics);
			EnsurePrivateImplementationDetailsStaticConstructor(details, syntaxOpt, diagnostics);

			return details.GetOrAddInstrumentationPayloadRoot(analysisKind, payloadType);
		}

		private void EnsurePrivateImplementationDetailsStaticConstructor(PrivateImplementationDetails details, TGreenNode syntaxOpt, DiagnosticBag diagnostics)
		{
			if (details.GetMethod(WellKnownMemberNames.StaticConstructorName) == null)
			{
				details.TryAddSynthesizedMethod(CreatePrivateImplementationDetailsStaticConstructor(details, syntaxOpt, diagnostics));
			}
		}

		protected abstract IMethodDefinition CreatePrivateImplementationDetailsStaticConstructor(PrivateImplementationDetails details, TGreenNode syntaxOpt, DiagnosticBag diagnostics);

		#region Synthesized Members

		/// <summary>
		/// Captures the set of synthesized definitions that should be added to a type
		/// during emit process.
		/// </summary>
		private sealed class SynthesizedDefinitions
		{
			public Queue<INestedTypeDefinition> NestedTypes;
			public Queue<IMethodDefinition> Methods;
			public Queue<IPropertyDefinition> Properties;
			public Queue<IFieldDefinition> Fields;

			public ITypeDefinitionMember[] GetAllMembers()
			{
				var builder = new List<ITypeDefinitionMember>();

				if (Fields != null)
				{
					foreach (var field in Fields)
					{
						builder.Add(field);
					}
				}

				if (Methods != null)
				{
					foreach (var method in Methods)
					{
						builder.Add(method);
					}
				}

				if (Properties != null)
				{
					foreach (var property in Properties)
					{
						builder.Add(property);
					}
				}

				if (NestedTypes != null)
				{
					foreach (var type in NestedTypes)
					{
						builder.Add(type);
					}
				}

				return builder.ToArray();
			}
		}

		private readonly Dictionary<TNamedTypeSymbol, SynthesizedDefinitions> _synthesizedDefs =
			new Dictionary<TNamedTypeSymbol, SynthesizedDefinitions>();

		public void AddSynthesizedDefinition(TNamedTypeSymbol container, INestedTypeDefinition nestedType)
		{
			Debug.Assert(nestedType != null);

			SynthesizedDefinitions defs = GetCacheOfSynthesizedDefinitions(container);
			if (defs.NestedTypes == null)
			{
				Interlocked.CompareExchange(ref defs.NestedTypes, new Queue<INestedTypeDefinition>(), null);
			}

			defs.NestedTypes.Enqueue(nestedType);
		}

		public abstract IEnumerable<INestedTypeDefinition> GetSynthesizedNestedTypes(TNamedTypeSymbol container);

		/// <summary>
		/// Returns null if there are no compiler generated types.
		/// </summary>
		public IEnumerable<INestedTypeDefinition> GetSynthesizedTypes(TNamedTypeSymbol container)
		{
			IEnumerable<INestedTypeDefinition> declareTypes = GetSynthesizedNestedTypes(container);
			IEnumerable<INestedTypeDefinition> compileEmitTypes = null;

			SynthesizedDefinitions defs = GetCacheOfSynthesizedDefinitions(container, addIfNotFound: false);
			if (defs != null)
			{
				compileEmitTypes = defs.NestedTypes;
			}

			if (declareTypes == null)
			{
				return compileEmitTypes;
			}

			if (compileEmitTypes == null)
			{
				return declareTypes;
			}

			return declareTypes.Concat(compileEmitTypes);
		}

		private SynthesizedDefinitions GetCacheOfSynthesizedDefinitions(TNamedTypeSymbol container, bool addIfNotFound = true)
		{
			Debug.Assert(((INamedTypeSymbol)container).IsDefinition);
			if (addIfNotFound)
			{
				SynthesizedDefinitions result;
				if (!_synthesizedDefs.TryGetValue(container, out result))
				{
					result = new SynthesizedDefinitions();
					_synthesizedDefs.Add(container, result);
				}
				return result;
			}

			SynthesizedDefinitions defs;
			_synthesizedDefs.TryGetValue(container, out defs);
			return defs;
		}

		public void AddSynthesizedDefinition(TNamedTypeSymbol container, IMethodDefinition method)
		{
			Debug.Assert(method != null);

			SynthesizedDefinitions defs = GetCacheOfSynthesizedDefinitions(container);
			if (defs.Methods == null)
			{
				Interlocked.CompareExchange(ref defs.Methods, new Queue<IMethodDefinition>(), null);
			}

			defs.Methods.Enqueue(method);
		}

		/// <summary>
		/// Returns null if there are no synthesized methods.
		/// </summary>
		public IEnumerable<IMethodDefinition> GetSynthesizedMethods(TNamedTypeSymbol container)
		{
			return GetCacheOfSynthesizedDefinitions(container, addIfNotFound: false)?.Methods;
		}

		public void AddSynthesizedDefinition(TNamedTypeSymbol container, IPropertyDefinition property)
		{
			Debug.Assert(property != null);

			SynthesizedDefinitions defs = GetCacheOfSynthesizedDefinitions(container);
			if (defs.Properties == null)
			{
				Interlocked.CompareExchange(ref defs.Properties, new Queue<IPropertyDefinition>(), null);
			}

			defs.Properties.Enqueue(property);
		}

		/// <summary>
		/// Returns null if there are no synthesized properties.
		/// </summary>
		public IEnumerable<IPropertyDefinition> GetSynthesizedProperties(TNamedTypeSymbol container)
		{
			return GetCacheOfSynthesizedDefinitions(container, addIfNotFound: false)?.Properties;
		}

		public void AddSynthesizedDefinition(TNamedTypeSymbol container, IFieldDefinition field)
		{
			Debug.Assert(field != null);

			SynthesizedDefinitions defs = GetCacheOfSynthesizedDefinitions(container);
			if (defs.Fields == null)
			{
				Interlocked.CompareExchange(ref defs.Fields, new Queue<IFieldDefinition>(), null);
			}

			defs.Fields.Enqueue(field);
		}

		/// <summary>
		/// Returns null if there are no synthesized fields.
		/// </summary>
		public IEnumerable<IFieldDefinition> GetSynthesizedFields(TNamedTypeSymbol container)
		{
			return GetCacheOfSynthesizedDefinitions(container, addIfNotFound: false)?.Fields;
		}

		public override Dictionary<ITypeDefinition, ITypeDefinitionMember[]> GetSynthesizedMembers()
		{
			var builder = new Dictionary<ITypeDefinition, ITypeDefinitionMember[]>();

			foreach (var entry in _synthesizedDefs)
			{
				builder.Add(entry.Key, entry.Value.GetAllMembers());
			}

			return builder;
		}

		public ITypeDefinitionMember[] GetSynthesizedMembers(ITypeDefinition container)
		{
			SynthesizedDefinitions defs = GetCacheOfSynthesizedDefinitions((TNamedTypeSymbol)container, addIfNotFound: false);
			if (defs == null)
			{
				return new ITypeDefinitionMember[0];
			}

			return defs.GetAllMembers();
		}

		#endregion

		#region Token Mapping

		IFieldReference ITokenDeferral.GetFieldForData(byte[] data, GreenNode syntaxNode, DiagnosticBag diagnostics)
		{
			Debug.Assert(this.SupportsPrivateImplClass);

			var privateImpl = this.GetPrivateImplClass((TGreenNode)syntaxNode, diagnostics);

			// map a field to the block (that makes it addressable via a token)
			return privateImpl.CreateDataField(data);
		}

		public abstract IMethodReference GetInitArrayHelper();

		public ArrayMethods ArrayMethods
		{
			get
			{
				ArrayMethods result = _lazyArrayMethods;

				if (result == null)
				{
					result = new ArrayMethods();

					if (Interlocked.CompareExchange(ref _lazyArrayMethods, result, null) != null)
					{
						result = _lazyArrayMethods;
					}
				}

				return result;
			}
		}

		#endregion

		#region Private Implementation Details Type

		public PrivateImplementationDetails GetPrivateImplClass(TGreenNode syntaxNodeOpt, DiagnosticBag diagnostics)
		{
			var result = _privateImplementationDetails;

			if ((result == null) && this.SupportsPrivateImplClass)
			{
				result = new PrivateImplementationDetails(
						this,
						this.SourceModule.Name,
						/*Compilation.GetSubmissionSlotIndex()*/-1,
						this.GetSpecialType(SpecialType.System_Object, syntaxNodeOpt, diagnostics),
						this.GetSpecialType(SpecialType.System_ValueType, syntaxNodeOpt, diagnostics),
						this.GetSpecialType(SpecialType.System_Byte, syntaxNodeOpt, diagnostics),
						this.GetSpecialType(SpecialType.System_Int16, syntaxNodeOpt, diagnostics),
						this.GetSpecialType(SpecialType.System_Int32, syntaxNodeOpt, diagnostics),
						this.GetSpecialType(SpecialType.System_Int64, syntaxNodeOpt, diagnostics),
						SynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));

				if (Interlocked.CompareExchange(ref _privateImplementationDetails, result, null) != null)
				{
					result = _privateImplementationDetails;
				}
			}

			return result;
		}

		public PrivateImplementationDetails PrivateImplClass
		{
			get { return _privateImplementationDetails; }
		}

		public override bool SupportsPrivateImplClass
		{
			get { return true; }
		}

		#endregion

		public sealed override ITypeReference GetPlatformType(PlatformType platformType, EmitContext context)
		{
			Debug.Assert((object)this == context.Module);

			switch (platformType)
			{
				case PlatformType.SystemType:
					return GetSystemType((TGreenNode)context.SyntaxNodeOpt, context.Diagnostics);

				default:
					return GetSpecialType((SpecialType)platformType, (TGreenNode)context.SyntaxNodeOpt, context.Diagnostics);
			}
		}
	}
}
