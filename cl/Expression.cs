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

	public class ConditionalExpression : Expression
	{
		public Expression cond;
		public Expression v_true;
		public Expression v_false;

		public ConditionalExpression(Expression cond, Expression v_true, Expression v_false)
		{
			this.cond = cond;
			this.v_true = v_true;
			this.v_false = v_false;
		}
	}

	public class LogicOrExpression : Expression
	{
		public Expression left;
		public Expression right;

		public LogicOrExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
	public class LogicAndExpression : Expression
	{
		public Expression left;
		public Expression right;

		public LogicAndExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
	public class OrExpression : Expression
	{
		public Expression left;
		public Expression right;

		public OrExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
	public class XorExpression : Expression
	{
		public Expression left;
		public Expression right;

		public XorExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
	public class AndExpression : Expression
	{
		public Expression left;
		public Expression right;

		public AndExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
	public class EqualityExpression : Expression
	{
		public Expression left;
		public Expression right;

		public EqualityExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
	public class NotEqualityExpression : Expression
	{
		public Expression left;
		public Expression right;

		public NotEqualityExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
	public class LtExpression : Expression
	{
		public Expression left;
		public Expression right;

		public LtExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
	public class GtExpression : Expression
	{
		public Expression left;
		public Expression right;

		public GtExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
	public class LeExpression : Expression
	{
		public Expression left;
		public Expression right;

		public LeExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
	public class GeExpression : Expression
	{
		public Expression left;
		public Expression right;

		public GeExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
	public class ShiftLeftExpression : Expression
	{
		public Expression left;
		public Expression right;

		public ShiftLeftExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
	public class ShiftRightExpression : Expression
	{
		public Expression left;
		public Expression right;

		public ShiftRightExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
	public class AddExpression : Expression
	{
		public Expression left;
		public Expression right;

		public AddExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
	public class SubExpression : Expression
	{
		public Expression left;
		public Expression right;

		public SubExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
	public class MultExpression : Expression
	{
		public Expression left;
		public Expression right;

		public MultExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
	public class DivExpression : Expression
	{
		public Expression left;
		public Expression right;

		public DivExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
	public class ModExpression : Expression
	{
		public Expression left;
		public Expression right;

		public ModExpression(Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
	}
	public class CastExpression : Expression
	{
		public Expression type;
		public Expression value;

		public CastExpression(Expression type, Expression value)
		{
			this.type = type;
			this.value = value;
		}
	}
	public class IncExpression : Expression
	{
		public Expression value;

		public IncExpression(Expression value)
		{
			this.value = value;
		}
	}
	public class PostIncExpression : Expression
	{
		public Expression value;

		public PostIncExpression(Expression value)
		{
			this.value = value;
		}
	}
	public class DecExpression : Expression
	{
		public Expression value;

		public DecExpression(Expression value)
		{
			this.value = value;
		}
	}
	public class PostDecExpression : Expression
	{
		public Expression value;

		public PostDecExpression(Expression value)
		{
			this.value = value;
		}
	}
	public class NegExpression : Expression
	{
		public Expression value;

		public NegExpression(Expression value)
		{
			this.value = value;
		}
	}
	public class NotExpression : Expression
	{
		public Expression value;

		public NotExpression(Expression value)
		{
			this.value = value;
		}
	}
	public class LogicNotExpression : Expression
	{
		public Expression value;

		public LogicNotExpression(Expression value)
		{
			this.value = value;
		}
	}
	public class ValueListExpression : Expression
	{
		public List<Expression> values = new List<Expression>();

		public ValueListExpression()
		{
		}
		public void Add(Expression val)
		{
			values.Add(val);
		}
	}
	public class IndexExpression : Expression
	{
		public Expression value;
		public ValueListExpression args;

		public IndexExpression(Expression value, ValueListExpression args)
		{
			this.value = value;
			this.args = args;
		}
	}
	public class InvokeExpression : Expression
	{
		public Expression value;
		public ValueListExpression args;

		public InvokeExpression(Expression value, ValueListExpression args)
		{
			this.value = value;
			this.args = args;
		}
	}
	public class MemberExpression : Expression
	{
		public Expression value;
		public NameExpression name;

		public MemberExpression(Expression value, NameExpression name)
		{
			this.value = value;
			this.name = name;
		}
	}
	public class LiteralExpression : Expression
	{
		public TokenValue value;

		public LiteralExpression(TokenValue value)
		{
			this.value = value;
		}
	}
	public class NewExpression : Expression
	{
		public Expression type;
		public ValueListExpression args;
		public ValueListExpression value;
		public bool is_array;

		public NewExpression(Expression type, ValueListExpression args, ValueListExpression value, bool is_array)
		{
			this.type = type;
			this.args = args;
			this.value = value;
			this.is_array = is_array;
		}
	}
	public class TypeofExpression : Expression
	{
		public Expression type;

		public TypeofExpression(Expression type)
		{
			this.type = type;
		}
	}
	public class DefaultExpression : Expression
	{
		public DefaultExpression()
		{
		}
	}
}

