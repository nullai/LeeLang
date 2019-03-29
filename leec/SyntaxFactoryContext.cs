using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public class SyntaxFactoryContext
	{
		public bool IsInAsync;

		public int QueryDepth;

		public bool IsInQuery
		{
			get { return QueryDepth > 0; }
		}
	}
}
