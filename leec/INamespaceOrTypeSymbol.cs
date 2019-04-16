﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface INamespaceOrTypeSymbol : ISymbol
	{
		/// <summary>
		/// Get all the members of this symbol.
		/// </summary>
		/// <returns>An ImmutableArray containing all the members of this symbol. If this symbol has no members,
		/// returns an empty ImmutableArray. Never returns Null.</returns>
		ISymbol[] GetMembers();

		/// <summary>
		/// Get all the members of this symbol that have a particular name.
		/// </summary>
		/// <returns>An ImmutableArray containing all the members of this symbol with the given name. If there are
		/// no members with this name, returns an empty ImmutableArray. Never returns Null.</returns>
		ISymbol[] GetMembers(string name);

		/// <summary>
		/// Get all the members of this symbol that are types.
		/// </summary>
		/// <returns>An ImmutableArray containing all the types that are members of this symbol. If this symbol has no type members,
		/// returns an empty ImmutableArray. Never returns null.</returns>
		INamedTypeSymbol[] GetTypeMembers();

		/// <summary>
		/// Get all the members of this symbol that are types that have a particular name, of any arity.
		/// </summary>
		/// <returns>An ImmutableArray containing all the types that are members of this symbol with the given name.
		/// If this symbol has no type members with this name,
		/// returns an empty ImmutableArray. Never returns null.</returns>
		INamedTypeSymbol[] GetTypeMembers(string name);

		/// <summary>
		/// Get all the members of this symbol that are types that have a particular name and arity
		/// </summary>
		/// <returns>An ImmutableArray containing all the types that are members of this symbol with the given name and arity.
		/// If this symbol has no type members with this name and arity,
		/// returns an empty ImmutableArray. Never returns null.</returns>
		INamedTypeSymbol[] GetTypeMembers(string name, int arity);

		/// <summary>
		/// Returns true if this symbol is a namespace. If it is not a namespace, it must be a type.
		/// </summary>
		bool IsNamespace { get; }

		/// <summary>
		/// Returns true if this symbols is a type. If it is not a type, it must be a namespace.
		/// </summary>
		bool IsType { get; }
	}
}
