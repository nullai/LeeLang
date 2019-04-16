using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IFlowCaptureOperation : IOperation
	{
		/// <summary>
		/// An id used to match references to the same intermediate result.
		/// </summary>
		CaptureId Id { get; }

		/// <summary>
		/// Value to be captured.
		/// </summary>
		IOperation Value { get; }
	}
}
