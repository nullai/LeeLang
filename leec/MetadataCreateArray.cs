using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class MetadataCreateArray : IMetadataExpression
	{
		public IArrayTypeReference ArrayType { get; }
		public ITypeReference ElementType { get; }
		public IMetadataExpression[] Elements { get; }

		public MetadataCreateArray(IArrayTypeReference arrayType, ITypeReference elementType, IMetadataExpression[] initializers)
		{
			ArrayType = arrayType;
			ElementType = elementType;
			Elements = initializers;
		}

		ITypeReference IMetadataExpression.Type => ArrayType;
		void IMetadataExpression.Dispatch(MetadataVisitor visitor) => visitor.Visit(this);
	}
}
