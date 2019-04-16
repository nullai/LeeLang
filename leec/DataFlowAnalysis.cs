using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public abstract class DataFlowAnalysis
	{
		/// <summary>
		/// The set of local variables that are declared within a region. Note
		/// that the region must be bounded by a method's body or a field's initializer, so
		/// parameter symbols are never included in the result.
		/// </summary>
		public abstract ISymbol[] VariablesDeclared { get; }

		/// <summary>
		/// The set of local variables which are assigned a value outside a region
		/// that may be used inside the region.
		/// </summary>
		public abstract ISymbol[] DataFlowsIn { get; }

		/// <summary>
		/// The set of local variables which are assigned a value inside a region
		/// that may be used outside the region.
		/// </summary>
		public abstract ISymbol[] DataFlowsOut { get; }

		/// <summary>
		/// The set of local variables for which a value is always assigned inside
		/// a region.
		/// </summary>
		public abstract ISymbol[] AlwaysAssigned { get; }

		/// <summary>
		/// The set of local variables that are read inside a region.
		/// </summary>
		public abstract ISymbol[] ReadInside { get; }

		/// <summary>
		/// The set of local variables that are written inside a region.
		/// </summary>
		public abstract ISymbol[] WrittenInside { get; }

		/// <summary>
		/// The set of the local variables that are read outside a region.
		/// </summary>
		public abstract ISymbol[] ReadOutside { get; }

		/// <summary>
		/// The set of local variables that are written outside a region.
		/// </summary>
		public abstract ISymbol[] WrittenOutside { get; }

		/// <summary>
		/// The set of the local variables that have been referenced in anonymous
		/// functions within a region and therefore must be moved to a field of a frame class.
		/// </summary>
		public abstract ISymbol[] Captured { get; }

		/// <summary>
		/// The set of variables that are captured inside a region.
		/// </summary>
		public abstract ISymbol[] CapturedInside { get; }

		/// <summary>
		/// The set of variables that are captured outside a region.
		/// </summary>
		public abstract ISymbol[] CapturedOutside { get; }

		/// <summary>
		/// The set of non-constant local variables and parameters that have had their
		/// address (or the address of one of their fields) taken.
		/// </summary>
		public abstract ISymbol[] UnsafeAddressTaken { get; }

		/// <summary>
		/// Returns true iff analysis was successful.  Analysis can fail if the region does not
		/// properly span a single expression, a single statement, or a contiguous series of
		/// statements within the enclosing block.
		/// </summary>
		public abstract bool Succeeded { get; }
	}
}
