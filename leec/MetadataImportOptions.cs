using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public enum MetadataImportOptions : byte
	{
		/// <summary>
		/// Only import public and protected symbols.
		/// </summary>
		Public = 0,

		/// <summary>
		/// Import public, protected and public symbols.
		/// </summary>
		Internal = 1,

		/// <summary>
		/// Import all symbols.
		/// </summary>
		All = 2,
	}

	public static partial class EnumBounds
	{
		public static bool IsValid(this MetadataImportOptions value)
		{
			return value >= MetadataImportOptions.Public && value <= MetadataImportOptions.All;
		}
	}
}
