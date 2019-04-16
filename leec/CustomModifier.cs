using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public abstract class CustomModifier : ICustomModifier
	{
		/// <summary>
		/// If true, a language may use the modified storage location without 
		/// being aware of the meaning of the modification, modopt vs. modreq. 
		/// </summary>
		public abstract bool IsOptional { get; }

		/// <summary>
		/// A type used as a tag that indicates which type of modification applies.
		/// </summary>
		public abstract INamedTypeSymbol Modifier { get; }

		#region ICustomModifier

		bool ICustomModifier.IsOptional
		{
			get
			{
				return this.IsOptional;
			}
		}

		ITypeReference ICustomModifier.GetModifier(EmitContext context)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
