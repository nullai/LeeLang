using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ILockOperation : IOperation
	{
		/// <summary>
		/// Operation producing a value to be locked.
		/// </summary>
		IOperation LockedValue { get; }
		/// <summary>
		/// Body of the lock, to be executed while holding the lock.
		/// </summary>
		IOperation Body { get; }
	}
}
