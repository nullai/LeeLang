using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public struct UnifiedAssembly<TAssemblySymbol>
		where TAssemblySymbol : class, IAssemblySymbol
	{
		/// <summary>
		/// Original reference that was unified to the identity of the <see cref="TargetAssembly"/>.
		/// </summary>
		public readonly AssemblyIdentity OriginalReference;

		public readonly TAssemblySymbol TargetAssembly;

		public UnifiedAssembly(TAssemblySymbol targetAssembly, AssemblyIdentity originalReference)
		{
			Debug.Assert(originalReference != null);
			Debug.Assert(targetAssembly != null);

			this.OriginalReference = originalReference;
			this.TargetAssembly = targetAssembly;
		}
	}
}
