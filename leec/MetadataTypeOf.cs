using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class MetadataTypeOf : IMetadataExpression
	{
		private readonly ITypeReference _typeToGet;
		private readonly ITypeReference _systemType;

		public MetadataTypeOf(ITypeReference typeToGet, ITypeReference systemType)
		{
			_typeToGet = typeToGet;
			_systemType = systemType;
		}

		/// <summary>
		/// The type that will be represented by the System.Type instance.
		/// </summary>
		public ITypeReference TypeToGet
		{
			get
			{
				return _typeToGet;
			}
		}

		void IMetadataExpression.Dispatch(MetadataVisitor visitor)
		{
			visitor.Visit(this);
		}

		ITypeReference IMetadataExpression.Type
		{
			get { return _systemType; }
		}
	}
}
