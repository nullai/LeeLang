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
			SourceFile file = new SourceFile(args[0]);
			try
			{
				file.Load();

				Parser par = new Parser(file);
				var s = par.ParseFile();
				for (int i = 0; i < s.Count; i++)
					Console.WriteLine(s[i].GetType().Name);
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}
