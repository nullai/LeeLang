using System;
using System.Collections.Generic;

namespace LeeLang
{
	public class Expression
	{
		public virtual bool IsLeftExpression => false;
	}
	public class NameExpression : Expression
	{
		public TokenValue token;

		public NameExpression(TokenValue token)
		{
			this.token = token;
		}

		public override bool IsLeftExpression => true;
	}

	public class AccessExpression : Expression
	{
		public Expression left;
		public TokenValue name;

		public AccessExpression(Expression left, TokenValue name)
		{
			this.left = left;
			this.name = name;
		}

		public override bool IsLeftExpression => true;
	}

	public class GenericExpression : Expression
	{
		public Expression left;
		public List<Expression> members = new List<Expression>();

		public GenericExpression(Expression left)
		{
			this.left = left;
		}

		public void Add(Expression expr)
		{
			members.Add(expr);
		}
	}

	public class ArrayExpression : Expression
	{
		public Expression left;

		public ArrayExpression(Expression left)
		{
			this.left = left;
		}
	}

	public class PointerExpression : Expression
	{
		public Expression left;

		public PointerExpression(Expression left)
		{
			this.left = left;
		}
	}

	public class AssignExpression : Expression
	{
		public Expression left;
		public Expression right;

		public AssignExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}

	public class MultAssignExpression : Expression
	{
		public Expression left;
		public Expression right;

		public MultAssignExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}

	public class DivAssignExpression : Expression
	{
		public Expression left;
		public Expression right;

		public DivAssignExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}

	public class ModAssignExpression : Expression
	{
		public Expression left;
		public Expression right;

		public ModAssignExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}

	public class AddAssignExpression : Expression
	{
		public Expression left;
		public Expression right;

		public AddAssignExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}

	public class SubAssignExpression : Expression
	{
		public Expression left;
		public Expression right;

		public SubAssignExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}

	public class ShiftLeftAssignExpression : Expression
	{
		public Expression left;
		public Expression right;

		public ShiftLeftAssignExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}

	public class ShiftRightAssignExpression : Expression
	{
		public Expression left;
		public Expression right;

		public ShiftRightAssignExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}

	public class AndAssignExpression : Expression
	{
		public Expression left;
		public Expression right;

		public AndAssignExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}

	public class XorAssignExpression : Expression
	{
		public Expression left;
		public Expression right;

		public XorAssignExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}

	public class OrAssignExpression : Expression
	{
		public Expression left;
		public Expression right;

		public OrAssignExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
}

