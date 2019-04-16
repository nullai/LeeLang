using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IAssemblySymbolInternal : IAssemblySymbol
	{
		Version AssemblyVersionPattern { get; }
	}
}
