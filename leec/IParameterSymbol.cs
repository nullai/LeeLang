﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IParameterSymbol : ISymbol
	{
		/// <summary>
		/// Whether the parameter passed by value or by reference.
		/// </summary>
		RefKind RefKind { get; }

		/// <summary>
		/// Returns true if the parameter was declared as a parameter array. 
		/// </summary>
		bool IsParams { get; }

		/// <summary>
		/// Returns true if the parameter is optional.
		/// </summary>
		bool IsOptional { get; }

		/// <summary>
		/// Returns true if the parameter is the hidden 'this' ('Me' in Visual Basic) parameter.
		/// </summary>
		bool IsThis { get; }

		/// <summary>
		/// Gets the type of the parameter.
		/// </summary>
		ITypeSymbol Type { get; }

		/// <summary>
		/// Custom modifiers associated with the parameter type, or an empty array if there are none.
		/// </summary>
		CustomModifier[] CustomModifiers { get; }

		/// <summary>
		/// Custom modifiers associated with the ref modifier, or an empty array if there are none.
		/// </summary>
		CustomModifier[] RefCustomModifiers { get; }

		/// <summary>
		/// Gets the ordinal position of the parameter. The first parameter has ordinal zero.
		/// The 'this' parameter ('Me' in Visual Basic) has ordinal -1.
		/// </summary>
		int Ordinal { get; }

		/// <summary>
		/// Returns true if the parameter specifies a default value to be passed
		/// when no value is provided as an argument to a call. The default value
		/// can be obtained with the DefaultValue property.
		/// </summary>
		bool HasExplicitDefaultValue { get; }

		/// <summary>
		/// Returns the default value of the parameter. 
		/// </summary>
		/// <remarks>
		/// Returns null if the parameter type is a struct and the default value of the parameter
		/// is the default value of the struct type.
		/// </remarks>
		/// <exception cref="System.InvalidOperationException">The parameter has no default value.</exception>
		object ExplicitDefaultValue { get; }

		/// <summary>
		/// Get the original definition of this symbol. If this symbol is derived from another
		/// symbol by (say) type substitution, this gets the original symbol, as it was defined in
		/// source or metadata.
		/// </summary>
		new IParameterSymbol OriginalDefinition { get; }
	}
}
