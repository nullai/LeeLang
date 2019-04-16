using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public enum PlaceholderKind
	{
		Unspecified = 0,
		SwitchOperationExpression = 1,
		ForToLoopBinaryOperatorLeftOperand = 2,
		ForToLoopBinaryOperatorRightOperand = 3,
		AggregationGroup = 4,
	}

	public interface IPlaceholderOperation : IOperation // https://github.com/dotnet/roslyn/issues/21294
	{
		PlaceholderKind PlaceholderKind { get; }
	}
}
