using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public struct UsedNamespaceOrType : IEquatable<UsedNamespaceOrType>
	{
		public readonly string AliasOpt;
		public readonly IAssemblyReference TargetAssemblyOpt;
		public readonly INamespace TargetNamespaceOpt;
		public readonly ITypeReference TargetTypeOpt;
		public readonly string TargetXmlNamespaceOpt;

		private UsedNamespaceOrType(
			string alias = null,
			IAssemblyReference targetAssembly = null,
			INamespace targetNamespace = null,
			ITypeReference targetType = null,
			string targetXmlNamespace = null)
		{
			AliasOpt = alias;
			TargetAssemblyOpt = targetAssembly;
			TargetNamespaceOpt = targetNamespace;
			TargetTypeOpt = targetType;
			TargetXmlNamespaceOpt = targetXmlNamespace;
		}

		internal static UsedNamespaceOrType CreateType(ITypeReference type, string aliasOpt = null)
		{
			Debug.Assert(type != null);
			return new UsedNamespaceOrType(alias: aliasOpt, targetType: type);
		}

		internal static UsedNamespaceOrType CreateNamespace(INamespace @namespace, IAssemblyReference assemblyOpt = null, string aliasOpt = null)
		{
			Debug.Assert(@namespace != null);
			return new UsedNamespaceOrType(alias: aliasOpt, targetAssembly: assemblyOpt, targetNamespace: @namespace);
		}

		internal static UsedNamespaceOrType CreateExternAlias(string alias)
		{
			Debug.Assert(alias != null);
			return new UsedNamespaceOrType(alias: alias);
		}

		internal static UsedNamespaceOrType CreateXmlNamespace(string prefix, string xmlNamespace)
		{
			Debug.Assert(xmlNamespace != null);
			Debug.Assert(prefix != null);
			return new UsedNamespaceOrType(alias: prefix, targetXmlNamespace: xmlNamespace);
		}

		public override bool Equals(object obj)
		{
			return obj is UsedNamespaceOrType other && Equals(other);
		}

		public bool Equals(UsedNamespaceOrType other)
		{
			return AliasOpt == other.AliasOpt
				&& object.Equals(TargetAssemblyOpt, other.TargetAssemblyOpt)
				&& object.Equals(TargetNamespaceOpt, other.TargetNamespaceOpt)
				&& object.Equals(TargetTypeOpt, other.TargetTypeOpt)
				&& TargetXmlNamespaceOpt == other.TargetXmlNamespaceOpt;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(AliasOpt,
				   Hash.Combine((object)TargetAssemblyOpt,
				   Hash.Combine((object)TargetNamespaceOpt,
				   Hash.Combine((object)TargetTypeOpt,
				   Hash.Combine(TargetXmlNamespaceOpt, 0)))));
		}
	}
}
