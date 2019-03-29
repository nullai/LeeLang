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
		NEW = 1 << 17,
	}

	[Flags]
	public enum ParameterAttribute
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
		public Location Location;
		public bool reachable;
	}

	public class EmptyStatement : Statement
	{
	}

	public class Block : Statement
	{
		public Location StartLocation;
		public Location EndLocation;
		public List<Statement> statements = new List<Statement>();
		public Block Parent;

		public virtual CompilerContext Compiler => Parent.Compiler;

		public Block(Block parent, Location start)
		{
			this.Parent = parent;
			this.StartLocation = start;
			this.EndLocation = start;
			Location = start;
		}
	}
	
	public class If : Statement
	{
		public Expression expr;
		public Statement TrueStatement;
		public Statement FalseStatement;

		public If(Expression bool_expr, Statement true_statement, Statement false_statement, Location l)
		{
			this.expr = bool_expr;
			TrueStatement = true_statement;
			FalseStatement = false_statement;
			Location = l;
		}
	}

	public class LoopStatement : Statement
	{
		public Statement Statement;

		protected LoopStatement(Statement statement)
		{
			Statement = statement;
		}
	}

	public class For : LoopStatement
	{
		public Statement Initializer;
		public Expression Condition;
		public Expression Iterator;

		public For(Statement init, Expression cond, Expression itr, Location l)
			: base(null)
		{
			Initializer = init;
			Condition = cond;
			Iterator = itr;
		}
	}

	//public class Foreach : LoopStatement
	//{
	//	public LocalVariable variable;
	//	public Expression expr;

	//	public Foreach(LocalVariable variable, Expression value)
	//		: base(null)
	//	{
	//		this.variable = variable;
	//		this.expr = value;
	//	}
	//}

	public class While : LoopStatement
	{
		public Expression expr;
	//	bool end_reachable;

		public While(Expression bool_expr, Statement statement, Location l)
			: base (statement)
		{
			this.expr = bool_expr;
			Location = l;
		}
	}

	public class Do : LoopStatement
	{
		public Expression expr;
		public Location WhileLocation;
	//	bool iterator_reachable, end_reachable;

		public Do(Statement statement, Expression bool_expr, Location doLocation, Location whileLocation)
			: base(statement)
		{
			expr = bool_expr;
			Location = doLocation;
			WhileLocation = whileLocation;
		}
	}
	public class Switch : LoopStatement
	{
		public Expression Expr;
	//	Dictionary<long, SwitchLabel> labels;
	//	Dictionary<string, SwitchLabel> string_labels;
	//	List<SwitchLabel> case_labels;
		public Block Block => (Block)Statement;

		public Switch(Expression value, Block block, Location l)
			: base(block)
		{
			this.Expr = value;
			Location = l;
		}
	}

	public class Return : ExitStatement
	{
		public Expression expr;
		public override bool IsLocalExit => false;

		public Return(Expression expr, Location l)
		{
			this.expr = expr;
			Location = l;
		}
	}

	public class Break : LocalExitStatement
	{
		public Break(Location l)
			: base(l)
		{
		}
	}

	public class Continue : LocalExitStatement
	{
		public Continue(Location l)
			: base(l)
		{
		}
	}
	public class LabeledStatement : Statement
	{
		string name;
		Block block;
		public LabeledStatement(string name, Block block, Location l)
		{
			this.name = name;
			this.block = block;
			this.Location = l;
		}
	}
	public class Goto : ExitStatement
	{
		string target;
		public override bool IsLocalExit => false;
		public Goto(string name, Location l)
		{
			this.target = name;
			this.Location = l;
		}
	}
	public class SwitchLabel : Statement
	{
		public Constant converted;
		public Expression value;

		public bool IsDefault => value == null;

		public SwitchLabel(Expression value, Location l)
		{
			this.value = value;
			Location = l;
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

	public abstract class ExitStatement : Statement
	{
		public abstract bool IsLocalExit { get; }
	}

	public class LocalExitStatement : ExitStatement
	{
		public LoopStatement enclosing_loop;
		public override bool IsLocalExit => true;

		protected LocalExitStatement(Location loc)
		{
			Location = loc;
		}
	}
}

