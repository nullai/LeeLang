using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public struct LocalScope
	{
		/// <summary>
		/// The offset of the first operation in the scope.
		/// </summary>
		public readonly int StartOffset;

		/// <summary>
		/// The offset of the first operation outside of the scope, or the method body length.
		/// </summary>
		public readonly int EndOffset;

		private readonly ILocalDefinition[] _constants;
		private readonly ILocalDefinition[] _locals;

		internal LocalScope(int offset, int endOffset, ILocalDefinition[] constants, ILocalDefinition[] locals)
		{
			Debug.Assert(!locals.Any(l => l.Name == null));
			Debug.Assert(!constants.Any(c => c.Name == null));
			Debug.Assert(offset >= 0);
			Debug.Assert(endOffset > offset);

			StartOffset = offset;
			EndOffset = endOffset;

			_constants = constants;
			_locals = locals;
		}

		public int Length => EndOffset - StartOffset;

		/// <summary>
		/// Returns zero or more local constant definitions that are local to the given scope.
		/// </summary>
		public ILocalDefinition[] Constants => _constants ?? new ILocalDefinition[0];

		/// <summary>
		/// Returns zero or more local variable definitions that are local to the given scope.
		/// </summary>
		public ILocalDefinition[] Variables => _locals ?? new ILocalDefinition[0];
	}
}
