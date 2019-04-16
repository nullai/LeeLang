using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IFlowCaptureReferenceOperation : IOperation
	{
		/// <summary>
		/// An id used to match references to the same intermediate result.
		/// </summary>
		CaptureId Id { get; }
	}
}
