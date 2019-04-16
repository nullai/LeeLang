using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	internal static class ReferenceDependencyWalker
	{
		public static void VisitReference(IReference reference, EmitContext context)
		{
			var typeReference = reference as ITypeReference;
			if (typeReference != null)
			{
				VisitTypeReference(typeReference, context);
				return;
			}

			var methodReference = reference as IMethodReference;
			if (methodReference != null)
			{
				VisitMethodReference(methodReference, context);
				return;
			}

			var fieldReference = reference as IFieldReference;
			if (fieldReference != null)
			{
				VisitFieldReference(fieldReference, context);
				return;
			}
		}

		private static void VisitTypeReference(ITypeReference typeReference, EmitContext context)
		{
			Debug.Assert(typeReference != null);

			IArrayTypeReference arrayType = typeReference as IArrayTypeReference;
			if (arrayType != null)
			{
				VisitTypeReference(arrayType.GetElementType(context), context);
				return;
			}

			IPointerTypeReference pointerType = typeReference as IPointerTypeReference;
			if (pointerType != null)
			{
				VisitTypeReference(pointerType.GetTargetType(context), context);
				return;
			}

			//IManagedPointerTypeReference managedPointerType = typeReference as IManagedPointerTypeReference;
			//if (managedPointerType != null)
			//{
			//    VisitTypeReference(managedPointerType.GetTargetType(this.context));
			//    return;
			//}

			IModifiedTypeReference modifiedType = typeReference as IModifiedTypeReference;
			if (modifiedType != null)
			{
				foreach (var custModifier in modifiedType.CustomModifiers)
				{
					VisitTypeReference(custModifier.GetModifier(context), context);
				}
				VisitTypeReference(modifiedType.UnmodifiedType, context);
				return;
			}

			// Visit containing type
			INestedTypeReference nestedType = typeReference.AsNestedTypeReference;
			if (nestedType != null)
			{
				VisitTypeReference(nestedType.GetContainingType(context), context);
			}

			// Visit generic arguments
			IGenericTypeInstanceReference genericInstance = typeReference.AsGenericTypeInstanceReference;
			if (genericInstance != null)
			{
				foreach (var arg in genericInstance.GetGenericArguments(context))
				{
					VisitTypeReference(arg, context);
				}
			}
		}

		private static void VisitMethodReference(IMethodReference methodReference, EmitContext context)
		{
			Debug.Assert(methodReference != null);

			// Visit containing type
			VisitTypeReference(methodReference.GetContainingType(context), context);

			// Visit generic arguments if any
			IGenericMethodInstanceReference genericInstance = methodReference.AsGenericMethodInstanceReference;
			if (genericInstance != null)
			{
				foreach (var arg in genericInstance.GetGenericArguments(context))
				{
					VisitTypeReference(arg, context);
				}
				methodReference = genericInstance.GetGenericMethod(context);
			}

			// Translate substituted method to original definition
			ISpecializedMethodReference specializedMethod = methodReference.AsSpecializedMethodReference;
			if (specializedMethod != null)
			{
				methodReference = specializedMethod.UnspecializedVersion;
			}

			// Visit parameter types
			VisitParameters(methodReference.GetParameters(context), context);

			if (methodReference.AcceptsExtraArguments)
			{
				VisitParameters(methodReference.ExtraParameters, context);
			}

			// Visit return value type
			VisitTypeReference(methodReference.GetType(context), context);

			foreach (var typeModifier in methodReference.RefCustomModifiers)
			{
				VisitTypeReference(typeModifier.GetModifier(context), context);
			}

			foreach (var typeModifier in methodReference.ReturnValueCustomModifiers)
			{
				VisitTypeReference(typeModifier.GetModifier(context), context);
			}
		}

		private static void VisitParameters(IParameterTypeInformation[] parameters, EmitContext context)
		{
			foreach (var param in parameters)
			{
				VisitTypeReference(param.GetType(context), context);

				foreach (var typeModifier in param.RefCustomModifiers)
				{
					VisitTypeReference(typeModifier.GetModifier(context), context);
				}

				foreach (var typeModifier in param.CustomModifiers)
				{
					VisitTypeReference(typeModifier.GetModifier(context), context);
				}
			}
		}

		private static void VisitFieldReference(IFieldReference fieldReference, EmitContext context)
		{
			Debug.Assert(fieldReference != null);

			// Visit containing type
			VisitTypeReference(fieldReference.GetContainingType(context), context);

			// Translate substituted field to original definition
			ISpecializedFieldReference specializedField = fieldReference.AsSpecializedFieldReference;
			if (specializedField != null)
			{
				fieldReference = specializedField.UnspecializedVersion;
			}

			// Visit field type
			VisitTypeReference(fieldReference.GetType(context), context);
		}
	}
}
