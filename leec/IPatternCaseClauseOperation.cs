using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IPatternCaseClauseOperation : ICaseClauseOperation
	{
		/// <summary>
		/// Label associated with the case clause.
		/// https://github.com/dotnet/roslyn/issues/27602: Similar property was added to the base interface, consider if we can remove this one.
		/// </summary>
		new ILabelSymbol Label { get; }

		/// <summary>
		/// Pattern associated with case clause.
		/// </summary>
		IPatternOperation Pattern { get; }

		/// <summary>
		/// Guard associated with the pattern case clause.
		/// </summary>
		IOperation Guard { get; }
	}
}
