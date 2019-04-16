using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class ExistingReferencesResolver : MetadataReferenceResolver, IEquatable<ExistingReferencesResolver>
	{
		private readonly MetadataReferenceResolver _resolver;
		private readonly MetadataReference[] _availableReferences;
		private readonly Lazy<HashSet<AssemblyIdentity>> _lazyAvailableReferences;

		public ExistingReferencesResolver(MetadataReferenceResolver resolver, MetadataReference[] availableReferences)
		{
			Debug.Assert(resolver != null);
			Debug.Assert(availableReferences != null);

			_resolver = resolver;
			_availableReferences = availableReferences;

			// Delay reading assembly identities until they are actually needed (only when #r is encountered).
			_lazyAvailableReferences = new Lazy<HashSet<AssemblyIdentity>>(() => new HashSet<AssemblyIdentity>(
				from reference in _availableReferences
				let identity = TryGetIdentity(reference)
				where identity != null
				select identity));
		}

		public override PortableExecutableReference[] ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
		{
			var resolvedReferences = _resolver.ResolveReference(reference, baseFilePath, properties);
			return resolvedReferences.Where(r => _lazyAvailableReferences.Value.Contains(TryGetIdentity(r))).ToArray();
		}

		private static AssemblyIdentity TryGetIdentity(MetadataReference metadataReference)
		{
			var peReference = metadataReference as PortableExecutableReference;
			if (peReference == null || peReference.Properties.Kind != MetadataImageKind.Assembly)
			{
				return null;
			}

			try
			{
				return ((AssemblyMetadata)peReference.GetMetadataNoCopy()).GetAssembly().Identity;
			}
			catch (Exception e) when (e is BadImageFormatException || e is IOException)
			{
				// ignore, metadata reading errors are reported by the compiler for the existing references
				return null;
			}
		}

		public override int GetHashCode()
		{
			return _resolver.GetHashCode();
		}

		public bool Equals(ExistingReferencesResolver other)
		{
			return _resolver.Equals(other._resolver) &&
				   _availableReferences.SequenceEqual(other._availableReferences);
		}

		public override bool Equals(object other) => Equals(other as ExistingReferencesResolver);
	}
}
