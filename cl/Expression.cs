using System;
using System.Collections.Generic;
using System.Text;

namespace LeeLang
{
	public class Expression
	{
		public virtual List<MemberSpec> ResolveType(ResolveContext ctx)
		{
			throw new Exception("ResolveType In " + GetType().Name);
		}

		public TypeSpec ResolveToType(ResolveContext ctx)
		{
			var types = ResolveType(ctx);
			if (types == null)
				return null;

			TypeSpec result = null;
			for (int i = 0; i < types.Count; i++)
			{
				var t = types[i] as TypeSpec;
				if (t == null)
					continue;

				if (result != null)
					ctx.complier.OutputError(string.Format("不能确定是\"{0}\"或\"{1}\"。", result, t));
				else
					result = t;
			}
			if (result == null)
				ctx.complier.OutputError(string.Format("未能找到类型或命名空间\"{0}\"(是否缺少 using 指令或程序集引用?)", this));
			return result;
		}
		public virtual IR_Value CodeGen(CodeGenContext ctx)
		{
			throw new Exception("CodeGen In " + GetType().Name);
		}
	}
	public class NameExpression : Expression
	{
		public TokenValue token;

		public static NameExpression Void = new NameExpression(new TokenValue("void", Location.Null));

		public NameExpression(TokenValue token)
		{
			this.token = token;
		}

		public override string ToString()
		{
			return token.value;
		}

		public override List<MemberSpec> ResolveType(ResolveContext ctx)
		{
			MemberSpec type = null;
			switch (token.token)
			{
				case Token.VOID:
					type = BuildinTypeSpec.Void;
					break;
				case Token.BOOL:
					type = BuildinTypeSpec.Bool;
					break;
				case Token.CHAR:
					type = BuildinTypeSpec.Char;
					break;
				case Token.SBYTE:
					type = BuildinTypeSpec.SByte;
					break;
				case Token.BYTE:
					type = BuildinTypeSpec.Byte;
					break;
				case Token.SHORT:
					type = BuildinTypeSpec.Short;
					break;
				case Token.USHORT:
					type = BuildinTypeSpec.UShort;
					break;
				case Token.INT:
					type = BuildinTypeSpec.Int;
					break;
				case Token.UINT:
					type = BuildinTypeSpec.UInt;
					break;
				case Token.LONG:
					type = BuildinTypeSpec.Long;
					break;
				case Token.ULONG:
					type = BuildinTypeSpec.ULong;
					break;
				case Token.FLOAT:
					type = BuildinTypeSpec.Float;
					break;
				case Token.DOUBLE:
					type = BuildinTypeSpec.Double;
					break;
				case Token.STRING:
					type = BuildinTypeSpec.String;
					break;
				case Token.OBJECT:
					type = BuildinTypeSpec.Object;
					break;
				default:
					var r = ctx.scope.ResolveName(token.value);
					if (r == null)
					{
						ctx.complier.OutputError(string.Format("未能找到类型或命名空间\"{0}\"(是否缺少 using 指令或程序集引用?)", token.value));
						return null;
					}
					return r;
			}
			return new List<MemberSpec>() { type };
		}

