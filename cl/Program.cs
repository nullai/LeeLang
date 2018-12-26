using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	class Program
	{
		static void Main(string[] args)
		{
			Parser par = new Parser(args[0]);
			try
			{
				var s = par.ParseFile();
				par.FlushError();
				if (s != null)
				{
					for (int i = 0; i < s.members.Count; i++)
						Console.WriteLine(s.members[i].GetType().Name);
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}
