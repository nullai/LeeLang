using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IAssemblyReference : IModuleReference
	{
		AssemblyIdentity Identity { get; }
		Version AssemblyVersionPattern { get; }
	}

	public struct DefinitionWithLocation
	{
		public readonly IDefinition Definition;
		public readonly int StartLine;
		public readonly int StartColumn;
		public readonly int EndLine;
		public readonly int EndColumn;

		public DefinitionWithLocation(IDefinition definition,
			int startLine, int startColumn, int endLine, int endColumn)
		{
			Debug.Assert(startLine >= 0);
			Debug.Assert(startColumn >= 0);
			Debug.Assert(endLine >= 0);
			Debug.Assert(endColumn >= 0);

			Definition = definition;
			StartLine = startLine;
			StartColumn = startColumn;
			EndLine = endLine;
			EndColumn = endColumn;
		}

		private string GetDebuggerDisplay()
			=> $"{Definition} => ({StartLine},{StartColumn}) - ({EndLine}, {EndColumn})";
	}

	/// <summary>
	/// A reference to a .NET module.
	/// </summary>
	public interface IModuleReference : IUnitReference
	{
		/// <summary>
		/// The Assembly that contains this module. May be null if the module is not part of an assembly.
		/// </summary>
		IAssemblyReference GetContainingAssembly(EmitContext context);
	}

	/// <summary>
	/// A unit of metadata stored as a single artifact and potentially produced and revised independently from other units.
	/// Examples of units include .NET assemblies and modules, as well C++ object files and compiled headers.
	/// </summary>
	public interface IUnit : IUnitReference, IDefinition
	{
	}

	/// <summary>
	/// A reference to a instance of <see cref="IUnit"/>.
	/// </summary>
	public interface IUnitReference : IReference, INamedEntity
	{
	}
}
