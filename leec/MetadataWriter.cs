using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace leec
{
	public class MetadataWriter
	{
		public static string GetMangledName(INamedTypeReference namedType)
		{
			string unmangledName = namedType.Name;

			return namedType.MangleName
				? MetadataHelpers.ComposeAritySuffixedMetadataName(unmangledName, namedType.GenericParameterCount)
				: unmangledName;
		}

		internal static string StrongName(IAssemblyReference assemblyReference)
		{
			var identity = assemblyReference.Identity;

			var pooled = PooledStringBuilder.GetInstance();
			StringBuilder sb = pooled.Builder;
			sb.Append(identity.Name);
			sb.AppendFormat(CultureInfo.InvariantCulture, ", Version={0}.{1}.{2}.{3}", identity.Version.Major, identity.Version.Minor, identity.Version.Build, identity.Version.Revision);
			if (!string.IsNullOrEmpty(identity.CultureName))
			{
				sb.AppendFormat(CultureInfo.InvariantCulture, ", Culture={0}", identity.CultureName);
			}
			else
			{
				sb.Append(", Culture=neutral");
			}

			sb.Append(", PublicKeyToken=");
			if (identity.PublicKeyToken.Length > 0)
			{
				foreach (byte b in identity.PublicKeyToken)
				{
					sb.Append(b.ToString("x2"));
				}
			}
			else
			{
				sb.Append("null");
			}

			if (identity.IsRetargetable)
			{
				sb.Append(", Retargetable=Yes");
			}

			//if (identity.ContentType == AssemblyContentType.WindowsRuntime)
			//{
			//	sb.Append(", ContentType=WindowsRuntime");
			//}
			//else
			//{
			//	Debug.Assert(identity.ContentType == AssemblyContentType.Default);
			//}

			return pooled.ToStringAndFree();
		}

		public static IUnitReference GetDefiningUnitReference(ITypeReference typeReference, EmitContext context)
		{
			INestedTypeReference nestedTypeReference = typeReference.AsNestedTypeReference;
			while (nestedTypeReference != null)
			{
				if (nestedTypeReference.AsGenericTypeInstanceReference != null)
				{
					return null;
				}

				typeReference = nestedTypeReference.GetContainingType(context);
				nestedTypeReference = typeReference.AsNestedTypeReference;
			}

			INamespaceTypeReference namespaceTypeReference = typeReference.AsNamespaceTypeReference;
			if (namespaceTypeReference == null)
			{
				return null;
			}

			Debug.Assert(namespaceTypeReference.AsGenericTypeInstanceReference == null);

			return namespaceTypeReference.GetUnit(context);
		}
	}
}
