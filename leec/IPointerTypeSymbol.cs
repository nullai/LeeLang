using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IPointerTypeSymbol : ITypeSymbol
	{
		/// <summary>
		/// Gets the type of the storage location that an instance of the pointer type points to.
		/// </summary>
		ITypeSymbol PointedAtType { get; }

		/// <summary>
		/// Custom modifiers associated with the pointer type, or an empty array if there are none.
		/// </summary>
		/// <remarks>
		/// Some managed languages may represent special information about the pointer type
		/// as a custom modifier on either the pointer type or the element type, or
		/// both.
		/// </remarks>
		CustomModifier[] CustomModifiers { get; }
	}
}
