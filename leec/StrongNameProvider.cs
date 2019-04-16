using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public abstract class StrongNameProvider
	{
		protected StrongNameProvider()
		{
		}

		public abstract override int GetHashCode();
		public override abstract bool Equals(object other);

		public abstract StrongNameFileSystem FileSystem { get; }
	}
}
