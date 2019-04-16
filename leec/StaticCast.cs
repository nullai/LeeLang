using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public static class StaticCast<T>
	{
		internal static T[] From<TDerived>(TDerived[] from) where TDerived : class, T
		{
			if (from == null)
				return null;

			T[] r = new T[from.Length];
			for (int i = 0; i < from.Length; i++)
				r[i] = from[i];

			return r;
		}
	}
}
