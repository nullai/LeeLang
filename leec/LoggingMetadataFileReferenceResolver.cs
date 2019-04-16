using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class LoggingMetadataFileReferenceResolver : MetadataReferenceResolver, IEquatable<LoggingMetadataFileReferenceResolver>
	{
		private readonly RelativePathResolver _pathResolver;
		private readonly Func<string, MetadataReferenceProperties, PortableExecutableReference> _provider;

		public LoggingMetadataFileReferenceResolver(RelativePathResolver pathResolver, Func<string, MetadataReferenceProperties, PortableExecutableReference> provider)
		{
			Debug.Assert(pathResolver != null);
			Debug.Assert(provider != null);

			_pathResolver = pathResolver;
			_provider = provider;
		}

		public override PortableExecutableReference[] ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
		{
			string fullPath = _pathResolver.ResolvePath(reference, baseFilePath);

			if (fullPath != null)
			{
				return new PortableExecutableReference[] { _provider(fullPath, properties) };
			}

			return new PortableExecutableReference[0];
		}

		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		public bool Equals(LoggingMetadataFileReferenceResolver other)
		{
			throw new NotImplementedException();
		}

		public override bool Equals(object other) => Equals(other as LoggingMetadataFileReferenceResolver);
	}
}
