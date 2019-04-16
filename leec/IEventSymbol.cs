using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IEventSymbol : ISymbol
	{
		/// <summary>
		/// The type of the event. 
		/// </summary>
		ITypeSymbol Type { get; }

		/// <summary>
		/// Returns true if the event is a WinRT type event.
		/// </summary>
		bool IsWindowsRuntimeEvent { get; }

		/// <summary>
		/// The 'add' accessor of the event.  Null only in error scenarios.
		/// </summary>
		IMethodSymbol AddMethod { get; }

		/// <summary>
		/// The 'remove' accessor of the event.  Null only in error scenarios.
		/// </summary>
		IMethodSymbol RemoveMethod { get; }

		/// <summary>
		/// The 'raise' accessor of the event.  Null if there is no raise method.
		/// </summary>
		IMethodSymbol RaiseMethod { get; }

		/// <summary>
		/// The original definition of the event. If the event is constructed from another
		/// symbol by type substitution, OriginalDefinition gets the original symbol, as it was 
		/// defined in source or metadata.
		/// </summary>
		new IEventSymbol OriginalDefinition { get; }

		/// <summary>
		/// Returns the overridden event, or null.
		/// </summary>
		IEventSymbol OverriddenEvent { get; }

		/// <summary>
		/// Returns interface properties explicitly implemented by this event.
		/// </summary>
		/// <remarks>
		/// Properties imported from metadata can explicitly implement more than one event.
		/// </remarks>
		IEventSymbol[] ExplicitInterfaceImplementations { get; }
	}
}
