using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LeeLang
{
	public class Compiler
	{
		public AssemblyDefinition Assembly;
		public List<string> InputFiles = new List<string>();
		public CompilerContext Context;
		public GlobalNamespace Global;
		public AstProject AstRoot;

		public Compiler(string name)
		{
			Assembly = new AssemblyDefinition(name, name + ".exe");
			Context =  new CompilerContext(this);
			Global = new GlobalNamespace();
			AstRoot = new AstProject(name + ".exe");
		}

		public void AddFile(string path)
		{
			InputFiles.Add(path);
		}

		public bool Build()
		{
			if (!DoGrammar())
				return false;

			StringBuilder sb = new StringBuilder();
			AstRoot.Print(sb, 0);
			Console.WriteLine(sb.ToString());

			//Program.PrintAst(file_statement, 0);

			if (!DoDefine())
				return false;

			//if (!DoResolveUsing())
			//	return false;

			//if (!DoResolveMember())
			//	return false;

			//if (!DoResolve())
			//	return false;

			//if (!DoCodeGen())
			//	return false;

			return true;
		}

		private bool DoGrammar()
		{
			if (InputFiles.Count == 0)
			{
				Context.Error_NoSouceFile();
				return false;
			}

			Parser par = new Parser(this);
			for (int i = 0; i < InputFiles.Count; i++)
			{
				par.ParseFile(InputFiles[i]);
			}

			return Context.error_count == 0;
		}
		
		private bool DoDefine()
		{
			//Global.Define(Context);
			return Context.error_count == 0;
		}
	}
}
