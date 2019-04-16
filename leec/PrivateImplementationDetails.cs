using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace leec
{
	public sealed class PrivateImplementationDetails : DefaultTypeDef, INamespaceTypeDefinition
	{
		// Note: Dev11 uses the source method token as the prefix, rather than a fixed token
		// value, and data field offsets are unique within the method, not across all methods.
		internal const string SynthesizedStringHashFunctionName = "ComputeStringHash";

		private readonly CommonPEModuleBuilder _moduleBuilder;       //the module builder
		private readonly ITypeReference _systemObject;           //base type
		private readonly ITypeReference _systemValueType;        //base for nested structs

		private readonly ITypeReference _systemInt8Type;         //for metadata init of byte arrays
		private readonly ITypeReference _systemInt16Type;        //for metadata init of short arrays
		private readonly ITypeReference _systemInt32Type;        //for metadata init of int arrays
		private readonly ITypeReference _systemInt64Type;        //for metadata init of long arrays

		private readonly ICustomAttribute _compilerGeneratedAttribute;

		private readonly string _name;

		// Once frozen the collections of fields, methods and types are immutable.
		private int _frozen;

		// fields mapped to metadata blocks
		private SynthesizedStaticField[] _orderedSynthesizedFields;
		private readonly Dictionary<byte[], MappedField> _mappedFields = new Dictionary<byte[], MappedField>();

		private ModuleVersionIdField _mvidField;
		// Dictionary that maps from analysis kind to instrumentation payload field.
		private readonly Dictionary<int, InstrumentationPayloadRootField> _instrumentationPayloadRootFields = new Dictionary<int, InstrumentationPayloadRootField>();

		// synthesized methods
		private IMethodDefinition[] _orderedSynthesizedMethods;
		private readonly Dictionary<string, IMethodDefinition> _synthesizedMethods =
			new Dictionary<string, IMethodDefinition>();

		// field types for different block sizes.
		private ITypeReference[] _orderedProxyTypes;
		private readonly Dictionary<uint, ITypeReference> _proxyTypes = new Dictionary<uint, ITypeReference>();

		internal PrivateImplementationDetails(
			CommonPEModuleBuilder moduleBuilder,
			string moduleName,
			int submissionSlotIndex,
			ITypeReference systemObject,
			ITypeReference systemValueType,
			ITypeReference systemInt8Type,
			ITypeReference systemInt16Type,
			ITypeReference systemInt32Type,
			ITypeReference systemInt64Type,
			ICustomAttribute compilerGeneratedAttribute)
		{
			Debug.Assert(systemObject != null);
			Debug.Assert(systemValueType != null);

			_moduleBuilder = moduleBuilder;
			_systemObject = systemObject;
			_systemValueType = systemValueType;

			_systemInt8Type = systemInt8Type;
			_systemInt16Type = systemInt16Type;
			_systemInt32Type = systemInt32Type;
			_systemInt64Type = systemInt64Type;

			_compilerGeneratedAttribute = compilerGeneratedAttribute;

			var isNetModule = moduleBuilder.OutputKind == OutputKind.NetModule;
			_name = GetClassName(moduleName, submissionSlotIndex, isNetModule);
		}

		private static string GetClassName(string moduleName, int submissionSlotIndex, bool isNetModule)
		{
			// we include the module name in the name of the PrivateImplementationDetails class so that more than
			// one of them can be included in an assembly as part of netmodules.    
			var name = isNetModule ?
						$"<PrivateImplementationDetails><{MetadataHelpers.MangleForTypeNameIfNeeded(moduleName)}>" :
						$"<PrivateImplementationDetails>";

			if (submissionSlotIndex >= 0)
			{
				name += submissionSlotIndex.ToString();
			}

			return name;
		}

		internal void Freeze()
		{
			var wasFrozen = Interlocked.Exchange(ref _frozen, 1);
			if (wasFrozen != 0)
			{
				throw new InvalidOperationException();
			}

			// Sort fields.
			var fieldsBuilder = new List<SynthesizedStaticField>(_mappedFields.Count + (_mvidField != null ? 1 : 0));
			fieldsBuilder.AddRange(_mappedFields.Values);
			if (_mvidField != null)
			{
				fieldsBuilder.Add(_mvidField);
			}
			fieldsBuilder.AddRange(_instrumentationPayloadRootFields.Values);
			fieldsBuilder.Sort(FieldComparer.Instance);
			_orderedSynthesizedFields = fieldsBuilder.ToArray();

			// Sort methods.
			_orderedSynthesizedMethods = _synthesizedMethods.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToArray();

			// Sort proxy types.
			_orderedProxyTypes = _proxyTypes.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToArray();
		}

		private bool IsFrozen => _frozen != 0;

		internal IFieldReference CreateDataField(byte[] data)
		{
			Debug.Assert(!IsFrozen);
			ITypeReference type;
			uint k = (uint)data.Length;
			if (!_proxyTypes.TryGetValue(k, out type))
			{
				type = GetStorageStruct(k);
				_proxyTypes.Add(k, type);
			}

			MappedField field;
			if (!_mappedFields.TryGetValue(data, out field))
			{
				var name = GenerateDataFieldName(data);
				field = new MappedField(name, this, type, data);
			}
			return field;
		}

		private ITypeReference GetStorageStruct(uint size)
		{
			switch (size)
			{
				case 1:
					return _systemInt8Type ?? new ExplicitSizeStruct(1, this, _systemValueType);
				case 2:
					return _systemInt16Type ?? new ExplicitSizeStruct(2, this, _systemValueType);
				case 4:
					return _systemInt32Type ?? new ExplicitSizeStruct(4, this, _systemValueType);
				case 8:
					return _systemInt64Type ?? new ExplicitSizeStruct(8, this, _systemValueType);
			}

			return new ExplicitSizeStruct(size, this, _systemValueType);
		}

		internal IFieldReference GetModuleVersionId(ITypeReference mvidType)
		{
			if (_mvidField == null)
			{
				Debug.Assert(!IsFrozen);
				Interlocked.CompareExchange(ref _mvidField, new ModuleVersionIdField(this, mvidType), null);
			}

			Debug.Assert(_mvidField.Type.Equals(mvidType));
			return _mvidField;
		}

		internal IFieldReference GetOrAddInstrumentationPayloadRoot(int analysisKind, ITypeReference payloadRootType)
		{
			InstrumentationPayloadRootField payloadRootField;
			if (!_instrumentationPayloadRootFields.TryGetValue(analysisKind, out payloadRootField))
			{
				Debug.Assert(!IsFrozen);
				if (!_instrumentationPayloadRootFields.TryGetValue(analysisKind, out payloadRootField))
				{
					payloadRootField = new InstrumentationPayloadRootField(this, analysisKind, payloadRootType);
				}
			}

			Debug.Assert(payloadRootField.Type.Equals(payloadRootType));
			return payloadRootField;
		}

		// Get the instrumentation payload roots ordered by analysis kind.
		internal IOrderedEnumerable<KeyValuePair<int, InstrumentationPayloadRootField>> GetInstrumentationPayloadRoots()
		{
			Debug.Assert(IsFrozen);
			return _instrumentationPayloadRootFields.OrderBy(analysis => analysis.Key);
		}

		// Add a new synthesized method indexed by its name if the method isn't already present.
		internal bool TryAddSynthesizedMethod(IMethodDefinition method)
		{
			Debug.Assert(!IsFrozen);
			if (_synthesizedMethods.ContainsKey(method.Name))
				return false;

			_synthesizedMethods[method.Name] = method;
			return true;
		}

		public override IEnumerable<IFieldDefinition> GetFields(EmitContext context)
		{
			Debug.Assert(IsFrozen);
			return _orderedSynthesizedFields;
		}

		public override IEnumerable<IMethodDefinition> GetMethods(EmitContext context)
		{
			Debug.Assert(IsFrozen);
			return _orderedSynthesizedMethods;
		}

		// Get method by name, if one exists. Otherwise return null.
		internal IMethodDefinition GetMethod(string name)
		{
			IMethodDefinition method;
			_synthesizedMethods.TryGetValue(name, out method);
			return method;
		}

		public override IEnumerable<INestedTypeDefinition> GetNestedTypes(EmitContext context)
		{
			Debug.Assert(IsFrozen);
			return _orderedProxyTypes.OfType<ExplicitSizeStruct>();
		}

		public override string ToString() => this.Name;

		public override ITypeReference GetBaseClass(EmitContext context) => _systemObject;

		public override IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
		{
			if (_compilerGeneratedAttribute != null)
			{
				return new ICustomAttribute[] { _compilerGeneratedAttribute };
			}

			return new ICustomAttribute[0];
		}

		public override void Dispatch(MetadataVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override INamespaceTypeDefinition AsNamespaceTypeDefinition(EmitContext context) => this;

		public override INamespaceTypeReference AsNamespaceTypeReference => this;

		public string Name => _name;

		public bool IsPublic => false;

		public IUnitReference GetUnit(EmitContext context)
		{
			Debug.Assert(context.Module == _moduleBuilder);
			return _moduleBuilder;
		}

		public string NamespaceName => string.Empty;

		internal static string GenerateDataFieldName(byte[] data)
		{
			// TODO: replace SHA1 with non-crypto alg: https://github.com/dotnet/roslyn/issues/24737
			var hash = Hash.ComputeHash(data);
			char[] c = new char[hash.Length * 2];
			int i = 0;
			foreach (var b in hash)
			{
				c[i++] = Hexchar(b >> 4);
				c[i++] = Hexchar(b & 0xF);
			}

			return new string(c);
		}

		private static char Hexchar(int x)
			=> (char)((x <= 9) ? (x + '0') : (x + ('A' - 10)));

		private sealed class FieldComparer : IComparer<SynthesizedStaticField>
		{
			public static readonly FieldComparer Instance = new FieldComparer();

			private FieldComparer()
			{
			}

			public int Compare(SynthesizedStaticField x, SynthesizedStaticField y)
			{
				// Fields are always synthesized with non-null names.
				Debug.Assert(x.Name != null && y.Name != null);
				return x.Name.CompareTo(y.Name);
			}
		}
	}

	/// <summary>
	/// Simple struct type with explicit size and no members.
	/// </summary>
	internal sealed class ExplicitSizeStruct : DefaultTypeDef, INestedTypeDefinition
	{
		private readonly uint _size;
		private readonly INamedTypeDefinition _containingType;
		private readonly ITypeReference _sysValueType;

		internal ExplicitSizeStruct(uint size, PrivateImplementationDetails containingType, ITypeReference sysValueType)
		{
			_size = size;
			_containingType = containingType;
			_sysValueType = sysValueType;
		}

		public override string ToString()
			=> _containingType.ToString() + "." + this.Name;

		override public ushort Alignment => 1;

		override public ITypeReference GetBaseClass(EmitContext context) => _sysValueType;

		override public LayoutKind Layout => LayoutKind.Explicit;

		override public uint SizeOf => _size;

		override public void Dispatch(MetadataVisitor visitor)
		{
			visitor.Visit(this);
		}

		public string Name => "__StaticArrayInitTypeSize=" + _size;

		public ITypeDefinition ContainingTypeDefinition => _containingType;

		public TypeMemberVisibility Visibility => TypeMemberVisibility.Private;

		public override bool IsValueType => true;

		public ITypeReference GetContainingType(EmitContext context) => _containingType;

		public override INestedTypeDefinition AsNestedTypeDefinition(EmitContext context) => this;

		public override INestedTypeReference AsNestedTypeReference => this;
	}

	internal abstract class SynthesizedStaticField : IFieldDefinition
	{
		private readonly INamedTypeDefinition _containingType;
		private readonly ITypeReference _type;
		private readonly string _name;

		internal SynthesizedStaticField(string name, INamedTypeDefinition containingType, ITypeReference type)
		{
			Debug.Assert(name != null);
			Debug.Assert(containingType != null);
			Debug.Assert(type != null);

			_containingType = containingType;
			_type = type;
			_name = name;
		}

		public override string ToString() => $"{_type} {_containingType}.{this.Name}";

		public MetadataConstant GetCompileTimeValue(EmitContext context) => null;

		public abstract byte[] MappedData { get; }

		public bool IsCompileTimeConstant => false;

		public bool IsNotSerialized => false;

		public bool IsReadOnly => true;

		public bool IsRuntimeSpecial => false;

		public bool IsSpecialName => false;

		public bool IsStatic => true;

		public bool IsMarshalledExplicitly => false;

		public IMarshallingInformation MarshallingInformation => null;

		public byte[] MarshallingDescriptor => null;

		public int Offset
		{
			get { throw ExceptionUtilities.Unreachable; }
		}

		public ITypeDefinition ContainingTypeDefinition => _containingType;

		public TypeMemberVisibility Visibility => TypeMemberVisibility.Assembly;

		public ITypeReference GetContainingType(EmitContext context) => _containingType;

		public IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
			=> new ICustomAttribute[0];

		public void Dispatch(MetadataVisitor visitor)
		{
			visitor.Visit(this);
		}

		public IDefinition AsDefinition(EmitContext context)
		{
			throw ExceptionUtilities.Unreachable;
		}

		public string Name => _name;

		public bool IsContextualNamedEntity => false;

		public ITypeReference GetType(EmitContext context) => _type;

		internal ITypeReference Type => _type;

		public IFieldDefinition GetResolvedField(EmitContext context) => this;

		public ISpecializedFieldReference AsSpecializedFieldReference => null;

		public MetadataConstant Constant
		{
			get { throw ExceptionUtilities.Unreachable; }
		}
	}

	internal sealed class ModuleVersionIdField : SynthesizedStaticField
	{
		internal ModuleVersionIdField(INamedTypeDefinition containingType, ITypeReference type)
			: base("MVID", containingType, type)
		{
		}

		public override byte[] MappedData => null;
	}

	internal sealed class InstrumentationPayloadRootField : SynthesizedStaticField
	{
		internal InstrumentationPayloadRootField(INamedTypeDefinition containingType, int analysisIndex, ITypeReference payloadType)
			: base("PayloadRoot" + analysisIndex.ToString(), containingType, payloadType)
		{
		}

		public override byte[] MappedData => null;
	}

	/// <summary>
	/// Definition of a simple field mapped to a metadata block
	/// </summary>
	internal sealed class MappedField : SynthesizedStaticField
	{
		private readonly byte[] _block;

		internal MappedField(string name, INamedTypeDefinition containingType, ITypeReference type, byte[] block)
			: base(name, containingType, type)
		{
			Debug.Assert(block != null);

			_block = block;
		}

		public override byte[] MappedData => _block;
	}

	/// <summary>
	/// Just a default implementation of a type definition.
	/// </summary>
	public abstract class DefaultTypeDef : ITypeDefinition
	{
		public IEnumerable<IEventDefinition> GetEvents(EmitContext context)
			=> new IEventDefinition[0];

		public IEnumerable<MethodImplementation> GetExplicitImplementationOverrides(EmitContext context)
			=> new MethodImplementation[0];

		virtual public IEnumerable<IFieldDefinition> GetFields(EmitContext context)
			=> new IFieldDefinition[0];

		public IEnumerable<IGenericTypeParameter> GenericParameters
			=> new IGenericTypeParameter[0];

		public ushort GenericParameterCount => 0;

		public bool HasDeclarativeSecurity => false;

		public IEnumerable<TypeReferenceWithAttributes> Interfaces(EmitContext context)
			=> new TypeReferenceWithAttributes[0];

		public bool IsAbstract => false;

		public bool IsBeforeFieldInit => false;

		public bool IsComObject => false;

		public bool IsGeneric => false;

		public bool IsInterface => false;

		public bool IsRuntimeSpecial => false;

		public bool IsSerializable => false;

		public bool IsSpecialName => false;

		public bool IsWindowsRuntimeImport => false;

		public bool IsSealed => true;

		public virtual IEnumerable<IMethodDefinition> GetMethods(EmitContext context)
			=> new IMethodDefinition[0];

		public virtual IEnumerable<INestedTypeDefinition> GetNestedTypes(EmitContext context)
			=> new INestedTypeDefinition[0];

		public IEnumerable<IPropertyDefinition> GetProperties(EmitContext context)
			=> new IPropertyDefinition[0];

		public IEnumerable<SecurityAttribute> SecurityAttributes
			=> new SecurityAttribute[0];

		public virtual IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
			=> new ICustomAttribute[0];

		public IDefinition AsDefinition(EmitContext context) => this;

		public bool IsEnum => false;

		public ITypeDefinition GetResolvedType(EmitContext context) => this;

		public PrimitiveTypeCode TypeCode => PrimitiveTypeCode.NotPrimitive;

		//public TypeDefinitionHandle TypeDef
		//{
		//	get { throw ExceptionUtilities.Unreachable; }
		//}

		public IGenericMethodParameterReference AsGenericMethodParameterReference => null;

		public IGenericTypeInstanceReference AsGenericTypeInstanceReference => null;

		public IGenericTypeParameterReference AsGenericTypeParameterReference => null;

		public virtual INamespaceTypeDefinition AsNamespaceTypeDefinition(EmitContext context) => null;

		public virtual INamespaceTypeReference AsNamespaceTypeReference => null;

		public ISpecializedNestedTypeReference AsSpecializedNestedTypeReference => null;

		public virtual INestedTypeDefinition AsNestedTypeDefinition(EmitContext context) => null;

		public virtual INestedTypeReference AsNestedTypeReference => null;

		public ITypeDefinition AsTypeDefinition(EmitContext context) => this;

		public bool MangleName => false;

		public virtual ushort Alignment => 0;

		public virtual ITypeReference GetBaseClass(EmitContext context)
		{
			throw ExceptionUtilities.Unreachable;
		}

		public virtual LayoutKind Layout => LayoutKind.Auto;

		public virtual uint SizeOf => 0;

		public virtual void Dispatch(MetadataVisitor visitor)
		{
			throw ExceptionUtilities.Unreachable;
		}

		public virtual bool IsValueType => false;
	}
}
