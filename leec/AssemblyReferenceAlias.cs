using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public struct AssemblyReferenceAlias
	{
		/// <summary>
		/// An alias for the global namespace of the assembly.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The assembly reference.
		/// </summary>
		public readonly IAssemblyReference Assembly;

		internal AssemblyReferenceAlias(string name, IAssemblyReference assembly)
		{
			Debug.Assert(name != null);
			Debug.Assert(assembly != null);

			Name = name;
			Assembly = assembly;
		}
	}
}
