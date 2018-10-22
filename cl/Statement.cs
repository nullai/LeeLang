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

	public class ClassStatement : Statement
	{
		public Attributes attr;
		public LocatedToken key;
		public NamesExpression name;
		public List<NamesExpression> base_type;
		public List<Statement> members = new List<Statement>();

		public ClassStatement(Attributes attr, LocatedToken key, NamesExpression name)
		{
			this.attr = attr;
			this.key = key;
			this.name = name;
		}

	}

	public class EnumStatement : Statement
	{
		public Attributes attr;
		public LocatedToken key;
		public NamesExpression name;
		public List<NamesExpression> base_type;
		public List<Statement> members = new List<Statement>();

		public EnumStatement(Attributes attr, LocatedToken key, NamesExpression name)
		{
			this.attr = attr;
			this.key = key;
			this.name = name;
		}
	}

	public class FieldStatement : Statement
	{
		public Attributes attr;
		public NamesExpression type;
		public NamesExpression name;
		public Statement value;

		public FieldStatement(Attributes attr, NamesExpression type, NamesExpression name)
		{
			this.attr = attr;
			this.type = type;
			this.name = name;
		}
	}

	public class ParameterStatement : Statement
	{
		public Attributes attr;
		public NamesExpression type;
		public NamesExpression name;
		public Statement value;

		public ParameterStatement(Attributes attr, NamesExpression type, NamesExpression name)
		{
			this.attr = attr;
			this.type = type;
			this.name = name;
		}
	}

	public class MethodStatement : Statement
	{
		public Attributes attr;
		public NamesExpression type;
		public NamesExpression name;
		public List<ParameterStatement> parameters;
		public List<Statement> body;

		public MethodStatement(Attributes attr, NamesExpression type, NamesExpression name)
		{
			this.attr = attr;
			this.type = type;
			this.name = name;
		}

		public void AddParameter(ParameterStatement param)
		{
			if (parameters == null)
				parameters = new List<ParameterStatement>();
			parameters.Add(param);
		}
	}
}

