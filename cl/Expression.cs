using System;
using System.Collections.Generic;

namespace LeeLang
{
	public class Expression
	{
		public virtual void ResolveMember(ResolveContext ctx, List<MemberSpec> result, MemberSpec.VerifyMember verify)
		{
			throw new Exception("ResolveMember In " + GetType().Name);
		}
	}
	public class NameExpression : Expression
	{
		public TokenValue token;

		public NameExpression(TokenValue token)
		{
			this.token = token;
		}

		public override void ResolveMember(ResolveContext ctx, List<MemberSpec> result, MemberSpec.VerifyMember verify)
		{
			int count = result.Count;
			ctx.scope.ResolveMember(token.value, result, verify);
			if (result.Count == count)
			{
				ctx.complier.OutputError(string.Format("未能找到名称\"{0}\"(是否缺少 using 指令或程序集引用?)", token.value));
				return;
			}
		}
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
		public override void ResolveMember(ResolveContext ctx, List<MemberSpec> result, MemberSpec.VerifyMember verify)
		{
			List<MemberSpec> r = new List<MemberSpec>();
			left.ResolveMember(ctx, r, x => x is NamespaceSpec);
			if (r.Count == 0)
			{
				ctx.complier.OutputError(string.Format("未能找到类型或命名空间名\"{0}\"(是否缺少 using 指令或程序集引用?)", left));
				return;
			}
			for (int i = 0; i < r.Count; i++)
			{
				r[i].ResolveMember(name.value, result, verify);
			}
		}
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
		public override void ResolveMember(ResolveContext ctx, List<MemberSpec> result, MemberSpec.VerifyMember verify)
		{
			List<MemberSpec> gt = new List<MemberSpec>();
			left.ResolveMember(ctx, gt, x => x is GenericTypeSpec && (x as GenericTypeSpec).Arity == members.Count);
			if (gt.Count == 0)
				return;

			bool error = false;
			List<TypeSpec>[] types = new List<TypeSpec>[members.Count];
			List<MemberSpec> ts = new List<MemberSpec>();
			for (int i = 0; i < types.Length; i++)
			{
				ts.Clear();

				members[i].ResolveMember(ctx, ts, x => x is TypeSpec);
				if (ts.Count == 0)
				{
					ctx.complier.OutputError(string.Format("未能找到类型\"{0}\"(是否缺少 using 指令或程序集引用?)", members[i]));
					error = true;
					continue;
				}

				List<TypeSpec> vs = new List<TypeSpec>();
				for (int j = 0; j < ts.Count; j++)
				{
					vs.Add(ts[i] as TypeSpec);
				}
				types[i] = vs;
			}
			if (error)
				return;

			TypeSpec[] args = new TypeSpec[members.Count];
			EnumCreateType(types, args, 0, gt, result, verify);
		}
		private void EnumCreateType(List<TypeSpec>[] types, TypeSpec[] args, int idx, List<MemberSpec> gt, List<MemberSpec> result, MemberSpec.VerifyMember verify)
		{
			if (idx >= types.Length)
			{
				for (int i = 0; i < gt.Count; i++)
				{
					var t = (gt[i] as GenericTypeSpec).CreateType(args);
					if (verify == null || verify(t))
						result.Add(t);
				}
			}
			var ts = types[idx];
			for (int i = 0; i < ts.Count; i++)
			{
				args[idx] = ts[i];
				EnumCreateType(types, args, idx + 1, gt, result, verify);
			}
		}
	}

	public class ArrayExpression : Expression
	{
		public Expression left;

		public ArrayExpression(Expression left)
		{
			this.left = left;
		}
		public override void ResolveMember(ResolveContext ctx, List<MemberSpec> result, MemberSpec.VerifyMember verify)
		{
			left.ResolveMember(ctx, result, x => x is TypeSpec);
			if (result.Count == 0)
			{
				ctx.complier.OutputError(string.Format("未能找到类型或命名空间名\"{0}\"(是否缺少 using 指令或程序集引用?)", left));
				return;
			}
			for (int i = result.Count - 1; i >= 0; i--)
			{
				TypeSpec t = result[i] as TypeSpec;
				t = t.MakeArray();
				if (verify == null || verify(t))
					result[i] = t;
				else
					result.RemoveAt(i);
			}
		}
	}

	public class PointerExpression : Expression
	{
		public Expression left;

