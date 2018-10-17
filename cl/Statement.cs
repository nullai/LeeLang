using System;
using System.Collections.Generic;

namespace LeeLang
{
	public abstract class Statement
	{

	}

	public class ErrorStatement : Statement
	{
	}

	public class EmptyStatement : Statement
	{
	}

	public class FileStatement : Statement
	{
		public string file_name;
		public List<Statement> members = new List<Statement>();

		public FileStatement(string file_name)
		{
			this.file_name = file_name;
		}
	}

	public class UsingStatement : Statement
	{
		public string name;
		public NamesExpression value;

		public UsingStatement(string name, NamesExpression value)
		{
			this.name = name;
			this.value = value;
		}

	}

	public class NamespaceStatement : Statement
	{
		public NamesExpression name;
		public List<Statement> members = new List<Statement>();

		public NamespaceStatement(NamesExpression name)
		{
			this.name = name;
		}

	}
}