		public override IR_Value CodeGen(CodeGenContext ctx)
		{
			var r = ctx.scope.ResolveName(token.value);

			FieldSpec v = null;
			for (int j = 0; j < r.Count; j++)
			{
				if (!(r[j] is FieldSpec))
					continue;

				if (v != null)
					ctx.complier.OutputError(string.Format("不确定使用\"{0}\"或是\"{1}\"", v, r[j]));
				else
					v = r[j] as FieldSpec;
			}

			if (v == null)
			{
				ctx.complier.OutputError(string.Format("未定义的变量\"{0}\"。)", token));
				return null;
			}

			return v.GetIRValue();
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
		public override string ToString()
		{
			return left.ToString() + "." + name.value;
		}
		public override List<MemberSpec> ResolveType(ResolveContext ctx)
		{
			var r = left.ResolveType(ctx);
			if (r == null)
				return null;
			List<MemberSpec> result = null;
			for (int i = 0; i < r.Count; i++)
			{
				if (!(r[i] is NamespaceSpec))
					continue;

				var t = r[i].ResolveName(name.value);
				if (t != null)
				{
					if (result != null)
						result.AddRange(t);
					else
						result = t;
				}
			}
			if (result == null)
				ctx.complier.OutputError(string.Format("未能找到类型或命名空间\"{0}\"(是否缺少 using 指令或程序集引用?)", name));
			return result;
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
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(left.ToString());
			sb.Append('<');
			for (int i = 0; i < members.Count; i++)
			{
				if (i != 0)
					sb.Append(',');
				sb.Append(members[i].ToString());
			}
			sb.Append('>');
			return sb.ToString();
		}
		public void Add(Expression expr)
		{
			members.Add(expr);
		}
		public override List<MemberSpec> ResolveType(ResolveContext ctx)
		{
			var r = left.ResolveType(ctx);
			if (r == null)
				return null;

			TypeSpec[] args = null;
			List<MemberSpec> result = null;
			for (int i = 0; i < r.Count; i++)
			{
				ClassStructSpec g = r[i] as ClassStructSpec;
				if (g == null || g.Arity != members.Count)
					continue;

				if (args == null)
				{
					args = GetTypeArgs(ctx);
					if (args == null)
						return null;
				}
				var t = g.CreateGenericType(args);
				if (result != null)
					result.Add(t);
				else
					result = new List<MemberSpec>() { t };
			}
			if (args != null)
				return result;

			ctx.complier.OutputError(string.Format("未能找到类型名\"{0}`{1}\"(是否缺少 using 指令或程序集引用?)", left, members.Count));
			return null;
		}
		private TypeSpec[] GetTypeArgs(ResolveContext ctx)
		{
			bool error = false;
			TypeSpec[] args = new TypeSpec[members.Count];
			for (int i = 0; i < args.Length; i++)
			{
				var r = members[i].ResolveType(ctx);
				if (r == null)
				{
					error = true;
					continue;
				}
				for (int j = 0; j < r.Count; j++)
				{
					if (!(r[j] is TypeSpec))
						continue;

					ctx.CheckAccess(r[j]);
					if (args[i] != null)
						ctx.complier.OutputError(string.Format("不确定使用\"{0}\"或是\"{1}\"", args[i], r[j]));
					else
						args[i] = r[j] as TypeSpec;
				}

				if (args[i] == null)
				{
					ctx.complier.OutputError(string.Format("未能找到类型名\"{0}\"(是否缺少 using 指令或程序集引用?)", members[i]));
					error = true;
				}
			}
			if (error)
				return null;

			return args;
		}
	}

	public class ArrayExpression : Expression
	{
		public Expression left;

		public ArrayExpression(Expression left)
		{
			this.left = left;
		}
		public override string ToString()
		{
			return left.ToString() + "[]";
		}
		public override List<MemberSpec> ResolveType(ResolveContext ctx)
		{
			var r = left.ResolveType(ctx);
			if (r == null)
				return null;
			List<MemberSpec> result = null;
			for (int i = 0; i < r.Count; i++)
			{
				var t = (r[i] as TypeSpec).MakeArray();
				if (result != null)
					result.Add(t);
				else
					result = new List<MemberSpec>() { t };
			}
			return result;
		}
	}

	public class PointerExpression : Expression
	{
		public Expression left;

