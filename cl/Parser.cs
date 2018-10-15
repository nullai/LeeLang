using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace leelang
{
	public class Parser
	{
		public Tokenizer tokener;
		public LocatedToken cur;

		public Parser(Tokenizer t)
		{
			tokener = t;
			tokener.Fixup();
			Next();
		}

		public void Next()
		{
			cur = tokener.Next();
		}
	}
}
