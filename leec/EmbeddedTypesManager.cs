using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace leec
{
	public abstract class CommonEmbeddedTypesManager
	{
		public abstract bool IsFrozen { get; }
		public abstract INamespaceTypeDefinition[] GetTypes(DiagnosticBag diagnostics, HashSet<string> namesOfTopLevelTypes);
	}

	public abstract class EmbeddedTypesManager<
		TPEModuleBuilder,
		TModuleCompilationState,
		TEmbeddedTypesManager,
		TSyntaxNode,
		TAttributeData,
		TSymbol,
		TAssemblySymbol,
		TNamedTypeSymbol,
		TFieldSymbol,
		TMethodSymbol,
		TEventSymbol,
		TPropertySymbol,
		TParameterSymbol,
		TTypeParameterSymbol,
		TEmbeddedType,
		TEmbeddedField,
		TEmbeddedMethod,
		TEmbeddedEvent,
		TEmbeddedProperty,
		TEmbeddedParameter,
		TEmbeddedTypeParameter> : CommonEmbeddedTypesManager
		where TPEModuleBuilder : CommonPEModuleBuilder
		where TModuleCompilationState : CommonModuleCompilationState
		where TEmbeddedTypesManager : EmbeddedTypesManager<TPEModuleBuilder, TModuleCompilationState, TEmbeddedTypesManager, TSyntaxNode, TAttributeData, TSymbol, TAssemblySymbol, TNamedTypeSymbol, TFieldSymbol, TMethodSymbol, TEventSymbol, TPropertySymbol, TParameterSymbol, TTypeParameterSymbol, TEmbeddedType, TEmbeddedField, TEmbeddedMethod, TEmbeddedEvent, TEmbeddedProperty, TEmbeddedParameter, TEmbeddedTypeParameter>
		where TSyntaxNode : GreenNode
		where TAttributeData : AttributeData, ICustomAttribute
		where TAssemblySymbol : class, TSymbol
		where TNamedTypeSymbol : class, TSymbol, INamespaceTypeReference
		where TFieldSymbol : class, TSymbol, IFieldReference
		where TMethodSymbol : class, TSymbol, IMethodReference
		where TEventSymbol : class, TSymbol, ITypeMemberReference
		where TPropertySymbol : class, TSymbol, ITypeMemberReference
		where TParameterSymbol : class, TSymbol, IParameterListEntry, INamedEntity
		where TTypeParameterSymbol : class, TSymbol, IGenericMethodParameterReference
		where TEmbeddedType : EmbeddedTypesManager<TPEModuleBuilder, TModuleCompilationState, TEmbeddedTypesManager, TSyntaxNode, TAttributeData, TSymbol, TAssemblySymbol, TNamedTypeSymbol, TFieldSymbol, TMethodSymbol, TEventSymbol, TPropertySymbol, TParameterSymbol, TTypeParameterSymbol, TEmbeddedType, TEmbeddedField, TEmbeddedMethod, TEmbeddedEvent, TEmbeddedProperty, TEmbeddedParameter, TEmbeddedTypeParameter>.CommonEmbeddedType
		where TEmbeddedField : EmbeddedTypesManager<TPEModuleBuilder, TModuleCompilationState, TEmbeddedTypesManager, TSyntaxNode, TAttributeData, TSymbol, TAssemblySymbol, TNamedTypeSymbol, TFieldSymbol, TMethodSymbol, TEventSymbol, TPropertySymbol, TParameterSymbol, TTypeParameterSymbol, TEmbeddedType, TEmbeddedField, TEmbeddedMethod, TEmbeddedEvent, TEmbeddedProperty, TEmbeddedParameter, TEmbeddedTypeParameter>.CommonEmbeddedField
		where TEmbeddedMethod : EmbeddedTypesManager<TPEModuleBuilder, TModuleCompilationState, TEmbeddedTypesManager, TSyntaxNode, TAttributeData, TSymbol, TAssemblySymbol, TNamedTypeSymbol, TFieldSymbol, TMethodSymbol, TEventSymbol, TPropertySymbol, TParameterSymbol, TTypeParameterSymbol, TEmbeddedType, TEmbeddedField, TEmbeddedMethod, TEmbeddedEvent, TEmbeddedProperty, TEmbeddedParameter, TEmbeddedTypeParameter>.CommonEmbeddedMethod
		where TEmbeddedEvent : EmbeddedTypesManager<TPEModuleBuilder, TModuleCompilationState, TEmbeddedTypesManager, TSyntaxNode, TAttributeData, TSymbol, TAssemblySymbol, TNamedTypeSymbol, TFieldSymbol, TMethodSymbol, TEventSymbol, TPropertySymbol, TParameterSymbol, TTypeParameterSymbol, TEmbeddedType, TEmbeddedField, TEmbeddedMethod, TEmbeddedEvent, TEmbeddedProperty, TEmbeddedParameter, TEmbeddedTypeParameter>.CommonEmbeddedEvent
		where TEmbeddedProperty : EmbeddedTypesManager<TPEModuleBuilder, TModuleCompilationState, TEmbeddedTypesManager, TSyntaxNode, TAttributeData, TSymbol, TAssemblySymbol, TNamedTypeSymbol, TFieldSymbol, TMethodSymbol, TEventSymbol, TPropertySymbol, TParameterSymbol, TTypeParameterSymbol, TEmbeddedType, TEmbeddedField, TEmbeddedMethod, TEmbeddedEvent, TEmbeddedProperty, TEmbeddedParameter, TEmbeddedTypeParameter>.CommonEmbeddedProperty
		where TEmbeddedParameter : EmbeddedTypesManager<TPEModuleBuilder, TModuleCompilationState, TEmbeddedTypesManager, TSyntaxNode, TAttributeData, TSymbol, TAssemblySymbol, TNamedTypeSymbol, TFieldSymbol, TMethodSymbol, TEventSymbol, TPropertySymbol, TParameterSymbol, TTypeParameterSymbol, TEmbeddedType, TEmbeddedField, TEmbeddedMethod, TEmbeddedEvent, TEmbeddedProperty, TEmbeddedParameter, TEmbeddedTypeParameter>.CommonEmbeddedParameter
		where TEmbeddedTypeParameter : EmbeddedTypesManager<TPEModuleBuilder, TModuleCompilationState, TEmbeddedTypesManager, TSyntaxNode, TAttributeData, TSymbol, TAssemblySymbol, TNamedTypeSymbol, TFieldSymbol, TMethodSymbol, TEventSymbol, TPropertySymbol, TParameterSymbol, TTypeParameterSymbol, TEmbeddedType, TEmbeddedField, TEmbeddedMethod, TEmbeddedEvent, TEmbeddedProperty, TEmbeddedParameter, TEmbeddedTypeParameter>.CommonEmbeddedTypeParameter
	{
		public readonly TPEModuleBuilder ModuleBeingBuilt;

		public readonly Dictionary<TNamedTypeSymbol, TEmbeddedType> EmbeddedTypesMap = new Dictionary<TNamedTypeSymbol, TEmbeddedType>();
		public readonly Dictionary<TFieldSymbol, TEmbeddedField> EmbeddedFieldsMap = new Dictionary<TFieldSymbol, TEmbeddedField>();
		public readonly Dictionary<TMethodSymbol, TEmbeddedMethod> EmbeddedMethodsMap = new Dictionary<TMethodSymbol, TEmbeddedMethod>();
		public readonly Dictionary<TPropertySymbol, TEmbeddedProperty> EmbeddedPropertiesMap = new Dictionary<TPropertySymbol, TEmbeddedProperty>();
		public readonly Dictionary<TEventSymbol, TEmbeddedEvent> EmbeddedEventsMap = new Dictionary<TEventSymbol, TEmbeddedEvent>();

		private TEmbeddedType[] _frozen;

		protected EmbeddedTypesManager(TPEModuleBuilder moduleBeingBuilt)
		{
			this.ModuleBeingBuilt = moduleBeingBuilt;
		}

		public override bool IsFrozen
		{
			get
			{
				return _frozen != null;
			}
		}

		public override INamespaceTypeDefinition[] GetTypes(DiagnosticBag diagnostics, HashSet<string> namesOfTopLevelTypes)
		{
			if (_frozen == null)
			{
				var builder = new List<TEmbeddedType>();
				builder.AddRange(EmbeddedTypesMap.Values);
				builder.Sort(TypeComparer.Instance);

				_frozen = builder.ToArray();
				if (_frozen.Length > 0)
				{
					INamespaceTypeDefinition prev = _frozen[0];
					bool reportedDuplicate = HasNameConflict(namesOfTopLevelTypes, _frozen[0], diagnostics);

					for (int i = 1; i < _frozen.Length; i++)
					{
						INamespaceTypeDefinition current = _frozen[i];

						if (prev.NamespaceName == current.NamespaceName &&
							prev.Name == current.Name)
						{
							if (!reportedDuplicate)
							{
								Debug.Assert(_frozen[i - 1] == prev);

								// ERR_DuplicateLocalTypes3/ERR_InteropTypesWithSameNameAndGuid
								ReportNameCollisionBetweenEmbeddedTypes(_frozen[i - 1], _frozen[i], diagnostics);
								reportedDuplicate = true;
							}
						}
						else
						{
							prev = current;
							reportedDuplicate = HasNameConflict(namesOfTopLevelTypes, _frozen[i], diagnostics);
						}
					}

					OnGetTypesCompleted(_frozen, diagnostics);
				}
			}

			return StaticCast<INamespaceTypeDefinition>.From(_frozen);
		}

		private bool HasNameConflict(HashSet<string> namesOfTopLevelTypes, TEmbeddedType type, DiagnosticBag diagnostics)
		{
			INamespaceTypeDefinition def = type;

			if (namesOfTopLevelTypes.Contains(MetadataHelpers.BuildQualifiedName(def.NamespaceName, def.Name)))
			{
				// ERR_LocalTypeNameClash2/ERR_LocalTypeNameClash
				ReportNameCollisionWithAlreadyDeclaredType(type, diagnostics);
				return true;
			}

			return false;
		}

		public abstract int GetTargetAttributeSignatureIndex(TSymbol underlyingSymbol, TAttributeData attrData, AttributeDescription description);

		public bool IsTargetAttribute(TSymbol underlyingSymbol, TAttributeData attrData, AttributeDescription description)
		{
			return GetTargetAttributeSignatureIndex(underlyingSymbol, attrData, description) != -1;
		}

		public abstract TAttributeData CreateSynthesizedAttribute(WellKnownMember constructor, TAttributeData attrData, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);
		public abstract void ReportIndirectReferencesToLinkedAssemblies(TAssemblySymbol assembly, DiagnosticBag diagnostics);

		protected abstract void OnGetTypesCompleted(TEmbeddedType[] types, DiagnosticBag diagnostics);
		protected abstract void ReportNameCollisionBetweenEmbeddedTypes(TEmbeddedType typeA, TEmbeddedType typeB, DiagnosticBag diagnostics);
		protected abstract void ReportNameCollisionWithAlreadyDeclaredType(TEmbeddedType type, DiagnosticBag diagnostics);
		protected abstract TAttributeData CreateCompilerGeneratedAttribute();

		private sealed class TypeComparer : IComparer<TEmbeddedType>
		{
			public static readonly TypeComparer Instance = new TypeComparer();

			private TypeComparer()
			{
			}

			public int Compare(TEmbeddedType x, TEmbeddedType y)
			{
				INamespaceTypeDefinition dx = x;
				INamespaceTypeDefinition dy = y;

				int result = string.Compare(dx.NamespaceName, dy.NamespaceName, StringComparison.Ordinal);

				if (result == 0)
				{
					result = string.Compare(dx.Name, dy.Name, StringComparison.Ordinal);

					if (result == 0)
					{
						// this is a name conflict.
						result = x.AssemblyRefIndex - y.AssemblyRefIndex;
					}
				}

				return result;
			}
		}

		protected void EmbedReferences(ITypeDefinitionMember embeddedMember, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
		{
			var noPiaIndexer = new TypeReferenceIndexer(new EmitContext(ModuleBeingBuilt, syntaxNodeOpt, diagnostics, metadataOnly: false, includePrivateMembers: true));
			noPiaIndexer.Visit(embeddedMember);
		}

		/// <summary>
		/// Returns null if member doesn't belong to an embedded NoPia type.
		/// </summary>
		protected abstract TEmbeddedType GetEmbeddedTypeForMember(TSymbol member, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);

		public abstract TEmbeddedField EmbedField(TEmbeddedType type, TFieldSymbol field, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);
		public abstract TEmbeddedMethod EmbedMethod(TEmbeddedType type, TMethodSymbol method, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);
		public abstract TEmbeddedProperty EmbedProperty(TEmbeddedType type, TPropertySymbol property, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);
		public abstract TEmbeddedEvent EmbedEvent(TEmbeddedType type, TEventSymbol @event, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, bool isUsedForComAwareEventBinding);

		public IFieldReference EmbedFieldIfNeedTo(TFieldSymbol fieldSymbol, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
		{
			TEmbeddedType type = GetEmbeddedTypeForMember(fieldSymbol, syntaxNodeOpt, diagnostics);
			if (type != null)
			{
				return EmbedField(type, fieldSymbol, syntaxNodeOpt, diagnostics);
			}
			return fieldSymbol;
		}

		public IMethodReference EmbedMethodIfNeedTo(TMethodSymbol methodSymbol, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
		{
			TEmbeddedType type = GetEmbeddedTypeForMember(methodSymbol, syntaxNodeOpt, diagnostics);
			if (type != null)
			{
				return EmbedMethod(type, methodSymbol, syntaxNodeOpt, diagnostics);
			}
			return methodSymbol;
		}

		public void EmbedEventIfNeedTo(TEventSymbol eventSymbol, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, bool isUsedForComAwareEventBinding)
		{
			TEmbeddedType type = GetEmbeddedTypeForMember(eventSymbol, syntaxNodeOpt, diagnostics);
			if (type != null)
			{
				EmbedEvent(type, eventSymbol, syntaxNodeOpt, diagnostics, isUsedForComAwareEventBinding);
			}
		}

		public void EmbedPropertyIfNeedTo(TPropertySymbol propertySymbol, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
		{
			TEmbeddedType type = GetEmbeddedTypeForMember(propertySymbol, syntaxNodeOpt, diagnostics);
			if (type != null)
			{
				EmbedProperty(type, propertySymbol, syntaxNodeOpt, diagnostics);
			}
		}

		public abstract class CommonEmbeddedEvent : CommonEmbeddedMember<TEventSymbol>, IEventDefinition
		{
			private readonly TEmbeddedMethod _adder;
			private readonly TEmbeddedMethod _remover;
			private readonly TEmbeddedMethod _caller;

			private int _isUsedForComAwareEventBinding;

			protected CommonEmbeddedEvent(TEventSymbol underlyingEvent, TEmbeddedMethod adder, TEmbeddedMethod remover, TEmbeddedMethod caller) :
				base(underlyingEvent)
			{
				Debug.Assert(adder != null || remover != null);

				_adder = adder;
				_remover = remover;
				_caller = caller;
			}

			public override TEmbeddedTypesManager TypeManager
			{
				get
				{
					return AnAccessor.TypeManager;
				}
			}

			protected abstract bool IsRuntimeSpecial { get; }
			protected abstract bool IsSpecialName { get; }
			protected abstract ITypeReference GetType(TPEModuleBuilder moduleBuilder, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);
			protected abstract TEmbeddedType ContainingType { get; }
			protected abstract TypeMemberVisibility Visibility { get; }
			protected abstract string Name { get; }

			public TEventSymbol UnderlyingEvent
			{
				get
				{
					return this.UnderlyingSymbol;
				}
			}

			protected abstract void EmbedCorrespondingComEventInterfaceMethodInternal(TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, bool isUsedForComAwareEventBinding);

			public void EmbedCorrespondingComEventInterfaceMethod(TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, bool isUsedForComAwareEventBinding)
			{
				if (_isUsedForComAwareEventBinding == 0 &&
					(!isUsedForComAwareEventBinding ||
					 Interlocked.CompareExchange(ref _isUsedForComAwareEventBinding, 1, 0) == 0))
				{
					Debug.Assert(!isUsedForComAwareEventBinding || _isUsedForComAwareEventBinding != 0);

					EmbedCorrespondingComEventInterfaceMethodInternal(syntaxNodeOpt, diagnostics, isUsedForComAwareEventBinding);
				}

				Debug.Assert(!isUsedForComAwareEventBinding || _isUsedForComAwareEventBinding != 0);
			}

			IMethodReference IEventDefinition.Adder
			{
				get { return _adder; }
			}

			IMethodReference IEventDefinition.Remover
			{
				get { return _remover; }
			}

			IMethodReference IEventDefinition.Caller
			{
				get { return _caller; }
			}

			IEnumerable<IMethodReference> IEventDefinition.GetAccessors(EmitContext context)
			{
				if (_adder != null)
				{
					yield return _adder;
				}

				if (_remover != null)
				{
					yield return _remover;
				}

				if (_caller != null)
				{
					yield return _caller;
				}
			}

			bool IEventDefinition.IsRuntimeSpecial
			{
				get
				{
					return IsRuntimeSpecial;
				}
			}

			bool IEventDefinition.IsSpecialName
			{
				get
				{
					return IsSpecialName;
				}
			}

			ITypeReference IEventDefinition.GetType(EmitContext context)
			{
				return GetType((TPEModuleBuilder)context.Module, (TSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
			}

			protected TEmbeddedMethod AnAccessor
			{
				get
				{
					return _adder ?? _remover;
				}
			}

			ITypeDefinition ITypeDefinitionMember.ContainingTypeDefinition
			{
				get { return ContainingType; }
			}

			TypeMemberVisibility ITypeDefinitionMember.Visibility
			{
				get
				{
					return Visibility;
				}
			}

			ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
			{
				return ContainingType;
			}

			void IReference.Dispatch(MetadataVisitor visitor)
			{
				visitor.Visit((IEventDefinition)this);
			}

			IDefinition IReference.AsDefinition(EmitContext context)
			{
				return this;
			}

			string INamedEntity.Name
			{
				get
				{
					return Name;
				}
			}
		}
		public abstract class CommonEmbeddedField : CommonEmbeddedMember<TFieldSymbol>, IFieldDefinition
		{
			public readonly TEmbeddedType ContainingType;

			protected CommonEmbeddedField(TEmbeddedType containingType, TFieldSymbol underlyingField) :
				base(underlyingField)
			{
				this.ContainingType = containingType;
			}

			public TFieldSymbol UnderlyingField
			{
				get
				{
					return UnderlyingSymbol;
				}
			}

			protected abstract MetadataConstant GetCompileTimeValue(EmitContext context);
			protected abstract bool IsCompileTimeConstant { get; }
			protected abstract bool IsNotSerialized { get; }
			protected abstract bool IsReadOnly { get; }
			protected abstract bool IsRuntimeSpecial { get; }
			protected abstract bool IsSpecialName { get; }
			protected abstract bool IsStatic { get; }
			protected abstract bool IsMarshalledExplicitly { get; }
			protected abstract IMarshallingInformation MarshallingInformation { get; }
			protected abstract byte[] MarshallingDescriptor { get; }
			protected abstract int? TypeLayoutOffset { get; }
			protected abstract TypeMemberVisibility Visibility { get; }
			protected abstract string Name { get; }

			MetadataConstant IFieldDefinition.GetCompileTimeValue(EmitContext context)
			{
				return GetCompileTimeValue(context);
			}

			byte[] IFieldDefinition.MappedData
			{
				get
				{
					return null;
				}
			}

			bool IFieldDefinition.IsCompileTimeConstant
			{
				get
				{
					return IsCompileTimeConstant;
				}
			}

			bool IFieldDefinition.IsNotSerialized
			{
				get
				{
					return IsNotSerialized;
				}
			}

			bool IFieldDefinition.IsReadOnly
			{
				get
				{
					return IsReadOnly;
				}
			}

			bool IFieldDefinition.IsRuntimeSpecial
			{
				get
				{
					return IsRuntimeSpecial;
				}
			}

			bool IFieldDefinition.IsSpecialName
			{
				get
				{
					return IsSpecialName;
				}
			}

			bool IFieldDefinition.IsStatic
			{
				get
				{
					return IsStatic;
				}
			}

			bool IFieldDefinition.IsMarshalledExplicitly
			{
				get
				{
					return IsMarshalledExplicitly;
				}
			}

			IMarshallingInformation IFieldDefinition.MarshallingInformation
			{
				get
				{
					return MarshallingInformation;
				}
			}

			byte[] IFieldDefinition.MarshallingDescriptor
			{
				get
				{
					return MarshallingDescriptor;
				}
			}

			int IFieldDefinition.Offset
			{
				get
				{
					return TypeLayoutOffset ?? 0;
				}
			}

			ITypeDefinition ITypeDefinitionMember.ContainingTypeDefinition
			{
				get
				{
					return ContainingType;
				}
			}

			TypeMemberVisibility ITypeDefinitionMember.Visibility
			{
				get
				{
					return Visibility;
				}
			}

			ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
			{
				return ContainingType;
			}

			void IReference.Dispatch(MetadataVisitor visitor)
			{
				visitor.Visit((IFieldDefinition)this);
			}

			IDefinition IReference.AsDefinition(EmitContext context)
			{
				return this;
			}

			string INamedEntity.Name
			{
				get
				{
					return Name;
				}
			}

			ITypeReference IFieldReference.GetType(EmitContext context)
			{
				return UnderlyingField.GetType(context);
			}

			IFieldDefinition IFieldReference.GetResolvedField(EmitContext context)
			{
				return this;
			}

			ISpecializedFieldReference IFieldReference.AsSpecializedFieldReference
			{
				get
				{
					return null;
				}
			}

			bool IFieldReference.IsContextualNamedEntity
			{
				get
				{
					return false;
				}
			}
		}
		public abstract class CommonEmbeddedMember
		{
			public abstract TEmbeddedTypesManager TypeManager { get; }
		}

		public abstract class CommonEmbeddedMember<TMember> : CommonEmbeddedMember, IReference
			where TMember : TSymbol, ITypeMemberReference
		{
			protected readonly TMember UnderlyingSymbol;
			private TAttributeData[] _lazyAttributes;

			protected CommonEmbeddedMember(TMember underlyingSymbol)
			{
				this.UnderlyingSymbol = underlyingSymbol;
			}

			protected abstract IEnumerable<TAttributeData> GetCustomAttributesToEmit(TPEModuleBuilder moduleBuilder);

			protected virtual TAttributeData PortAttributeIfNeedTo(TAttributeData attrData, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
			{
				return null;
			}

			private TAttributeData[] GetAttributes(TPEModuleBuilder moduleBuilder, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
			{
				var builder = new List<TAttributeData>();

				// Copy some of the attributes.

				// Note, when porting attributes, we are not using constructors from original symbol.
				// The constructors might be missing (for example, in metadata case) and doing lookup
				// will ensure that we report appropriate errors.

				foreach (var attrData in GetCustomAttributesToEmit(moduleBuilder))
				{
					if (TypeManager.IsTargetAttribute(UnderlyingSymbol, attrData, AttributeDescription.DispIdAttribute))
					{
						if (attrData.CommonConstructorArguments.Length == 1)
						{
							builder.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Runtime_InteropServices_DispIdAttribute__ctor, attrData, syntaxNodeOpt, diagnostics));
						}
					}
					else
					{
						builder.AddOptional(PortAttributeIfNeedTo(attrData, syntaxNodeOpt, diagnostics));
					}
				}

				return builder.ToArray();
			}

			IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
			{
				if (_lazyAttributes == null)
				{
					var diagnostics = DiagnosticBag.GetInstance();
					var attributes = GetAttributes((TPEModuleBuilder)context.Module, (TSyntaxNode)context.SyntaxNodeOpt, diagnostics);

					_lazyAttributes = attributes;
					// Save any diagnostics that we encountered.
					context.Diagnostics.AddRange(diagnostics);

					diagnostics.Free();
				}

				return _lazyAttributes;
			}

			void IReference.Dispatch(MetadataVisitor visitor)
			{
				throw ExceptionUtilities.Unreachable;
			}

			IDefinition IReference.AsDefinition(EmitContext context)
			{
				throw ExceptionUtilities.Unreachable;
			}
		}
		public abstract class CommonEmbeddedMethod : CommonEmbeddedMember<TMethodSymbol>, IMethodDefinition
		{
			public readonly TEmbeddedType ContainingType;
			private readonly TEmbeddedTypeParameter[] _typeParameters;
			private readonly TEmbeddedParameter[] _parameters;

			protected CommonEmbeddedMethod(TEmbeddedType containingType, TMethodSymbol underlyingMethod) :
				base(underlyingMethod)
			{
				this.ContainingType = containingType;
				_typeParameters = GetTypeParameters();
				_parameters = GetParameters();
			}

			protected abstract TEmbeddedTypeParameter[] GetTypeParameters();
			protected abstract TEmbeddedParameter[] GetParameters();
			protected abstract bool IsAbstract { get; }
			protected abstract bool IsAccessCheckedOnOverride { get; }
			protected abstract bool IsConstructor { get; }
			protected abstract bool IsExternal { get; }
			protected abstract bool IsHiddenBySignature { get; }
			protected abstract bool IsNewSlot { get; }
			protected abstract IPlatformInvokeInformation PlatformInvokeData { get; }
			protected abstract bool IsRuntimeSpecial { get; }
			protected abstract bool IsSpecialName { get; }
			protected abstract bool IsSealed { get; }
			protected abstract bool IsStatic { get; }
			protected abstract bool IsVirtual { get; }
			protected abstract MethodImplAttributes GetImplementationAttributes(EmitContext context);
			protected abstract bool ReturnValueIsMarshalledExplicitly { get; }
			protected abstract IMarshallingInformation ReturnValueMarshallingInformation { get; }
			protected abstract byte[] ReturnValueMarshallingDescriptor { get; }
			protected abstract TypeMemberVisibility Visibility { get; }
			protected abstract string Name { get; }
			protected abstract bool AcceptsExtraArguments { get; }
			protected abstract ISignature UnderlyingMethodSignature { get; }
			protected abstract INamespace ContainingNamespace { get; }

			public TMethodSymbol UnderlyingMethod => this.UnderlyingSymbol;

			protected sealed override TAttributeData PortAttributeIfNeedTo(TAttributeData attrData, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
			{
				// Note, when porting attributes, we are not using constructors from original symbol.
				// The constructors might be missing (for example, in metadata case) and doing lookup
				// will ensure that we report appropriate errors.

				if (TypeManager.IsTargetAttribute(UnderlyingMethod, attrData, AttributeDescription.LCIDConversionAttribute))
				{
					if (attrData.CommonConstructorArguments.Length == 1)
					{
						return TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Runtime_InteropServices_LCIDConversionAttribute__ctor, attrData, syntaxNodeOpt, diagnostics);
					}
				}

				return null;
			}

			IMethodBody IMethodDefinition.GetBody(EmitContext context)
			{
				if (Extensions.HasBody(this))
				{
					// This is an error condition, which we already reported.
					// To prevent metadata emitter/visitor from crashing, let's
					// return an empty body.
					return new EmptyBody(this);
				}

				return null;
			}

			private sealed class EmptyBody : IMethodBody
			{
				private readonly CommonEmbeddedMethod _method;

				public EmptyBody(CommonEmbeddedMethod method)
				{
					_method = method;
				}

				public LocalScope[] LocalScopes => throw new NotImplementedException();

				ExceptionHandlerRegion[] IMethodBody.ExceptionRegions => new ExceptionHandlerRegion[0];

				bool IMethodBody.LocalsAreZeroed => false;

				ILocalDefinition[] IMethodBody.LocalVariables => new ILocalDefinition[0];

				IMethodDefinition IMethodBody.MethodDefinition => _method;

				ushort IMethodBody.MaxStack => 0;

				byte[] IMethodBody.IL => new byte[0];

				bool IMethodBody.HasDynamicLocalVariables => false;

				IImportScope IMethodBody.ImportScope => null;
			}

			IEnumerable<IGenericMethodParameter> IMethodDefinition.GenericParameters => _typeParameters;

			bool IMethodDefinition.IsImplicitlyDeclared => true;

			bool IMethodDefinition.HasDeclarativeSecurity => false;

			bool IMethodDefinition.IsAbstract => IsAbstract;

			bool IMethodDefinition.IsAccessCheckedOnOverride => IsAccessCheckedOnOverride;

			bool IMethodDefinition.IsConstructor => IsConstructor;

			bool IMethodDefinition.IsExternal => IsExternal;

			bool IMethodDefinition.IsHiddenBySignature => IsHiddenBySignature;

			bool IMethodDefinition.IsNewSlot => IsNewSlot;

			bool IMethodDefinition.IsPlatformInvoke => PlatformInvokeData != null;

			IPlatformInvokeInformation IMethodDefinition.PlatformInvokeData => PlatformInvokeData;

			bool IMethodDefinition.IsRuntimeSpecial => IsRuntimeSpecial;

			bool IMethodDefinition.IsSpecialName => IsSpecialName;

			bool IMethodDefinition.IsSealed => IsSealed;

			bool IMethodDefinition.IsStatic => IsStatic;

			bool IMethodDefinition.IsVirtual => IsVirtual;

			MethodImplAttributes IMethodDefinition.GetImplementationAttributes(EmitContext context)
			{
				return GetImplementationAttributes(context);
			}

			IParameterDefinition[] IMethodDefinition.Parameters
			{
				get
				{
					return StaticCast<IParameterDefinition>.From(_parameters);
				}
			}

			bool IMethodDefinition.RequiresSecurityObject => false;

			ICustomAttribute[] IMethodDefinition.GetReturnValueAttributes(EmitContext context)
			{
				return new ICustomAttribute[0];
			}

			bool IMethodDefinition.ReturnValueIsMarshalledExplicitly => ReturnValueIsMarshalledExplicitly;

			IMarshallingInformation IMethodDefinition.ReturnValueMarshallingInformation => ReturnValueMarshallingInformation;

			byte[] IMethodDefinition.ReturnValueMarshallingDescriptor => ReturnValueMarshallingDescriptor;

			IEnumerable<SecurityAttribute> IMethodDefinition.SecurityAttributes => new SecurityAttribute[0];

			ITypeDefinition ITypeDefinitionMember.ContainingTypeDefinition => ContainingType;

			INamespace IMethodDefinition.ContainingNamespace => ContainingNamespace;

			TypeMemberVisibility ITypeDefinitionMember.Visibility => Visibility;

			ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
			{
				return ContainingType;
			}

			void IReference.Dispatch(MetadataVisitor visitor)
			{
				visitor.Visit(this);
			}

			IDefinition IReference.AsDefinition(EmitContext context)
			{
				return this;
			}

			string INamedEntity.Name => Name;

			bool IMethodReference.AcceptsExtraArguments => AcceptsExtraArguments;

			ushort IMethodReference.GenericParameterCount => (ushort)_typeParameters.Length;

			bool IMethodReference.IsGeneric => _typeParameters.Length > 0;

			IMethodDefinition IMethodReference.GetResolvedMethod(EmitContext context)
			{
				return this;
			}

			IParameterTypeInformation[] IMethodReference.ExtraParameters
			{
				get
				{
					// This is a definition, no information about extra parameters 
					return new IParameterTypeInformation[0];
				}
			}

			IGenericMethodInstanceReference IMethodReference.AsGenericMethodInstanceReference => null;

			ISpecializedMethodReference IMethodReference.AsSpecializedMethodReference => null;

			CallingConvention ISignature.CallingConvention => UnderlyingMethodSignature.CallingConvention;

			ushort ISignature.ParameterCount => (ushort)_parameters.Length;

			IParameterTypeInformation[] ISignature.GetParameters(EmitContext context)
			{
				return StaticCast<IParameterTypeInformation>.From(_parameters);
			}

			ICustomModifier[] ISignature.RefCustomModifiers =>
				UnderlyingMethodSignature.RefCustomModifiers;

			ICustomModifier[] ISignature.ReturnValueCustomModifiers =>
				UnderlyingMethodSignature.ReturnValueCustomModifiers;

			bool ISignature.ReturnValueIsByRef => UnderlyingMethodSignature.ReturnValueIsByRef;

			ITypeReference ISignature.GetType(EmitContext context)
			{
				return UnderlyingMethodSignature.GetType(context);
			}

			/// <remarks>
			/// This is only used for testing.
			/// </remarks>
			public override string ToString()
			{
				return ((ISymbol)UnderlyingMethod).ToDisplayString();
			}
		}
		public abstract class CommonEmbeddedParameter : IParameterDefinition
		{
			public readonly CommonEmbeddedMember ContainingPropertyOrMethod;
			public readonly TParameterSymbol UnderlyingParameter;
			private TAttributeData[] _lazyAttributes;

			protected CommonEmbeddedParameter(CommonEmbeddedMember containingPropertyOrMethod, TParameterSymbol underlyingParameter)
			{
				this.ContainingPropertyOrMethod = containingPropertyOrMethod;
				this.UnderlyingParameter = underlyingParameter;
			}

			protected TEmbeddedTypesManager TypeManager
			{
				get
				{
					return ContainingPropertyOrMethod.TypeManager;
				}
			}

			protected abstract bool HasDefaultValue { get; }
			protected abstract MetadataConstant GetDefaultValue(EmitContext context);
			protected abstract bool IsIn { get; }
			protected abstract bool IsOut { get; }
			protected abstract bool IsOptional { get; }
			protected abstract bool IsMarshalledExplicitly { get; }
			protected abstract IMarshallingInformation MarshallingInformation { get; }
			protected abstract byte[] MarshallingDescriptor { get; }
			protected abstract string Name { get; }
			protected abstract IParameterTypeInformation UnderlyingParameterTypeInformation { get; }
			protected abstract ushort Index { get; }
			protected abstract IEnumerable<TAttributeData> GetCustomAttributesToEmit(TPEModuleBuilder moduleBuilder);

			private bool IsTargetAttribute(TAttributeData attrData, AttributeDescription description)
			{
				return TypeManager.IsTargetAttribute(UnderlyingParameter, attrData, description);
			}

			private TAttributeData[] GetAttributes(TPEModuleBuilder moduleBuilder, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
			{
				var builder = new List<TAttributeData>();

				// Copy some of the attributes.

				// Note, when porting attributes, we are not using constructors from original symbol.
				// The constructors might be missing (for example, in metadata case) and doing lookup
				// will ensure that we report appropriate errors.

				foreach (var attrData in GetCustomAttributesToEmit(moduleBuilder))
				{
					if (IsTargetAttribute(attrData, AttributeDescription.ParamArrayAttribute))
					{
						if (attrData.CommonConstructorArguments.Length == 0)
						{
							builder.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_ParamArrayAttribute__ctor, attrData, syntaxNodeOpt, diagnostics));
						}
					}
					else if (IsTargetAttribute(attrData, AttributeDescription.DateTimeConstantAttribute))
					{
						if (attrData.CommonConstructorArguments.Length == 1)
						{
							builder.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Runtime_CompilerServices_DateTimeConstantAttribute__ctor, attrData, syntaxNodeOpt, diagnostics));
						}
					}
					else
					{
						int signatureIndex = TypeManager.GetTargetAttributeSignatureIndex(UnderlyingParameter, attrData, AttributeDescription.DecimalConstantAttribute);
						if (signatureIndex != -1)
						{
							Debug.Assert(signatureIndex == 0 || signatureIndex == 1);

							if (attrData.CommonConstructorArguments.Length == 5)
							{
								builder.AddOptional(TypeManager.CreateSynthesizedAttribute(
									signatureIndex == 0 ? WellKnownMember.System_Runtime_CompilerServices_DecimalConstantAttribute__ctor :
										WellKnownMember.System_Runtime_CompilerServices_DecimalConstantAttribute__ctorByteByteInt32Int32Int32,
									attrData, syntaxNodeOpt, diagnostics));
							}
						}
						else if (IsTargetAttribute(attrData, AttributeDescription.DefaultParameterValueAttribute))
						{
							if (attrData.CommonConstructorArguments.Length == 1)
							{
								builder.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Runtime_InteropServices_DefaultParameterValueAttribute__ctor, attrData, syntaxNodeOpt, diagnostics));
							}
						}
					}
				}

				return builder.ToArray();
			}

			bool IParameterDefinition.HasDefaultValue
			{
				get
				{
					return HasDefaultValue;
				}
			}

			MetadataConstant IParameterDefinition.GetDefaultValue(EmitContext context)
			{
				return GetDefaultValue(context);
			}

			bool IParameterDefinition.IsIn
			{
				get
				{
					return IsIn;
				}
			}

			bool IParameterDefinition.IsOut
			{
				get
				{
					return IsOut;
				}
			}

			bool IParameterDefinition.IsOptional
			{
				get
				{
					return IsOptional;
				}
			}

			bool IParameterDefinition.IsMarshalledExplicitly
			{
				get
				{
					return IsMarshalledExplicitly;
				}
			}

			IMarshallingInformation IParameterDefinition.MarshallingInformation
			{
				get
				{
					return MarshallingInformation;
				}
			}

			byte[] IParameterDefinition.MarshallingDescriptor
			{
				get
				{
					return MarshallingDescriptor;
				}
			}

			IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
			{
				if (_lazyAttributes == null)
				{
					var diagnostics = DiagnosticBag.GetInstance();
					var attributes = GetAttributes((TPEModuleBuilder)context.Module, (TSyntaxNode)context.SyntaxNodeOpt, diagnostics);

					_lazyAttributes = attributes;
					// Save any diagnostics that we encountered.
					context.Diagnostics.AddRange(diagnostics);

					diagnostics.Free();
				}

				return _lazyAttributes;
			}

			void IReference.Dispatch(MetadataVisitor visitor)
			{
				throw ExceptionUtilities.Unreachable;
			}

			IDefinition IReference.AsDefinition(EmitContext context)
			{
				return this;
			}

			string INamedEntity.Name
			{
				get { return Name; }
			}

			ICustomModifier[] IParameterTypeInformation.CustomModifiers
			{
				get
				{
					return UnderlyingParameterTypeInformation.CustomModifiers;
				}
			}

			bool IParameterTypeInformation.IsByReference
			{
				get
				{
					return UnderlyingParameterTypeInformation.IsByReference;
				}
			}

			ICustomModifier[] IParameterTypeInformation.RefCustomModifiers
			{
				get
				{
					return UnderlyingParameterTypeInformation.RefCustomModifiers;
				}
			}

			ITypeReference IParameterTypeInformation.GetType(EmitContext context)
			{
				return UnderlyingParameterTypeInformation.GetType(context);
			}

			ushort IParameterListEntry.Index
			{
				get
				{
					return Index;
				}
			}

			/// <remarks>
			/// This is only used for testing.
			/// </remarks>
			public override string ToString()
			{
				return ((ISymbol)UnderlyingParameter).ToDisplayString();
			}
		}
		public abstract class CommonEmbeddedProperty : CommonEmbeddedMember<TPropertySymbol>, IPropertyDefinition
		{
			private readonly TEmbeddedParameter[] _parameters;
			private readonly TEmbeddedMethod _getter;
			private readonly TEmbeddedMethod _setter;

			protected CommonEmbeddedProperty(TPropertySymbol underlyingProperty, TEmbeddedMethod getter, TEmbeddedMethod setter) :
				base(underlyingProperty)
			{
				Debug.Assert(getter != null || setter != null);

				_getter = getter;
				_setter = setter;
				_parameters = GetParameters();
			}

			public override TEmbeddedTypesManager TypeManager
			{
				get
				{
					return AnAccessor.TypeManager;
				}
			}

			protected abstract TEmbeddedParameter[] GetParameters();
			protected abstract bool IsRuntimeSpecial { get; }
			protected abstract bool IsSpecialName { get; }
			protected abstract ISignature UnderlyingPropertySignature { get; }
			protected abstract TEmbeddedType ContainingType { get; }
			protected abstract TypeMemberVisibility Visibility { get; }
			protected abstract string Name { get; }

			public TPropertySymbol UnderlyingProperty
			{
				get
				{
					return this.UnderlyingSymbol;
				}
			}

			IMethodReference IPropertyDefinition.Getter
			{
				get { return _getter; }
			}

			IMethodReference IPropertyDefinition.Setter
			{
				get { return _setter; }
			}

			IEnumerable<IMethodReference> IPropertyDefinition.GetAccessors(EmitContext context)
			{
				if (_getter != null)
				{
					yield return _getter;
				}

				if (_setter != null)
				{
					yield return _setter;
				}
			}

			bool IPropertyDefinition.HasDefaultValue
			{
				get { return false; }
			}

			MetadataConstant IPropertyDefinition.DefaultValue
			{
				get { return null; }
			}

			bool IPropertyDefinition.IsRuntimeSpecial
			{
				get { return IsRuntimeSpecial; }
			}

			bool IPropertyDefinition.IsSpecialName
			{
				get
				{
					return IsSpecialName;
				}
			}

			IParameterDefinition[] IPropertyDefinition.Parameters
			{
				get { return StaticCast<IParameterDefinition>.From(_parameters); }
			}

			CallingConvention ISignature.CallingConvention
			{
				get
				{
					return UnderlyingPropertySignature.CallingConvention;
				}
			}

			ushort ISignature.ParameterCount
			{
				get { return (ushort)_parameters.Length; }
			}

			IParameterTypeInformation[] ISignature.GetParameters(EmitContext context)
			{
				return StaticCast<IParameterTypeInformation>.From(_parameters);
			}

			ICustomModifier[] ISignature.ReturnValueCustomModifiers
			{
				get
				{
					return UnderlyingPropertySignature.ReturnValueCustomModifiers;
				}
			}

			ICustomModifier[] ISignature.RefCustomModifiers
			{
				get
				{
					return UnderlyingPropertySignature.RefCustomModifiers;
				}
			}

			bool ISignature.ReturnValueIsByRef
			{
				get
				{
					return UnderlyingPropertySignature.ReturnValueIsByRef;
				}
			}

			ITypeReference ISignature.GetType(EmitContext context)
			{
				return UnderlyingPropertySignature.GetType(context);
			}

			protected TEmbeddedMethod AnAccessor
			{
				get
				{
					return _getter ?? _setter;
				}
			}

			ITypeDefinition ITypeDefinitionMember.ContainingTypeDefinition
			{
				get { return ContainingType; }
			}

			TypeMemberVisibility ITypeDefinitionMember.Visibility
			{
				get
				{
					return Visibility;
				}
			}

			ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
			{
				return ContainingType;
			}

			void IReference.Dispatch(MetadataVisitor visitor)
			{
				visitor.Visit((IPropertyDefinition)this);
			}

			IDefinition IReference.AsDefinition(EmitContext context)
			{
				return this;
			}

			string INamedEntity.Name
			{
				get
				{
					return Name;
				}
			}
		}
		public abstract class CommonEmbeddedType : INamespaceTypeDefinition
		{
			public readonly TEmbeddedTypesManager TypeManager;
			public readonly TNamedTypeSymbol UnderlyingNamedType;

			private IFieldDefinition[] _lazyFields;
			private IMethodDefinition[] _lazyMethods;
			private IPropertyDefinition[] _lazyProperties;
			private IEventDefinition[] _lazyEvents;
			private TAttributeData[] _lazyAttributes;
			private int _lazyAssemblyRefIndex = -1;

			protected CommonEmbeddedType(TEmbeddedTypesManager typeManager, TNamedTypeSymbol underlyingNamedType)
			{
				this.TypeManager = typeManager;
				this.UnderlyingNamedType = underlyingNamedType;
			}

			protected abstract int GetAssemblyRefIndex();

			protected abstract IEnumerable<TFieldSymbol> GetFieldsToEmit();
			protected abstract IEnumerable<TMethodSymbol> GetMethodsToEmit();
			protected abstract IEnumerable<TEventSymbol> GetEventsToEmit();
			protected abstract IEnumerable<TPropertySymbol> GetPropertiesToEmit();
			protected abstract bool IsPublic { get; }
			protected abstract bool IsAbstract { get; }
			protected abstract ITypeReference GetBaseClass(TPEModuleBuilder moduleBuilder, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);
			protected abstract IEnumerable<TypeReferenceWithAttributes> GetInterfaces(EmitContext context);
			protected abstract bool IsBeforeFieldInit { get; }
			protected abstract bool IsComImport { get; }
			protected abstract bool IsInterface { get; }
			protected abstract bool IsSerializable { get; }
			protected abstract bool IsSpecialName { get; }
			protected abstract bool IsWindowsRuntimeImport { get; }
			protected abstract bool IsSealed { get; }
			protected abstract TypeLayout? GetTypeLayoutIfStruct();
			protected abstract TAttributeData CreateTypeIdentifierAttribute(bool hasGuid, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);
			protected abstract void EmbedDefaultMembers(string defaultMember, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);
			protected abstract IEnumerable<TAttributeData> GetCustomAttributesToEmit(TPEModuleBuilder moduleBuilder);
			protected abstract void ReportMissingAttribute(AttributeDescription description, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);

			private bool IsTargetAttribute(TAttributeData attrData, AttributeDescription description)
			{
				return TypeManager.IsTargetAttribute(UnderlyingNamedType, attrData, description);
			}

			private TAttributeData[] GetAttributes(TPEModuleBuilder moduleBuilder, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
			{
				var builder = new List<TAttributeData>();

				// Put the CompilerGenerated attribute on the NoPIA types we define so that 
				// static analysis tools (e.g. fxcop) know that they can be skipped
				builder.AddOptional(TypeManager.CreateCompilerGeneratedAttribute());

				// Copy some of the attributes.

				bool hasGuid = false;
				bool hasComEventInterfaceAttribute = false;

				// Note, when porting attributes, we are not using constructors from original symbol.
				// The constructors might be missing (for example, in metadata case) and doing lookup
				// will ensure that we report appropriate errors.

				foreach (var attrData in GetCustomAttributesToEmit(moduleBuilder))
				{
					if (IsTargetAttribute(attrData, AttributeDescription.GuidAttribute))
					{
						string guidString;
						if (attrData.TryGetGuidAttributeValue(out guidString))
						{
							// If this type has a GuidAttribute, we should emit it.
							hasGuid = true;
							builder.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Runtime_InteropServices_GuidAttribute__ctor, attrData, syntaxNodeOpt, diagnostics));
						}
					}
					else if (IsTargetAttribute(attrData, AttributeDescription.ComEventInterfaceAttribute))
					{
						if (attrData.CommonConstructorArguments.Length == 2)
						{
							hasComEventInterfaceAttribute = true;
							builder.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Runtime_InteropServices_ComEventInterfaceAttribute__ctor, attrData, syntaxNodeOpt, diagnostics));
						}
					}
					else
					{
						int signatureIndex = TypeManager.GetTargetAttributeSignatureIndex(UnderlyingNamedType, attrData, AttributeDescription.InterfaceTypeAttribute);
						if (signatureIndex != -1)
						{
							Debug.Assert(signatureIndex == 0 || signatureIndex == 1);
							if (attrData.CommonConstructorArguments.Length == 1)
							{
								builder.AddOptional(TypeManager.CreateSynthesizedAttribute(signatureIndex == 0 ? WellKnownMember.System_Runtime_InteropServices_InterfaceTypeAttribute__ctorInt16 :
									WellKnownMember.System_Runtime_InteropServices_InterfaceTypeAttribute__ctorComInterfaceType,
									attrData, syntaxNodeOpt, diagnostics));
							}
						}
						else if (IsTargetAttribute(attrData, AttributeDescription.BestFitMappingAttribute))
						{
							if (attrData.CommonConstructorArguments.Length == 1)
							{
								builder.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Runtime_InteropServices_BestFitMappingAttribute__ctor, attrData, syntaxNodeOpt, diagnostics));
							}
						}
						else if (IsTargetAttribute(attrData, AttributeDescription.CoClassAttribute))
						{
							if (attrData.CommonConstructorArguments.Length == 1)
							{
								builder.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Runtime_InteropServices_CoClassAttribute__ctor, attrData, syntaxNodeOpt, diagnostics));
							}
						}
						else if (IsTargetAttribute(attrData, AttributeDescription.FlagsAttribute))
						{
							if (attrData.CommonConstructorArguments.Length == 0 && UnderlyingNamedType.IsEnum)
							{
								builder.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_FlagsAttribute__ctor, attrData, syntaxNodeOpt, diagnostics));
							}
						}
						else if (IsTargetAttribute(attrData, AttributeDescription.DefaultMemberAttribute))
						{
							if (attrData.CommonConstructorArguments.Length == 1)
							{
								builder.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Reflection_DefaultMemberAttribute__ctor, attrData, syntaxNodeOpt, diagnostics));

								// Embed members matching default member name.
								string defaultMember = attrData.CommonConstructorArguments[0].Value as string;
								if (defaultMember != null)
								{
									EmbedDefaultMembers(defaultMember, syntaxNodeOpt, diagnostics);
								}
							}
						}
						else if (IsTargetAttribute(attrData, AttributeDescription.UnmanagedFunctionPointerAttribute))
						{
							if (attrData.CommonConstructorArguments.Length == 1)
							{
								builder.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Runtime_InteropServices_UnmanagedFunctionPointerAttribute__ctor, attrData, syntaxNodeOpt, diagnostics));
							}
						}
					}
				}

				// We must emit a TypeIdentifier attribute which connects this local type with the canonical type. 
				// Interfaces usually have a guid attribute, in which case the TypeIdentifier attribute we emit will
				// not need any additional parameters. For interfaces which lack a guid and all other types, we must 
				// emit a TypeIdentifier that has parameters identifying the scope and name of the original type. We 
				// will use the Assembly GUID as the scope identifier.

				if (IsInterface && !hasComEventInterfaceAttribute)
				{
					if (!IsComImport)
					{
						// If we have an interface not marked ComImport, but the assembly is linked, then
						// we need to give an error. We allow event interfaces to not have ComImport marked on them.
						// ERRID_NoPIAAttributeMissing2/ERR_InteropTypeMissingAttribute
						ReportMissingAttribute(AttributeDescription.ComImportAttribute, syntaxNodeOpt, diagnostics);
					}
					else if (!hasGuid)
					{
						// Interfaces used with No-PIA ought to have a guid attribute, or the CLR cannot do type unification. 
						// This interface lacks a guid, so unification probably won't work. We allow event interfaces to not have a Guid.
						// ERRID_NoPIAAttributeMissing2/ERR_InteropTypeMissingAttribute
						ReportMissingAttribute(AttributeDescription.GuidAttribute, syntaxNodeOpt, diagnostics);
					}
				}

				// Note, this logic should match the one in RetargetingSymbolTranslator.RetargetNoPiaLocalType
				// when we try to predict what attributes we will emit on embedded type, which corresponds the 
				// type we are retargeting.

				builder.AddOptional(CreateTypeIdentifierAttribute(hasGuid && IsInterface, syntaxNodeOpt, diagnostics));

				return builder.ToArray();
			}

			public int AssemblyRefIndex
			{
				get
				{
					if (_lazyAssemblyRefIndex == -1)
					{
						_lazyAssemblyRefIndex = GetAssemblyRefIndex();
						Debug.Assert(_lazyAssemblyRefIndex >= 0);
					}
					return _lazyAssemblyRefIndex;
				}
			}

			bool INamespaceTypeDefinition.IsPublic
			{
				get
				{
					return IsPublic;
				}
			}

			ITypeReference ITypeDefinition.GetBaseClass(EmitContext context)
			{
				return GetBaseClass((TPEModuleBuilder)context.Module, (TSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
			}

			IEnumerable<IEventDefinition> ITypeDefinition.GetEvents(EmitContext context)
			{
				if (_lazyEvents == null)
				{
					Debug.Assert(TypeManager.IsFrozen);

					var builder = new List<IEventDefinition>();

					foreach (var e in GetEventsToEmit())
					{
						TEmbeddedEvent embedded;

						if (TypeManager.EmbeddedEventsMap.TryGetValue(e, out embedded))
						{
							builder.Add(embedded);
						}
					}

					_lazyEvents = builder.ToArray();
				}

				return _lazyEvents;
			}

			IEnumerable<MethodImplementation> ITypeDefinition.GetExplicitImplementationOverrides(EmitContext context)
			{
				return new MethodImplementation[0];
			}

			IEnumerable<IFieldDefinition> ITypeDefinition.GetFields(EmitContext context)
			{
				if (_lazyFields == null)
				{
					Debug.Assert(TypeManager.IsFrozen);

					var builder = new List<IFieldDefinition>();

					foreach (var f in GetFieldsToEmit())
					{
						TEmbeddedField embedded;

						if (TypeManager.EmbeddedFieldsMap.TryGetValue(f, out embedded))
						{
							builder.Add(embedded);
						}
					}

					_lazyFields = builder.ToArray();
				}

				return _lazyFields;
			}

			IEnumerable<IGenericTypeParameter> ITypeDefinition.GenericParameters
			{
				get
				{
					return new IGenericTypeParameter[0];
				}
			}

			ushort ITypeDefinition.GenericParameterCount
			{
				get
				{
					return 0;
				}
			}

			bool ITypeDefinition.HasDeclarativeSecurity
			{
				get
				{
					// None of the transferrable attributes are security attributes.
					return false;
				}
			}

			IEnumerable<TypeReferenceWithAttributes> ITypeDefinition.Interfaces(EmitContext context)
			{
				return GetInterfaces(context);
			}

			bool ITypeDefinition.IsAbstract
			{
				get
				{
					return IsAbstract;
				}
			}

			bool ITypeDefinition.IsBeforeFieldInit
			{
				get
				{
					return IsBeforeFieldInit;
				}
			}

			bool ITypeDefinition.IsComObject
			{
				get
				{
					return IsInterface || IsComImport;
				}
			}

			bool ITypeDefinition.IsGeneric
			{
				get
				{
					return false;
				}
			}

			bool ITypeDefinition.IsInterface
			{
				get
				{
					return IsInterface;
				}
			}

			bool ITypeDefinition.IsRuntimeSpecial
			{
				get
				{
					return false;
				}
			}

			bool ITypeDefinition.IsSerializable
			{
				get
				{
					return IsSerializable;
				}
			}

			bool ITypeDefinition.IsSpecialName
			{
				get
				{
					return IsSpecialName;
				}
			}

			bool ITypeDefinition.IsWindowsRuntimeImport
			{
				get
				{
					return IsWindowsRuntimeImport;
				}
			}

			bool ITypeDefinition.IsSealed
			{
				get
				{
					return IsSealed;
				}
			}

			LayoutKind ITypeDefinition.Layout
			{
				get
				{
					var layout = GetTypeLayoutIfStruct();
					return layout?.Kind ?? LayoutKind.Auto;
				}
			}

			ushort ITypeDefinition.Alignment
			{
				get
				{
					var layout = GetTypeLayoutIfStruct();
					return (ushort)(layout?.Alignment ?? 0);
				}
			}

			uint ITypeDefinition.SizeOf
			{
				get
				{
					var layout = GetTypeLayoutIfStruct();
					return (uint)(layout?.Size ?? 0);
				}
			}

			IEnumerable<IMethodDefinition> ITypeDefinition.GetMethods(EmitContext context)
			{
				if (_lazyMethods == null)
				{
					Debug.Assert(TypeManager.IsFrozen);

					var builder = new List<IMethodDefinition>();

					int gapIndex = 1;
					int gapSize = 0;

					foreach (var method in GetMethodsToEmit())
					{
						if ((object)method != null)
						{
							TEmbeddedMethod embedded;

							if (TypeManager.EmbeddedMethodsMap.TryGetValue(method, out embedded))
							{
								if (gapSize > 0)
								{
									builder.Add(new VtblGap(this, string.Format("_VtblGap{0}_{1}", gapIndex, gapSize)));
									gapIndex++;
									gapSize = 0;
								}

								builder.Add(embedded);
							}
							else
							{
								gapSize++;
							}
						}
						else
						{
							gapSize++;
						}
					}

					_lazyMethods = builder.ToArray();
				}

				return _lazyMethods;
			}

			IEnumerable<INestedTypeDefinition> ITypeDefinition.GetNestedTypes(EmitContext context)
			{
				return new INestedTypeDefinition[0];
			}

			IEnumerable<IPropertyDefinition> ITypeDefinition.GetProperties(EmitContext context)
			{
				if (_lazyProperties == null)
				{
					Debug.Assert(TypeManager.IsFrozen);

					var builder = new List<IPropertyDefinition>();

					foreach (var p in GetPropertiesToEmit())
					{
						TEmbeddedProperty embedded;

						if (TypeManager.EmbeddedPropertiesMap.TryGetValue(p, out embedded))
						{
							builder.Add(embedded);
						}
					}

					_lazyProperties = builder.ToArray();
				}

				return _lazyProperties;
			}

			IEnumerable<SecurityAttribute> ITypeDefinition.SecurityAttributes
			{
				get
				{
					// None of the transferrable attributes are security attributes.
					return new SecurityAttribute[0];
				}
			}

			IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
			{
				if (_lazyAttributes == null)
				{
					var diagnostics = DiagnosticBag.GetInstance();
					var attributes = GetAttributes((TPEModuleBuilder)context.Module, (TSyntaxNode)context.SyntaxNodeOpt, diagnostics);

					_lazyAttributes = attributes;
					// Save any diagnostics that we encountered.
					context.Diagnostics.AddRange(diagnostics);

					diagnostics.Free();
				}

				return _lazyAttributes;
			}

			void IReference.Dispatch(MetadataVisitor visitor)
			{
				throw ExceptionUtilities.Unreachable;
			}

			IDefinition IReference.AsDefinition(EmitContext context)
			{
				return this;
			}

			bool ITypeReference.IsEnum
			{
				get
				{
					return UnderlyingNamedType.IsEnum;
				}
			}

			bool ITypeReference.IsValueType
			{
				get
				{
					return UnderlyingNamedType.IsValueType;
				}
			}

			ITypeDefinition ITypeReference.GetResolvedType(EmitContext context)
			{
				return this;
			}

			PrimitiveTypeCode ITypeReference.TypeCode
			{
				get
				{
					return PrimitiveTypeCode.NotPrimitive;
				}
			}

			//TypeDefinitionHandle ITypeReference.TypeDef
			//{
			//	get
			//	{
			//		return default(TypeDefinitionHandle);
			//	}
			//}

			IGenericMethodParameterReference ITypeReference.AsGenericMethodParameterReference
			{
				get
				{
					return null;
				}
			}

			IGenericTypeInstanceReference ITypeReference.AsGenericTypeInstanceReference
			{
				get
				{
					return null;
				}
			}

			IGenericTypeParameterReference ITypeReference.AsGenericTypeParameterReference
			{
				get
				{
					return null;
				}
			}

			INamespaceTypeDefinition ITypeReference.AsNamespaceTypeDefinition(EmitContext context)
			{
				return this;
			}

			INamespaceTypeReference ITypeReference.AsNamespaceTypeReference
			{
				get
				{
					return this;
				}
			}

			INestedTypeDefinition ITypeReference.AsNestedTypeDefinition(EmitContext context)
			{
				return null;
			}

			INestedTypeReference ITypeReference.AsNestedTypeReference
			{
				get
				{
					return null;
				}
			}

			ISpecializedNestedTypeReference ITypeReference.AsSpecializedNestedTypeReference
			{
				get
				{
					return null;
				}
			}

			ITypeDefinition ITypeReference.AsTypeDefinition(EmitContext context)
			{
				return this;
			}

			ushort INamedTypeReference.GenericParameterCount
			{
				get
				{
					return 0;
				}
			}

			bool INamedTypeReference.MangleName
			{
				get
				{
					return UnderlyingNamedType.MangleName;
				}
			}

			string INamedEntity.Name
			{
				get
				{
					return UnderlyingNamedType.Name;
				}
			}

			IUnitReference INamespaceTypeReference.GetUnit(EmitContext context)
			{
				return TypeManager.ModuleBeingBuilt;
			}

			string INamespaceTypeReference.NamespaceName
			{
				get
				{
					return UnderlyingNamedType.NamespaceName;
				}
			}

			/// <remarks>
			/// This is only used for testing.
			/// </remarks>
			public override string ToString()
			{
				return ((ISymbol)UnderlyingNamedType).ToDisplayString();
			}
		}
		public abstract class CommonEmbeddedTypeParameter : IGenericMethodParameter
		{
			public readonly TEmbeddedMethod ContainingMethod;
			public readonly TTypeParameterSymbol UnderlyingTypeParameter;

			protected CommonEmbeddedTypeParameter(TEmbeddedMethod containingMethod, TTypeParameterSymbol underlyingTypeParameter)
			{
				this.ContainingMethod = containingMethod;
				this.UnderlyingTypeParameter = underlyingTypeParameter;
			}

			protected abstract IEnumerable<TypeReferenceWithAttributes> GetConstraints(EmitContext context);
			protected abstract bool MustBeReferenceType { get; }
			protected abstract bool MustBeValueType { get; }
			protected abstract bool MustHaveDefaultConstructor { get; }
			protected abstract string Name { get; }
			protected abstract ushort Index { get; }

			IMethodDefinition IGenericMethodParameter.DefiningMethod
			{
				get
				{
					return ContainingMethod;
				}
			}

			IEnumerable<TypeReferenceWithAttributes> IGenericParameter.GetConstraints(EmitContext context)
			{
				return GetConstraints(context);
			}

			bool IGenericParameter.MustBeReferenceType
			{
				get
				{
					return MustBeReferenceType;
				}
			}

			bool IGenericParameter.MustBeValueType
			{
				get
				{
					return MustBeValueType;
				}
			}

			bool IGenericParameter.MustHaveDefaultConstructor
			{
				get
				{
					return MustHaveDefaultConstructor;
				}
			}

			TypeParameterVariance IGenericParameter.Variance
			{
				get
				{
					// Method type parameters are not variant
					return TypeParameterVariance.NonVariant;
				}
			}

			IGenericMethodParameter IGenericParameter.AsGenericMethodParameter
			{
				get
				{
					return this;
				}
			}

			IGenericTypeParameter IGenericParameter.AsGenericTypeParameter
			{
				get
				{
					return null;
				}
			}

			bool ITypeReference.IsEnum
			{
				get { return false; }
			}

			bool ITypeReference.IsValueType
			{
				get { return false; }
			}

			ITypeDefinition ITypeReference.GetResolvedType(EmitContext context)
			{
				return null;
			}

			PrimitiveTypeCode ITypeReference.TypeCode
			{
				get
				{
					return PrimitiveTypeCode.NotPrimitive;
				}
			}

			//TypeDefinitionHandle ITypeReference.TypeDef
			//{
			//	get { return default(TypeDefinitionHandle); }
			//}

			IGenericMethodParameterReference ITypeReference.AsGenericMethodParameterReference
			{
				get { return this; }
			}

			IGenericTypeInstanceReference ITypeReference.AsGenericTypeInstanceReference
			{
				get { return null; }
			}

			IGenericTypeParameterReference ITypeReference.AsGenericTypeParameterReference
			{
				get { return null; }
			}

			INamespaceTypeDefinition ITypeReference.AsNamespaceTypeDefinition(EmitContext context)
			{
				return null;
			}

			INamespaceTypeReference ITypeReference.AsNamespaceTypeReference
			{
				get { return null; }
			}

			INestedTypeDefinition ITypeReference.AsNestedTypeDefinition(EmitContext context)
			{
				return null;
			}

			INestedTypeReference ITypeReference.AsNestedTypeReference
			{
				get { return null; }
			}

			ISpecializedNestedTypeReference ITypeReference.AsSpecializedNestedTypeReference
			{
				get { return null; }
			}

			ITypeDefinition ITypeReference.AsTypeDefinition(EmitContext context)
			{
				return null;
			}

			IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
			{
				// TODO:
				return new ICustomAttribute[0];
			}

			void IReference.Dispatch(MetadataVisitor visitor)
			{
				throw ExceptionUtilities.Unreachable;
			}

			IDefinition IReference.AsDefinition(EmitContext context)
			{
				return null;
			}

			string INamedEntity.Name
			{
				get { return Name; }
			}

			ushort IParameterListEntry.Index
			{
				get
				{
					return Index;
				}
			}

			IMethodReference IGenericMethodParameterReference.DefiningMethod
			{
				get { return ContainingMethod; }
			}
		}
	}
}
