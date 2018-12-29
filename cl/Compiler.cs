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
		private List<FileStatement> file_statement;

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
			if (!DoGrammar())
				return false;

			//Program.PrintAst(file_statement, 0);

			if (!DoResolveType())
				return false;

			if (!DoResolveUsing())
				return false;

			if (!DoResolve())
				return false;

			Program.PrintAst(Assembly, 0);
			return true;
		}

		private bool DoGrammar()
		{
			if (InputFiles.Count == 0)
			{
				OutputError("没有可用的源文件");
				return false;
			}

			Parser par = new Parser(this);
			file_statement = new List<FileStatement>();
			for (int i = 0; i < InputFiles.Count; i++)
			{
				var s = par.ParseFile(InputFiles[i]);
				file_statement.Add(s);
			}

			return error_count == 0;
		}

		private bool DoResolveType()
		{
			ResolveContext ctx = new ResolveContext(this);
			ctx.scope = Assembly.CurrentModule;
			for (int i = 0; i < file_statement.Count; i++)
			{
				file_statement[i].ResolveType(ctx);
			}
			return error_count == 0;
		}
		private bool DoResolveUsing()
		{
			ResolveContext ctx = new ResolveContext(this);
			ctx.scope = Assembly.CurrentModule;
			for (int i = 0; i < file_statement.Count; i++)
			{
				file_statement[i].ResolveUsing(ctx);
			}
			return error_count == 0;
		}
		private bool DoResolve()
		{
			ResolveContext ctx = new ResolveContext(this);
			ctx.scope = Assembly.CurrentModule;
			for (int i = 0; i < file_statement.Count; i++)
			{
				file_statement[i].Resolve(ctx);
			}
			return error_count == 0;
		}
	}

	public class ResolveContext
	{
		public Compiler complier;
		public NamespaceSpec scope;

		public ResolveContext(Compiler complier)
		{
			this.complier = complier;
		}

		public bool CheckAccess(MemberSpec member)
		{
			return true;
		}
	}
}
