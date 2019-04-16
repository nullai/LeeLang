using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public abstract class CommonAnonymousTypeManager
	{
		/// <summary>
		/// We should not see new anonymous types from source after we finished emit phase. 
		/// If this field is true, the collection is sealed; in DEBUG it also is used to check the assertion.
		/// </summary>
		private ThreeState _templatesSealed = ThreeState.False;

		/// <summary>
		/// Collection of anonymous type templates is sealed 
		/// </summary>
		public bool AreTemplatesSealed
		{
			get { return _templatesSealed == ThreeState.True; }
		}

		protected void SealTemplates()
		{
			_templatesSealed = ThreeState.True;
		}
	}
}