		public PointerExpression(Expression left)
		{
			this.left = left;
		}
		public override void ResolveMember(ResolveContext ctx, List<MemberSpec> result, MemberSpec.VerifyMember verify)
		{
			left.ResolveMember(ctx, result, x => x is TypeSpec);
			if (result.Count == 0)
			{
				ctx.complier.OutputError(string.Format("未能找到类型或命名空间名\"{0}\"(是否缺少 using 指令或程序集引用?)", left));
				return;
			}
			for (int i = result.Count - 1; i >= 0; i--)
			{
				TypeSpec t = result[i] as TypeSpec;
				t = t.MakePointer();
				if (verify == null || verify(t))
					result[i] = t;
				else
					result.RemoveAt(i);
			}
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
	public class TypeCastExpression : Expression
	{
		public Expression type;
		public Expression value;

		public TypeCastExpression(Expression type, Expression value)
		{
			this.type = type;
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
	public class InvocationExpression : Expression
	{
		public Expression value;
		public ValueListExpression args;

		public InvocationExpression(Expression value, ValueListExpression args)
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
	public class ConstantExpression : Expression
	{
		public BuildinTypeSpec type;

		public ConstantExpression(BuildinTypeSpec type)
		{
			this.type = type;
		}
	}
	public class StringConstantExpression : ConstantExpression
	{
		protected string value;

		public StringConstantExpression(string value)
			: base(BuildinTypeSpec.String)
		{
			this.value = value;
		}
		public string Value => value;
	}
	public class NameOfExpression : StringConstantExpression
	{
		public Exception expr;
		public NameOfExpression(Exception expr)
			: base(null)
		{
			this.expr = expr;
		}
	}
	public class NullConstantExpression : ConstantExpression
	{
		public NullConstantExpression()
			: base(BuildinTypeSpec.Object)
		{
		}
	}
	public class EnumConstantExpression : ConstantExpression
	{
		public long value;
		public EnumConstantExpression(BuildinTypeSpec type, long value)
			: base(type)
		{
			this.value = value;
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

	public class BinaryExpression : Expression
	{
		[Flags]
		public enum Operator
		{
			Multiply = 0 | ArithmeticMask,
			Division = 1 | ArithmeticMask,
			Modulus = 2 | ArithmeticMask,
			Addition = 3 | ArithmeticMask | AdditionMask,
			Subtraction = 4 | ArithmeticMask | SubtractionMask,

			LeftShift = 5 | ShiftMask,
			RightShift = 6 | ShiftMask,

			LessThan = 7 | ComparisonMask | RelationalMask,
			GreaterThan = 8 | ComparisonMask | RelationalMask,
			LessThanOrEqual = 9 | ComparisonMask | RelationalMask,
			GreaterThanOrEqual = 10 | ComparisonMask | RelationalMask,
			Equality = 11 | ComparisonMask | EqualityMask,
			Inequality = 12 | ComparisonMask | EqualityMask,

			BitwiseAnd = 13 | BitwiseMask,
			ExclusiveOr = 14 | BitwiseMask,
			BitwiseOr = 15 | BitwiseMask,

			LogicalAnd = 16 | LogicalMask,
			LogicalOr = 17 | LogicalMask,

			//
			// Operator masks
			//
			ValuesOnlyMask = ArithmeticMask - 1,
			ArithmeticMask = 1 << 5,
			ShiftMask = 1 << 6,
			ComparisonMask = 1 << 7,
			EqualityMask = 1 << 8,
			BitwiseMask = 1 << 9,
			LogicalMask = 1 << 10,
			AdditionMask = 1 << 11,
			SubtractionMask = 1 << 12,
			RelationalMask = 1 << 13,

			DecomposedMask = 1 << 19,
			NullableMask = 1 << 20
		}
		public readonly Operator oper;
		public Expression left, right;

		public BinaryExpression(Operator oper, Expression left, Expression right)
		{
			this.oper = oper;
			this.left = left;
			this.right = right;
		}
	}

	public class CompoundAssign : Expression
	{
		public readonly BinaryExpression.Operator oper;
		public Expression left, right;

		public CompoundAssign(BinaryExpression.Operator oper, Expression left, Expression right)
		{
			this.oper = oper;
			this.left = left;
			this.right = right;
		}
	}

	public class UnaryMutator : Expression
	{
		[Flags]
		public enum Mode : byte
		{
			IsIncrement = 0,
			IsDecrement = 1,
			IsPre = 0,
			IsPost = 2,

			PreIncrement = 0,
			PreDecrement = IsDecrement,
			PostIncrement = IsPost,
			PostDecrement = IsPost | IsDecrement
		}
		public Mode mode;
		public Expression expr;

		public UnaryMutator(Mode mode, Expression expr)
		{
			this.mode = mode;
			this.expr = expr;
		}
	}
	public class StringConcat : Expression
	{
		public ValueListExpression args;

		public StringConcat(ValueListExpression args)
		{
			this.args = args;
		}
	}
}

