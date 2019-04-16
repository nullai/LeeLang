using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class MetadataConstant : IMetadataExpression
	{
		public ITypeReference Type { get; }
		public object Value { get; }

		public MetadataConstant(ITypeReference type, object value)
		{
			Debug.Assert(type != null);
			AssertValidConstant(value);

			Type = type;
			Value = value;
		}

		void IMetadataExpression.Dispatch(MetadataVisitor visitor)
		{
			visitor.Visit(this);
		}

		[Conditional("DEBUG")]
		internal static void AssertValidConstant(object value)
		{
			Debug.Assert(value == null || value is string || value is DateTime || value is decimal || value.GetType().IsEnum || (value.GetType().IsPrimitive && !(value is IntPtr) && !(value is UIntPtr)));
		}
	}
}
