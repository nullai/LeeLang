using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	class Program
	{
		static void Main(string[] args)
		{
			ParseOptions options = new ParseOptions(new string[] { "DEBUG" });
			LeeSyntaxTree.ParseText("using System;", options);
		}
	}
}
