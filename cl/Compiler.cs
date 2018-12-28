using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class Compiler
	{
		public AssemblySpec Assembly;
		public List<string> InputFiles = new List<string>();
		public int error_count = 0;
		public int warn_count = 0;

		public Compiler(string name)
		{
			Assembly = new AssemblySpec(name);
		}

		public void AddFile(string path)
		{
			InputFiles.Add(path);
		}

		public void OutputError(string text)
		{
			++error_count;
			Console.WriteLine(text);
		}

		public void OutputWarning(string text)
		{
			++warn_count;
			Console.WriteLine(text);
		}

		public bool Build()
		{
			if (InputFiles.Count == 0)
			{
				OutputError("没有可用的源文件");
				return false;
			}

			Parser par = new Parser(this);
			List<FileStatement> fs = new List<FileStatement>();
			for (int i = 0; i < InputFiles.Count; i++)
			{
				var s = par.ParseFile(InputFiles[i]);
				if (s != null)
					Program.PrintAst(s, 0);
				fs.Add(s);
			}

			if (error_count > 0)
				return false;

			return true;
		}
	}

	public class ResolveContext
	{
		public Compiler complier;
		public NamespaceSpec scope;
	}
}