		public PointerExpression(Expression left)
		{
			this.left = left;
		}
		public override string ToString()
		{
			return left.ToString() + "*";
		}
		public override List<MemberSpec> ResolveType(ResolveContext ctx)
		{
			var r = left.ResolveType(ctx);
			if (r == null)
				return null;
			List<MemberSpec> result = null;
			for (int i = 0; i < r.Count; i++)
			{
				var t = (r[i] as TypeSpec).MakePointer();
				if (result != null)
					result.Add(t);
				else
					result = new List<MemberSpec>() { t };
			}
			return result;
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
		public override string ToString()
		{
			return left.ToString() + " = " + right.ToString();
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
		public override string ToString()
		{
			return cond.ToString() + " ? " + v_true.ToString() + " : " + v_false.ToString();
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
		public override string ToString()
		{
			return "(" + type.ToString() + ")" + value.ToString();
		}
	}
	public class NegExpression : Expression
	{
		public Expression value;

		public NegExpression(Expression value)
		{
			this.value = value;
		}
		public override string ToString()
		{
			return "-" + value.ToString();
		}
	}
	public class NotExpression : Expression
	{
		public Expression value;

		public NotExpression(Expression value)
		{
			this.value = value;
		}
		public override string ToString()
		{
			return "~" + value.ToString();
		}
	}
	public class LogicNotExpression : Expression
	{
		public Expression value;

		public LogicNotExpression(Expression value)
		{
			this.value = value;
		}
		public override string ToString()
		{
			return "!" + value.ToString();
		}
	}
	public class ValueListExpression : Expression
	{
		public List<Expression> values = new List<Expression>();

		public ValueListExpression()
		{
		}
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < values.Count; i++)
			{
				if (i != 0)
					sb.Append(',');
				sb.Append(values[i].ToString());
			}
			return sb.ToString();
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
		public override string ToString()
		{
			return value.ToString() + "[" + args.ToString() + "]";
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
		public override string ToString()
		{
			return value.ToString() + "(" + args.ToString() + ")";
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
		public override string ToString()
		{
			return value;
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
		public override string ToString()
		{
			return "nameof(" + expr.ToString() + ")";
		}
	}
	public class NullConstantExpression : ConstantExpression
	{
		public NullConstantExpression()
			: base(BuildinTypeSpec.Object)
		{
		}
		public override string ToString()
		{
			return "null";
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
		public override string ToString()
		{
			return value.ToString();
		}
	}
	public class LiteralExpression : Expression
	{
		public TokenValue value;

		public LiteralExpression(TokenValue value)
		{
			this.value = value;
		}
		public override string ToString()
		{
			return value.ToString();
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
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("new ");
			sb.Append(type.ToString());
			sb.Append(is_array ? '[' : '(');
			if (args != null)
				sb.Append(args.ToString());
			sb.Append(is_array ? ']' : ')');
			if (value != null)
			{
				sb.Append('{');
				sb.Append(value.ToString());
				sb.Append('}');
			}
			return sb.ToString();
		}
	}
	public class TypeofExpression : Expression
	{
		public Expression type;

		public TypeofExpression(Expression type)
		{
			this.type = type;
		}
		public override string ToString()
		{
			return "typeof(" + type.ToString() + ")";
		}
	}
	public class DefaultExpression : Expression
	{
		public DefaultExpression()
		{
		}
		public override string ToString()
		{
			return "default";
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
		public override string ToString()
		{
			switch (oper)
			{
				case Operator.Multiply:
					return left.ToString() + " * " + right.ToString();
				case Operator.Division:
					return left.ToString() + " / " + right.ToString();
				case Operator.Modulus:
					return left.ToString() + " % " + right.ToString();
				case Operator.Addition:
					return left.ToString() + " + " + right.ToString();
				case Operator.Subtraction:
					return left.ToString() + " - " + right.ToString();
				case Operator.LeftShift:
					return left.ToString() + " << " + right.ToString();
				case Operator.RightShift:
					return left.ToString() + " >> " + right.ToString();
				case Operator.LessThan:
					return left.ToString() + " < " + right.ToString();
				case Operator.GreaterThan:
					return left.ToString() + " > " + right.ToString();
				case Operator.LessThanOrEqual:
					return left.ToString() + " <= " + right.ToString();
				case Operator.GreaterThanOrEqual:
					return left.ToString() + " >= " + right.ToString();
				case Operator.Equality:
					return left.ToString() + " == " + right.ToString();
				case Operator.Inequality:
					return left.ToString() + " != " + right.ToString();
				case Operator.BitwiseAnd:
					return left.ToString() + " & " + right.ToString();
				case Operator.ExclusiveOr:
					return left.ToString() + " ^ " + right.ToString();
				case Operator.BitwiseOr:
					return left.ToString() + " | " + right.ToString();
				case Operator.LogicalAnd:
					return left.ToString() + " && " + right.ToString();
				case Operator.LogicalOr:
					return left.ToString() + " || " + right.ToString();
			}
			return "<<error>>";
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
		public override string ToString()
		{
			switch (oper)
			{
				case BinaryExpression.Operator.Multiply:
					return left.ToString() + " *= " + right.ToString();
				case BinaryExpression.Operator.Division:
					return left.ToString() + " /= " + right.ToString();
				case BinaryExpression.Operator.Modulus:
					return left.ToString() + " %= " + right.ToString();
				case BinaryExpression.Operator.Addition:
					return left.ToString() + " += " + right.ToString();
				case BinaryExpression.Operator.Subtraction:
					return left.ToString() + " -= " + right.ToString();
				case BinaryExpression.Operator.LeftShift:
					return left.ToString() + " <<= " + right.ToString();
				case BinaryExpression.Operator.RightShift:
					return left.ToString() + " >>= " + right.ToString();
				case BinaryExpression.Operator.BitwiseAnd:
					return left.ToString() + " &= " + right.ToString();
				case BinaryExpression.Operator.ExclusiveOr:
					return left.ToString() + " ^= " + right.ToString();
				case BinaryExpression.Operator.BitwiseOr:
					return left.ToString() + " |= " + right.ToString();
			}
			return "<<error>>";
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
		public override string ToString()
		{
			switch (mode)
			{
				case Mode.PreIncrement:
					return "++" + expr.ToString();
				case Mode.PreDecrement:
					return "--" + expr.ToString();
				case Mode.PostIncrement:
					return expr.ToString() + "++";
				case Mode.PostDecrement:
					return expr.ToString() + "--";
			}
			return "<<error>>";
		}
	}
	public class StringConcat : Expression
	{
		public ValueListExpression args;

		public StringConcat(ValueListExpression args)
		{
			this.args = args;
		}
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < args.values.Count; i++)
			{
				if (i != 0)
					sb.Append('+');
				sb.Append(args.values[i].ToString());
			}
			return sb.ToString();
		}
	}
}

