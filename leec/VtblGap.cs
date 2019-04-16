using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class VtblGap : IMethodDefinition
	{
		public readonly ITypeDefinition ContainingType;
		private readonly string _name;

		public VtblGap(ITypeDefinition containingType, string name)
		{
			this.ContainingType = containingType;
			_name = name;
		}

		IMethodBody IMethodDefinition.GetBody(EmitContext context)
		{
			return null;
		}

		IEnumerable<IGenericMethodParameter> IMethodDefinition.GenericParameters
		{
			get { return new IGenericMethodParameter[0]; }
		}

		bool IMethodDefinition.IsImplicitlyDeclared
		{
			get { return true; }
		}

		bool IMethodDefinition.HasDeclarativeSecurity
		{
			get { return false; }
		}

		bool IMethodDefinition.IsAbstract
		{
			get { return false; }
		}

		bool IMethodDefinition.IsAccessCheckedOnOverride
		{
			get { return false; }
		}

		bool IMethodDefinition.IsConstructor
		{
			get { return false; }
		}

		bool IMethodDefinition.IsExternal
		{
			get { return false; }
		}

		bool IMethodDefinition.IsHiddenBySignature
		{
			get { return false; }
		}

		bool IMethodDefinition.IsNewSlot
		{
			get { return false; }
		}

		bool IMethodDefinition.IsPlatformInvoke
		{
			get { return false; }
		}

		bool IMethodDefinition.IsRuntimeSpecial
		{
			get { return true; }
		}

		bool IMethodDefinition.IsSealed
		{
			get { return false; }
		}

		bool IMethodDefinition.IsSpecialName
		{
			get { return true; }
		}

		bool IMethodDefinition.IsStatic
		{
			get { return false; }
		}

		bool IMethodDefinition.IsVirtual
		{
			get { return false; }
		}

		MethodImplAttributes IMethodDefinition.GetImplementationAttributes(EmitContext context)
		{
			return MethodImplAttributes.Managed | MethodImplAttributes.Runtime;
		}

		IParameterDefinition[] IMethodDefinition.Parameters
		{
			get { return new IParameterDefinition[0]; }
		}

		IPlatformInvokeInformation IMethodDefinition.PlatformInvokeData
		{
			get { return null; }
		}

		bool IMethodDefinition.RequiresSecurityObject
		{
			get { return false; }
		}

		ICustomAttribute[] IMethodDefinition.GetReturnValueAttributes(EmitContext context)
		{
			return new ICustomAttribute[0];
		}

		bool IMethodDefinition.ReturnValueIsMarshalledExplicitly
		{
			get { return false; }
		}

		IMarshallingInformation IMethodDefinition.ReturnValueMarshallingInformation
		{
			get { return null; }
		}

		byte[] IMethodDefinition.ReturnValueMarshallingDescriptor
		{
			get { return null; }
		}

		IEnumerable<SecurityAttribute> IMethodDefinition.SecurityAttributes
		{
			get { return new SecurityAttribute[0]; }
		}

		ITypeDefinition ITypeDefinitionMember.ContainingTypeDefinition
		{
			get { return ContainingType; }
		}

		INamespace IMethodDefinition.ContainingNamespace
		{
			get
			{
				// The containing namespace is only used for methods for which we generate debug information.
				return null;
			}
		}

		TypeMemberVisibility ITypeDefinitionMember.Visibility
		{
			get { return TypeMemberVisibility.Public; }
		}

		ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
		{
			return ContainingType;
		}

		IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
		{
			return new ICustomAttribute[0];
		}

		void IReference.Dispatch(MetadataVisitor visitor)
		{
			visitor.Visit((IMethodDefinition)this);
		}

		IDefinition IReference.AsDefinition(EmitContext context)
		{
			return this;
		}

		string INamedEntity.Name
		{
			get { return _name; }
		}

		bool IMethodReference.AcceptsExtraArguments
		{
			get { return false; }
		}

		ushort IMethodReference.GenericParameterCount
		{
			get { return 0; }
		}

		bool IMethodReference.IsGeneric
		{
			get { return false; }
		}

		IMethodDefinition IMethodReference.GetResolvedMethod(EmitContext context)
		{
			return this;
		}

		IParameterTypeInformation[] IMethodReference.ExtraParameters
		{
			get { return new IParameterTypeInformation[0]; }
		}

		IGenericMethodInstanceReference IMethodReference.AsGenericMethodInstanceReference
		{
			get { return null; }
		}

		ISpecializedMethodReference IMethodReference.AsSpecializedMethodReference
		{
			get { return null; }
		}

		CallingConvention ISignature.CallingConvention
		{
			get { return CallingConvention.Default | CallingConvention.HasThis; }
		}

		ushort ISignature.ParameterCount
		{
			get { return 0; }
		}

		IParameterTypeInformation[] ISignature.GetParameters(EmitContext context)
		{
			return new IParameterTypeInformation[0];
		}

		ICustomModifier[] ISignature.ReturnValueCustomModifiers
		{
			get { return new ICustomModifier[0]; }
		}

		ICustomModifier[] ISignature.RefCustomModifiers
		{
			get { return new ICustomModifier[0]; }
		}

		bool ISignature.ReturnValueIsByRef
		{
			get { return false; }
		}

		ITypeReference ISignature.GetType(EmitContext context)
		{
			return context.Module.GetPlatformType(PlatformType.SystemVoid, context);
		}
	}
}
