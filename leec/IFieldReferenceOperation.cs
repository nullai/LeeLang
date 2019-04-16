using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IFieldReferenceOperation : IMemberReferenceOperation
	{
		/// <summary>
		/// Referenced field.
		/// </summary>
		IFieldSymbol Field { get; }
		/// <summary>
		/// If the field reference is also where the field was declared.
		/// </summary>
		/// <remarks>
		/// This is only ever true in CSharp scripts, where a top-level statement creates a new variable
		/// in a reference, such as an out variable declaration or a deconstruction declaration.
		/// </remarks>
		bool IsDeclaration { get; }
	}
}
