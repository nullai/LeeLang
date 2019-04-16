using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IWithOperation : IOperation // https://github.com/dotnet/roslyn/issues/22005
	{
		/// <summary>
		/// Body of the with.
		/// </summary>
		IOperation Body { get; }
		/// <summary>
		/// Value to whose members leading-dot-qualified references within the with body bind.
		/// </summary>
		IOperation Value { get; }
	}
}
