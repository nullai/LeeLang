using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	internal static class ITypeReferenceExtensions
	{
		internal static void GetConsolidatedTypeArguments(this ITypeReference typeReference, List<ITypeReference> consolidatedTypeArguments, EmitContext context)
		{
			INestedTypeReference nestedTypeReference = typeReference.AsNestedTypeReference;
			nestedTypeReference?.GetContainingType(context).GetConsolidatedTypeArguments(consolidatedTypeArguments, context);

			IGenericTypeInstanceReference genTypeInstance = typeReference.AsGenericTypeInstanceReference;
			if (genTypeInstance != null)
			{
				consolidatedTypeArguments.AddRange(genTypeInstance.GetGenericArguments(context));
			}
		}

		internal static ITypeReference GetUninstantiatedGenericType(this ITypeReference typeReference, EmitContext context)
		{
			IGenericTypeInstanceReference genericTypeInstanceReference = typeReference.AsGenericTypeInstanceReference;
			if (genericTypeInstanceReference != null)
			{
				return genericTypeInstanceReference.GetGenericType(context);
			}

			ISpecializedNestedTypeReference specializedNestedType = typeReference.AsSpecializedNestedTypeReference;
			if (specializedNestedType != null)
			{
				return specializedNestedType.GetUnspecializedVersion(context);
			}

			return typeReference;
		}

		internal static bool IsTypeSpecification(this ITypeReference typeReference)
		{
			INestedTypeReference nestedTypeReference = typeReference.AsNestedTypeReference;
			if (nestedTypeReference != null)
			{
				return nestedTypeReference.AsSpecializedNestedTypeReference != null ||
					nestedTypeReference.AsGenericTypeInstanceReference != null;
			}

			return typeReference.AsNamespaceTypeReference == null;
		}
	}
}
