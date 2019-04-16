using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public struct ExportedType
	{
		/// <summary>
		/// The target type reference. 
		/// </summary>
		public readonly ITypeReference Type;

		/// <summary>
		/// True if this <see cref="ExportedType"/> represents a type forwarder definition,
		/// false if it represents a type from a linked netmodule.
		/// </summary>
		public readonly bool IsForwarder;

		/// <summary>
		/// If <see cref="Type"/> is a nested type defined in a linked netmodule, 
		/// the index of the <see cref="ExportedType"/> entry that represents the enclosing type.
		/// </summary>
		public readonly int ParentIndex;

		public ExportedType(ITypeReference type, int parentIndex, bool isForwarder)
		{
			Type = type;
			IsForwarder = isForwarder;
			ParentIndex = parentIndex;
		}
	}
}
