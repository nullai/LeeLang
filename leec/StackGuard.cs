using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace leec
{
	public static class StackGuard
	{
		public const int MaxUncheckedRecursionDepth = 20;

		/// <summary>
		///     Ensures that the remaining stack space is large enough to execute
		///     the average function.
		/// </summary>
		/// <param name="recursionDepth">how many times the calling function has recursed</param>
		/// <exception cref="InsufficientExecutionStackException">
		///     The available stack space is insufficient to execute
		///     the average function.
		/// </exception>
		public static void EnsureSufficientExecutionStack(int recursionDepth)
		{
			if (recursionDepth > MaxUncheckedRecursionDepth)
			{
				RuntimeHelpers.EnsureSufficientExecutionStack();
			}
		}
	}
}
