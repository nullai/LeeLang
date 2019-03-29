using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public enum NullableContextOptions : byte
	{
		/// <summary>
		/// Nullable annotation and warning contexts are disabled.
		/// </summary>
		Disable,

		/// <summary>
		/// Nullable annotation and warning contexts are enabled.
		/// </summary>
		Enable,

		/// <summary>
		/// Nullable annotation context is enabled and the nullable warning context is safeonly.
		/// </summary>
		SafeOnly,

		/// <summary>
		/// Nullable annotation context is disabled and the nullable warning context is enabled.
		/// </summary>
		Warnings,

		/// <summary>
		/// Nullable annotation context is disabled and the nullable warning context is safeonly.
		/// </summary>
		SafeOnlyWarnings,
	}
}
