using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	/// <summary>
	/// The kind of metadata a PE file image contains.
	/// </summary>
	public enum MetadataImageKind : byte
	{
		/// <summary>
		/// The PE file is an assembly.
		/// </summary>
		Assembly = 0,

		/// <summary>
		/// The PE file is a module.
		/// </summary>
		Module = 1
	}

	public static partial class EnumBounds
	{
		public static bool IsValid(this MetadataImageKind kind)
		{
			return kind >= MetadataImageKind.Assembly && kind <= MetadataImageKind.Module;
		}
	}
}
