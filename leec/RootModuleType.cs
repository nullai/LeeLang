using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	internal class RootModuleType : INamespaceTypeDefinition
	{
		//public TypeDefinitionHandle TypeDef
		//{
		//	get { return default(TypeDefinitionHandle); }
		//}

		public ITypeDefinition ResolvedType
		{
			get { return this; }
		}

		public IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
		{
			return new ICustomAttribute[0];
		}

		public bool MangleName
		{
			get { return false; }
		}

		public string Name
		{
			get { return "<Module>"; }
		}

		public ushort Alignment
		{
			get { return 0; }
		}

		public ITypeReference GetBaseClass(EmitContext context)
		{
			return null;
		}

		public IEnumerable<IEventDefinition> GetEvents(EmitContext context)
		{
			return new IEventDefinition[0];
		}

		public IEnumerable<MethodImplementation> GetExplicitImplementationOverrides(EmitContext context)
		{
			return new MethodImplementation[0];
		}

		public IEnumerable<IFieldDefinition> GetFields(EmitContext context)
		{
			return new IFieldDefinition[0];
		}

		public bool HasDeclarativeSecurity
		{
			get { return false; }
		}

		public IEnumerable<TypeReferenceWithAttributes> Interfaces(EmitContext context)
		{
			return new TypeReferenceWithAttributes[0];
		}

		public bool IsAbstract
		{
			get { return false; }
		}

		public bool IsBeforeFieldInit
		{
			get { return false; }
		}

		public bool IsComObject
		{
			get { return false; }
		}

		public bool IsGeneric
		{
			get { return false; }
		}

		public bool IsInterface
		{
			get { return false; }
		}

		public bool IsRuntimeSpecial
		{
			get { return false; }
		}

		public bool IsSerializable
		{
			get { return false; }
		}

		public bool IsSpecialName
		{
			get { return false; }
		}

		public bool IsWindowsRuntimeImport
		{
			get { return false; }
		}

		public bool IsSealed
		{
			get { return false; }
		}

		public LayoutKind Layout
		{
			get { return LayoutKind.Auto; }
		}

		public IEnumerable<IMethodDefinition> GetMethods(EmitContext context)
		{
			return new IMethodDefinition[0];
		}

		public IEnumerable<INestedTypeDefinition> GetNestedTypes(EmitContext context)
		{
			return new INestedTypeDefinition[0];
		}

		public IEnumerable<IPropertyDefinition> GetProperties(EmitContext context)
		{
			return new IPropertyDefinition[0];
		}

		public uint SizeOf
		{
			get { return 0; }
		}

		public bool IsPublic
		{
			get { return false; }
		}

		public bool IsNested
		{
			get { return false; }
		}

		IEnumerable<IGenericTypeParameter> ITypeDefinition.GenericParameters
		{
			get { throw ExceptionUtilities.Unreachable; }
		}

		ushort ITypeDefinition.GenericParameterCount
		{
			get
			{
				return 0;
			}
		}

		IEnumerable<SecurityAttribute> ITypeDefinition.SecurityAttributes
		{
			get { throw ExceptionUtilities.Unreachable; }
		}

		void IReference.Dispatch(MetadataVisitor visitor)
		{
			throw ExceptionUtilities.Unreachable;
		}

		bool ITypeReference.IsEnum
		{
			get { throw ExceptionUtilities.Unreachable; }
		}

		bool ITypeReference.IsValueType
		{
			get { throw ExceptionUtilities.Unreachable; }
		}

		ITypeDefinition ITypeReference.GetResolvedType(EmitContext context)
		{
			return this;
		}

		PrimitiveTypeCode ITypeReference.TypeCode
		{
			get { throw ExceptionUtilities.Unreachable; }
		}

		ushort INamedTypeReference.GenericParameterCount
		{
			get { throw ExceptionUtilities.Unreachable; }
		}

		IUnitReference INamespaceTypeReference.GetUnit(EmitContext context)
		{
			throw ExceptionUtilities.Unreachable;
		}

		string INamespaceTypeReference.NamespaceName
		{
			get
			{
				return string.Empty;
			}
		}

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

		IDefinition IReference.AsDefinition(EmitContext context)
		{
			return this;
		}
	}
}
