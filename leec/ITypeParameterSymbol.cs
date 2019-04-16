using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public enum VarianceKind : short
	{
		/// <summary>
		/// Invariant.
		/// </summary>
		None = 0,

		/// <summary>
		/// Covariant (<c>out</c>).
		/// </summary>
		Out = 1,

		/// <summary>
		/// Contravariant (<c>in</c>).
		/// </summary>
		In = 2,
	}

	public enum TypeParameterKind
	{
		/// <summary>
		/// Type parameter of a named type. For example: <c>T</c> in <c><![CDATA[List<T>]]></c>.
		/// </summary>
		Type = 0,

		/// <summary>
		/// Type parameter of a method. For example: <c>T</c> in <c><![CDATA[void M<T>()]]></c>.
		/// </summary>
		Method = 1,

		/// <summary>
		/// Type parameter in a <c>cref</c> attribute in XML documentation comments. For example: <c>T</c> in <c><![CDATA[<see cref="List{T}"/>]]></c>.
		/// </summary>
		Cref = 2,
	}

	public interface ITypeParameterSymbol : ITypeSymbol
	{
		/// <summary>
		/// The ordinal position of the type parameter in the parameter list which declares
		/// it. The first type parameter has ordinal zero.
		/// </summary>
		int Ordinal { get; }

		/// <summary>
		/// The variance annotation, if any, of the type parameter declaration. Type parameters may be 
		/// declared as covariant (<c>out</c>), contravariant (<c>in</c>), or neither.
		/// </summary>
		VarianceKind Variance { get; }

		/// <summary>
		/// The type parameter kind of this type parameter.
		/// </summary>
		TypeParameterKind TypeParameterKind { get; }

		/// <summary>
		/// The method that declares the type parameter, or null.
		/// </summary>
		IMethodSymbol DeclaringMethod { get; }

		/// <summary>
		/// The type that declares the type parameter, or null.
		/// </summary>
		INamedTypeSymbol DeclaringType { get; }

		/// <summary>
		/// True if the reference type constraint (<c>class</c>) was specified for the type parameter.
		/// </summary>
		bool HasReferenceTypeConstraint { get; }

		/// <summary>
		/// True if the value type constraint (<c>struct</c>) was specified for the type parameter.
		/// </summary>
		bool HasValueTypeConstraint { get; }

		/// <summary>
		/// True if the value type constraint (<c>unmanaged</c>) was specified for the type parameter.
		/// </summary>
		bool HasUnmanagedTypeConstraint { get; }

		/// <summary>
		/// True if the parameterless constructor constraint (<c>new()</c>) was specified for the type parameter.
		/// </summary>
		bool HasConstructorConstraint { get; }

		/// <summary>
		/// The types that were directly specified as constraints on the type parameter.
		/// </summary>
		ITypeSymbol[] ConstraintTypes { get; }

		/// <summary>
		/// Get the original definition of this type symbol. If this symbol is derived from another
		/// symbol by (say) type substitution, this gets the original symbol, as it was defined in
		/// source or metadata.
		/// </summary>
		new ITypeParameterSymbol OriginalDefinition { get; }

		/// <summary>
		/// If this is a type parameter of a reduced extension method, gets the type parameter definition that
		/// this type parameter was reduced from. Otherwise, returns Nothing.
		/// </summary>
		ITypeParameterSymbol ReducedFrom { get; }
	}
}
