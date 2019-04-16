using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public enum SpeculativeBindingOption
	{
		/// <summary>
		/// Binds the given expression using the normal expression binding rules
		/// that would occur during normal binding of expressions.
		/// </summary>
		BindAsExpression = 0,

		/// <summary>
		/// Binds the given expression as a type or namespace only. If this option
		/// is selected, then the given expression must derive from TypeSyntax.
		/// </summary>
		BindAsTypeOrNamespace = 1
	}
}
