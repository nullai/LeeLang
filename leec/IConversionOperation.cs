using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IConversionOperation : IOperation
	{
		/// <summary>
		/// Value to be converted.
		/// </summary>
		IOperation Operand { get; }
		/// <summary>
		/// Operator method used by the operation, null if the operation does not use an operator method.
		/// </summary>
		IMethodSymbol OperatorMethod { get; }

#pragma warning disable CA1200 // Avoid using cref tags with a prefix
		/// <summary>
		/// Gets the underlying common conversion information.
		/// </summary>
		/// <remarks>
		/// If you need conversion information that is language specific, use either
		/// <see cref="T:Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetConversion(IConversionOperation)"/> or
		/// <see cref="T:Microsoft.CodeAnalysis.VisualBasic.VisualBasicExtensions.GetConversion(IConversionOperation)"/>.
		/// </remarks>
#pragma warning restore CA1200 // Avoid using cref tags with a prefix
		CommonConversion Conversion { get; }
		/// <summary>
		/// False if the conversion will fail with a <see cref="InvalidCastException"/> at runtime if the cast fails. This is true for C#'s
		/// <c>as</c> operator and for VB's <c>TryCast</c> operator.
		/// </summary>
		bool IsTryCast { get; }
		/// <summary>
		/// True if the conversion can fail at runtime with an overflow exception. This corresponds to C# checked and unchecked blocks.
		/// </summary>
		bool IsChecked { get; }
	}
}
