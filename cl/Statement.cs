using System;
using System.Collections.Generic;

namespace LeeLang
{
	public abstract class Statement
	{

	}

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
