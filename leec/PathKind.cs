using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public enum PathKind
	{
		/// <summary>
		/// Null or empty.
		/// </summary>
		Empty,

		/// <summary>
		/// "file"
		/// </summary>
		Relative,

		/// <summary>
		/// ".\file"
		/// </summary>
		RelativeToCurrentDirectory,

		/// <summary>
		/// "..\file"
		/// </summary>
		RelativeToCurrentParent,

		/// <summary>
		/// "\dir\file"
		/// </summary>
		RelativeToCurrentRoot,

		/// <summary>
		/// "C:dir\file"
		/// </summary>
		RelativeToDriveDirectory,

		/// <summary>
		/// "C:\file" or "\\machine" (UNC).
		/// </summary>
		Absolute,
	}
}
