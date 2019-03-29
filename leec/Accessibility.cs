using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public enum Accessibility
	{
		/// <summary>
		/// No accessibility specified.
		/// </summary>
		NotApplicable = 0,

		// DO NOT CHANGE ORDER OF THESE ENUM VALUES
		Private = 1,

		ProtectedAndInternal = 2,
		ProtectedAndFriend = ProtectedAndInternal,

		Protected = 3,

		Internal = 4,
		Friend = Internal,

		ProtectedOrInternal = 5,
		ProtectedOrFriend = ProtectedOrInternal,

		Public = 6
	}
}
