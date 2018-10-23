using System;
using System.Collections.Generic;

namespace LeeLang
{
	public class NamesExpression
	{
		public List<LocatedToken> names = new List<LocatedToken>();

		public bool IsEmpty => names.Count == 0;
		public bool IsSamepleName => names.Count == 1;

		public string this[int index]
		{
			get
			{
				return names[index].value;
			}
		}
		public void Add(LocatedToken val)
		{
			names.Add(val);
		}

		public NamesExpression()
		{

		}

		public NamesExpression(LocatedToken name)
		{
			names.Add(name);
		}

		public NamesExpression(string name)
		{
			names.Add(new LocatedToken(name, Location.Null));
		}
	}
}

