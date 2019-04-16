using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IArgumentOperation : IOperation
	{
		/// <summary>
		/// Kind of argument.
		/// </summary>
		ArgumentKind ArgumentKind { get; }
		/// <summary>
		/// Parameter the argument matches.
		/// </summary>
		IParameterSymbol Parameter { get; }
		/// <summary>
		/// Value supplied for the argument.
		/// </summary>
		IOperation Value { get; }
		/// <summary>
		/// Information of the conversion applied to the argument value passing it into the target method. Applicable only to VB Reference arguments.
		/// </summary>
		CommonConversion InConversion { get; }
		/// <summary>
		/// Information of the conversion applied to the argument value after the invocation. Applicable only to VB Reference arguments.
		/// </summary>
		CommonConversion OutConversion { get; }
	}
}
