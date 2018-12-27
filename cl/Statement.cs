using System;
using System.Collections.Generic;

namespace LeeLang
{
	[Flags]
	public enum CommonAttribute
	{
		NONE = 0,
		PUBLIC = 1 << 0,
		PROTECTED = 1 << 1,
		PRIVATE = 1 << 2,
		STATIC = 1 << 3,
		CONST = 1 << 4,
		ABSTRACT = 1 << 5,
		EXPLICIT = 1 << 6,
		EXTERN = 1 << 7,
		FINALLY = 1 << 8,
		IMPLICIT = 1 << 9,
		INTERFACE = 1 << 10,
		INTERNAL = 1 << 11,
		OVERRIDE = 1 << 12,
		SEALED = 1 << 13,
		VIRTUAL = 1 << 14,
		VOLATILE = 1 << 15,
		PARTIAL = 1 << 16,
	}

	[Flags]
	public enum ParamAttribute
	{
		NONE = 0,
		IN = 1,
		OUT = 2,
		REF = 4,
		PARAMS = 8,
		THIS = 16,
	}
	public abstract class Statement
	{

	}

	public class ErrorStatement : Statement
	{
	}

	public class EmptyStatement : Statement
	{
	}

	public class AttributeStatement : Statement
	{

	}

	public class BlockStatement : Statement
	{
		public List<Statement> values = new List<Statement>();
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
		public NameExpression name;
		public Expression value;

		public UsingStatement(NameExpression name, Expression value)
		{
			this.name = name;
			this.value = value;
		}
	}

	public class NamespaceStatement : Statement
	{
		public Expression name;
		public List<Statement> members = new List<Statement>();

		public NamespaceStatement(Expression name)
		{
			this.name = name;
		}
	}

	public class TypeStatement : Statement
	{
		public Token mToken;
		public CommonAttribute attr;
		public NameExpression name;
		public List<Expression> base_type;
		public List<Statement> members = new List<Statement>();

		public TypeStatement(CommonAttribute attr, Token token, NameExpression name)
		{
			this.mToken = token;
			this.attr = attr;
			this.name = name;
		}
	}

	public class FieldStatement : Statement
	{
		public CommonAttribute attr;
		public Expression type;
		public NameExpression name;
		public Expression value;
		public FieldStatement next;

		public FieldStatement(CommonAttribute attr, Expression type, NameExpression name)
		{
			this.attr = attr;
			this.type = type;
			this.name = name;
		}
	}

	public class EnumFieldStatement : Statement
	{
		public NameExpression name;
		public Expression value;

		public EnumFieldStatement(NameExpression name, Expression value)
		{
			this.name = name;
			this.value = value;
		}
	}

	public class PropertyStatement : Statement
	{
		public CommonAttribute attr;
		public Expression type;
		public NameExpression inter;
		public NameExpression name;
		public List<ParameterStatement> parameters;
		public List<MethodStatement> value = new List<MethodStatement>();

		public PropertyStatement(CommonAttribute attr, Expression type, NameExpression inter, NameExpression name)
		{
			this.attr = attr;
			this.type = type;
			this.inter = inter;
			this.name = name;
		}
	}

	public class ParameterStatement : Statement
	{
		public ParamAttribute attr;
		public Expression type;
		public NameExpression name;
		public Statement value;

		public ParameterStatement(ParamAttribute attr, Expression type, NameExpression name)
		{
			this.attr = attr;
			this.type = type;
			this.name = name;
		}
	}

	public class MethodStatement : Statement
	{
		public CommonAttribute attr;
		public Expression type;
		public NameExpression inter;
		public NameExpression name;
		public List<ParameterStatement> parameters;
		public BlockStatement body;

		public MethodStatement(CommonAttribute attr, Expression type, NameExpression inter, NameExpression name)
		{
			this.attr = attr;
			this.type = type;
			this.inter = inter;
			this.name = name;
		}

		public void AddParameter(ParameterStatement param)
		{
			if (parameters == null)
				parameters = new List<ParameterStatement>();
			parameters.Add(param);
		}
	}

	public class IfStatement : Statement
	{
		public Expression cond;
		public Statement bodyt;
		public Statement bodyf;

		public IfStatement(Expression cond)
		{
			this.cond = cond;
		}
	}

	public class ForStatement : Statement
	{
		public Statement init;
		public Expression cond;
		public Expression iter;
		public Statement body;

		public ForStatement(Statement init, Expression cond, Expression iter)
		{
			this.init = init;
			this.cond = cond;
			this.iter = iter;
		}
	}

	public class ForeachStatement : Statement
	{
		public Statement iter;
		public Expression value;
		public Statement body;

		public ForeachStatement(Statement iter, Expression value)
		{
			this.iter = iter;
			this.value = value;
		}
	}

	public class WhileStatement : Statement
	{
		public Expression cond;
		public Statement body;

		public WhileStatement(Expression cond)
		{
			this.cond = cond;
		}
	}

	public class DoStatement : Statement
	{
		public Expression cond;
		public Statement body;

		public DoStatement()
		{
		}
	}
	public class SwitchStatement : Statement
	{
		public Expression value;
		public BlockStatement body;

		public SwitchStatement(Expression value)
		{
			this.value = value;
		}
	}

	public class ReturnStatement : Statement
	{
		public Expression value;

		public ReturnStatement(Expression value)
		{
			this.value = value;
		}
	}

	public class BreakStatement : Statement
	{
		public BreakStatement()
		{
		}
	}

	public class ContinueStatement : Statement
	{
		public ContinueStatement()
		{
		}
	}
	public class LabelStatement : Statement
	{
		TokenValue name;
		public LabelStatement(TokenValue name)
		{
			this.name = name;
		}
	}
	public class GotoStatement : Statement
	{
		TokenValue name;
		public GotoStatement(TokenValue name)
		{
			this.name = name;
		}
	}
	public class CaseStatement : Statement
	{
		public Expression value;
		public CaseStatement(Expression value)
		{
			this.value = value;
		}
	}
	public class ExpressionStatement : Statement
	{
		public Expression value;
		public ExpressionStatement(Expression value)
		{
			this.value = value;
		}
	}
}

