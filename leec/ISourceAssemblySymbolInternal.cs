using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ISourceAssemblySymbol : IAssemblySymbol
	{
		Compilation Compilation { get; }
	}

	public interface ISourceAssemblySymbolInternal : ISourceAssemblySymbol
	{
		AssemblyFlags AssemblyFlags { get; }

		/// <summary>
		/// The contents of the AssemblySignatureKeyAttribute
		/// </summary>
		string SignatureKey { get; }

		AssemblyHashAlgorithm HashAlgorithm { get; }

		Version AssemblyVersionPattern { get; }

		bool InternalsAreVisible { get; }
	}
}
