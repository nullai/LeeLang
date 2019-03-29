using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class Expression
	{
		public TypeSpec Type;
		public Location Location;

		public virtual Location StartLocation => Location;

		public override string ToString()
		{
			return base.ToString();
		}

		public virtual Expression Resolve(ResolveContext ctx)
		{
			return this;
		}
	}

	public class NameExpression : Expression
	{
		public NameExpression Left;
		public string Name;

		public NameExpression(NameExpression left, string name, Location l)
		{
			Left = left;
			Name = name;
			Location = l;
		}

		public AstNamespace CreateNamespace(AstNamespace parent)
		{
			if (Left == null)
				return new AstNamespace(parent, Name, Location);

			return new AstNamespace(Left.CreateNamespace(parent), Name, Location);
		}
	}

	public class FullNamedExpression : Expression
	{
		public FullNamedExpression Left;
		public virtual int Arity => 0;

		public virtual TypeArguments GetTypeArguments()
		{
			throw new Exception();
		}
	}

	public class SimpleName : FullNamedExpression
	{
		public string Name;
		public TypeArguments TypeArgs;

		public SimpleName(string name, Location l)
		{
			Name = name;
			Location = l;
		}

		public override TypeArguments GetTypeArguments()
		{
			if (TypeArgs == null)
				TypeArgs = new TypeArguments();
			return TypeArgs;
		}

		public override int Arity => TypeArgs == null ? 0 : TypeArgs.Count;
		public bool HasTypeArguments => TypeArgs != null && !TypeArgs.IsEmpty;

		public override string ToString()
		{
			if (TypeArgs != null)
			{
				return Name + "<" + TypeArgs + ">";
			}

			return Name;
		}
	}

	public class VarExpr : SimpleName
	{
		public VarExpr(Location loc)
			: base("var", loc)
		{
		}
	}

	public class ArrayName : FullNamedExpression
	{
		public ArrayName(FullNamedExpression left)
		{
			Left = left;
		}

		public override string ToString()
		{
			return Left.ToString() + "[]";
		}
	}

	public class PointerName : FullNamedExpression
	{
		public PointerName(FullNamedExpression left)
		{
			Left = left;
		}

		public override string ToString()
		{
			return Left.ToString() + "*";
		}
	}

	public class TypeExpr : FullNamedExpression
	{
		public TypeExpr(TypeSpec type, Location loc)
		{
			Type = type;
			Location = loc;
		}

		public override string ToString()
		{
			return Type.ToString();
		}
	}

	public class Invocation : Expression
	{
		public Arguments arguments;
		public Expression expr;
		public MethodGroupExpr mg;

		public Invocation(Expression expr, Arguments arguments)
		{
			this.expr = expr;
			this.arguments = arguments;
			if (expr != null)
			{
				Location = expr.Location;
			}
		}
	}

	public class MemberAccess : Expression
	{
		public Expression expr;
		public string name;

		public MemberAccess(Expression expr, string name, Location loc)
		{
			this.expr = expr;
			this.name = name;
			Location = loc;
		}
	}

	public class GenericAccess : Expression
	{
		public Expression expr;
		public TypeArguments arguments;

		public GenericAccess(Expression expr, TypeArguments arguments, Location loc)
		{
			this.expr = expr;
			this.arguments = arguments;
			Location = loc;
		}
	}

	public class New : Expression
	{
		public Arguments arguments;
		public Expression RequestedType;
		public MethodSpec method;

		public New(Expression requested_type, Arguments arguments, Location l)
		{
			RequestedType = requested_type;
			this.arguments = arguments;
			Location = l;
		}
	}

	public class Assign : Expression
	{
		public Expression target;
		public Expression source;

		protected Assign(Expression target, Expression source, Location loc)
		{
			this.target = target;
			this.source = source;
			this.Location = loc;
		}
	}

	public class CompoundAssign : Assign
	{
		public Binary.Operator op;
		public Expression right;
		public Expression left;

		public CompoundAssign(Binary.Operator op, Expression target, Expression source)
			: base(target, source, target.Location)
		{
			right = source;
			this.op = op;
		}

		public CompoundAssign(Binary.Operator op, Expression target, Expression source, Expression left)
			: this(op, target, source)
		{
			this.left = left;
		}
	}

	public class SimpleAssign : Assign
	{
		public SimpleAssign(Expression target, Expression source)
			: this(target, source, target.Location)
		{
		}

		public SimpleAssign(Expression target, Expression source, Location loc)
			: base(target, source, loc)
		{
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

		Mode mode;

		protected Expression expr;

		public UnaryMutator(Mode m, Expression e, Location loc)
		{
			mode = m;
			this.Location = loc;
			expr = e;
		}
	}

	public class TypeCast : Expression
	{
		public Expression child;
		public FullNamedExpression return_type;

		public TypeCast(Expression child, FullNamedExpression return_type)
		{
			Location = child.Location;
			this.return_type = return_type;
			this.child = child;
		}
	}

	public class Binary : Expression
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

		readonly Operator oper;
		Expression left, right;

		public Binary(Operator oper, Expression left, Expression right)
			: this(oper, left, right, left.Location)
		{
		}

		public Binary(Operator oper, Expression left, Expression right, Location loc)
		{
			this.oper = oper;
			this.left = left;
			this.right = right;
			this.Location = loc;
		}

		string OperName(Operator oper)
		{
			string s;
			switch (oper)
			{
				case Operator.Multiply:
					s = "*";
					break;
				case Operator.Division:
					s = "/";
					break;
				case Operator.Modulus:
					s = "%";
					break;
				case Operator.Addition:
					s = "+";
					break;
				case Operator.Subtraction:
					s = "-";
					break;
				case Operator.LeftShift:
					s = "<<";
					break;
				case Operator.RightShift:
					s = ">>";
					break;
				case Operator.LessThan:
					s = "<";
					break;
				case Operator.GreaterThan:
					s = ">";
					break;
				case Operator.LessThanOrEqual:
					s = "<=";
					break;
				case Operator.GreaterThanOrEqual:
					s = ">=";
					break;
				case Operator.Equality:
					s = "==";
					break;
				case Operator.Inequality:
					s = "!=";
					break;
				case Operator.BitwiseAnd:
					s = "&";
					break;
				case Operator.BitwiseOr:
					s = "|";
					break;
				case Operator.ExclusiveOr:
					s = "^";
					break;
				case Operator.LogicalOr:
					s = "||";
					break;
				case Operator.LogicalAnd:
					s = "&&";
					break;
				default:
					s = oper.ToString();
					break;
			}

			return s;
		}
	}

	public class StringConcat : Expression
	{
		public Arguments arguments;

		StringConcat(Location loc)
		{
			this.Location = loc;
			arguments = new Arguments(2);
		}
	}

	public class Conditional : Expression
	{
		Expression expr, true_expr, false_expr;

		public Conditional(Expression expr, Expression true_expr, Expression false_expr, Location loc)
		{
			this.expr = expr;
			this.true_expr = true_expr;
			this.false_expr = false_expr;
			this.Location = loc;
		}
	}

	public class ElementAccess : Expression
	{
		public Arguments Arguments;
		public Expression Expr;

		public ElementAccess(Expression e, Arguments args, Location loc)
		{
			Expr = e;
			this.Location = loc;
			this.Arguments = args;
		}
	}

	public abstract class Constant : Expression
	{
		protected Constant(Location loc)
		{
			this.Location = loc;
		}

		public abstract object GetValue();

		public abstract long GetValueAsLong();

		public abstract string GetValueAsLiteral();
	}

	public class StringConstant : Constant
	{
		public string Value;

		public StringConstant(string s, Location loc)
			: this(BuildinTypeSpec.String, s, loc)
		{
		}

		public StringConstant(TypeSpec type, string s, Location loc)
			: base(loc)
		{
			this.Type = type;

			Value = s;
		}

		protected StringConstant(Location loc)
			: base(loc)
		{
		}
		public override object GetValue()
		{
			return Value;
		}
		public override string GetValueAsLiteral()
		{
			return "\"" + Value + "\"";
		}
		public override long GetValueAsLong()
		{
			throw new NotSupportedException();
		}
	}

	class NameOf : StringConstant
	{
		public SimpleName name;

		public NameOf(SimpleName name)
			: base(name.Location)
		{
			this.name = name;
		}
	}
	public class StringLiteral : StringConstant
	{
		public StringLiteral(string s, Location loc)
			: base(s, loc)
		{
		}
	}

	public class NullConstant : Constant
	{
		public NullConstant(TypeSpec type, Location loc)
			: base(loc)
		{
			this.Type = type;
		}

		public override string ToString()
		{
			return "null";
		}

		public override object GetValue()
		{
			return null;
		}

		public override string GetValueAsLiteral()
		{
			return "null";
		}

		public override long GetValueAsLong()
		{
			throw new NotSupportedException();
		}
	}

	public class EnumConstant : Constant
	{
		public Constant Child;

		public EnumConstant(Constant child, TypeSpec enum_type)
			: base(child.Location)
		{
			this.Child = child;

			this.Type = enum_type;
		}

		protected EnumConstant(Location loc)
			: base(loc)
		{
		}

		public override object GetValue()
		{
			return Child.GetValue();
		}

		public override string GetValueAsLiteral()
		{
			return Child.GetValueAsLiteral();
		}

		public override long GetValueAsLong()
		{
			return Child.GetValueAsLong();
		}
	}

	public abstract class IntegralConstant : Constant
	{
		protected IntegralConstant(TypeSpec type, Location loc)
			: base(loc)
		{
			this.Type = type;
		}

		public override string GetValueAsLiteral()
		{
			return GetValue().ToString();
		}
	}

	public class ByteConstant : IntegralConstant
	{
		public byte Value;

		public ByteConstant(byte v, Location loc)
			: this(BuildinTypeSpec.Byte, v, loc)
		{
		}

		public ByteConstant(TypeSpec type, byte v, Location loc)
			: base(type, loc)
		{
			Value = v;
		}

		public override object GetValue()
		{
			return Value;
		}

		public override long GetValueAsLong()
		{
			return Value;
		}
	}

	public class SByteConstant : IntegralConstant
	{
		public sbyte Value;

		public SByteConstant(sbyte v, Location loc)
			: this(BuildinTypeSpec.SByte, v, loc)
		{
		}

		public SByteConstant(TypeSpec type, sbyte v, Location loc)
			: base(type, loc)
		{
			Value = v;
		}

		public override object GetValue()
		{
			return Value;
		}

		public override long GetValueAsLong()
		{
			return Value;
		}
	}
	public class ShortConstant : IntegralConstant
	{
		public short Value;

		public ShortConstant(short v, Location loc)
			: this(BuildinTypeSpec.Short, v, loc)
		{
		}

		public ShortConstant(TypeSpec type, short v, Location loc)
			: base(type, loc)
		{
			Value = v;
		}

		public override object GetValue()
		{
			return Value;
		}

		public override long GetValueAsLong()
		{
			return Value;
		}
	}
	public class UShortConstant : IntegralConstant
	{
		public ushort Value;

		public UShortConstant(ushort v, Location loc)
			: this(BuildinTypeSpec.UShort, v, loc)
		{
		}

		public UShortConstant(TypeSpec type, ushort v, Location loc)
			: base(type, loc)
		{
			Value = v;
		}

		public override object GetValue()
		{
			return Value;
		}

		public override long GetValueAsLong()
		{
			return Value;
		}
	}
	public class IntConstant : IntegralConstant
	{
		public int Value;

		public IntConstant(int v, Location loc)
			: this(BuildinTypeSpec.Int, v, loc)
		{
		}

		public IntConstant(TypeSpec type, int v, Location loc)
			: base(type, loc)
		{
			Value = v;
		}

		public override object GetValue()
		{
			return Value;
		}

		public override long GetValueAsLong()
		{
			return Value;
		}
	}
	public class IntLiteral : IntConstant
	{
		public IntLiteral(int l, Location loc)
			: base(l, loc)
		{
		}
	}

	public class UIntConstant : IntegralConstant
	{
		public uint Value;

		public UIntConstant(uint v, Location loc)
			: this(BuildinTypeSpec.UInt, v, loc)
		{
		}

		public UIntConstant(TypeSpec type, uint v, Location loc)
			: base(type, loc)
		{
			Value = v;
		}

		public override object GetValue()
		{
			return Value;
		}

		public override long GetValueAsLong()
		{
			return Value;
		}
	}
	public class UIntLiteral : UIntConstant
	{
		public UIntLiteral(uint l, Location loc)
			: base(l, loc)
		{
		}
	}

	public class LongConstant : IntegralConstant
	{
		public long Value;

		public LongConstant(long v, Location loc)
			: this(BuildinTypeSpec.Long, v, loc)
		{
		}

		public LongConstant(TypeSpec type, long v, Location loc)
			: base(type, loc)
		{
			Value = v;
		}

		public override object GetValue()
		{
			return Value;
		}

		public override long GetValueAsLong()
		{
			return Value;
		}
	}
	public class LongLiteral : LongConstant
	{
		public LongLiteral(long l, Location loc)
			: base(l, loc)
		{
		}
	}
	public class ULongConstant : IntegralConstant
	{
		public ulong Value;

		public ULongConstant(ulong v, Location loc)
			: this(BuildinTypeSpec.Long, v, loc)
		{
		}

		public ULongConstant(TypeSpec type, ulong v, Location loc)
			: base(type, loc)
		{
			Value = v;
		}

		public override object GetValue()
		{
			return Value;
		}

		public override long GetValueAsLong()
		{
			return (long)Value;
		}
	}
	public class ULongLiteral : ULongConstant
	{
		public ULongLiteral(ulong l, Location loc)
			: base(l, loc)
		{
		}
	}

	public class BoolConstant : Constant
	{
		public bool Value;

		public BoolConstant(bool val, Location loc)
			: this(BuildinTypeSpec.Bool, val, loc)
		{
		}

		public BoolConstant(TypeSpec type, bool val, Location loc)
			: base(loc)
		{
			this.Type = type;

			Value = val;
		}

		public override object GetValue()
		{
			return (object)Value;
		}

		public override string GetValueAsLiteral()
		{
			return Value ? "true" : "false";
		}

		public override long GetValueAsLong()
		{
			return Value ? 1 : 0;
		}
	}

	public class BoolLiteral : BoolConstant
	{
		public BoolLiteral(bool val, Location loc)
			: base(val, loc)
		{
		}
	}

	public class CharConstant : Constant
	{
		public char Value;

		public CharConstant(char v, Location loc)
			: this(BuildinTypeSpec.Char, v, loc)
		{
		}

		public CharConstant(TypeSpec type, char v, Location loc)
			: base(loc)
		{
			this.Type = type;

			Value = v;
		}

		static string descape(char c)
		{
			switch (c)
			{
				case '\a':
					return "\\a";
				case '\b':
					return "\\b";
				case '\n':
					return "\\n";
				case '\t':
					return "\\t";
				case '\v':
					return "\\v";
				case '\r':
					return "\\r";
				case '\\':
					return "\\\\";
				case '\f':
					return "\\f";
				case '\0':
					return "\\0";
				case '"':
					return "\\\"";
				case '\'':
					return "\\\'";
			}
			return c.ToString();
		}

		public override object GetValue()
		{
			return Value;
		}

		public override long GetValueAsLong()
		{
			return Value;
		}

		public override string GetValueAsLiteral()
		{
			return "\"" + descape(Value) + "\"";
		}
	}


	public class CharLiteral : CharConstant
	{
		public CharLiteral(char val, Location loc)
			: base(val, loc)
		{
		}
	}

	public class FloatConstant : Constant
	{
		public double DoubleValue;
		public float Value => (float)DoubleValue;

		public FloatConstant(double v, Location loc)
			: this(BuildinTypeSpec.Float, v, loc)
		{
		}

		public FloatConstant(TypeSpec type, double v, Location loc)
			: base(loc)
		{
			this.Type = type;

			DoubleValue = v;
		}
		public override object GetValue()
		{
			return Value;
		}

		public override string GetValueAsLiteral()
		{
			return Value.ToString();
		}

		public override long GetValueAsLong()
		{
			throw new NotSupportedException();
		}
	}

	public class FloatLiteral : FloatConstant
	{
		public FloatLiteral(float val, Location loc)
			: base(val, loc)
		{
		}
	}

	public class DoubleConstant : Constant
	{
		public double Value;

		public DoubleConstant(double v, Location loc)
			: this(BuildinTypeSpec.Double, v, loc)
		{
		}

		public DoubleConstant(TypeSpec type, double v, Location loc)
			: base(loc)
		{
			this.Type = type;

			Value = v;
		}

		public override object GetValue()
		{
			return Value;
		}

		public override string GetValueAsLiteral()
		{
			return Value.ToString();
		}

		public override long GetValueAsLong()
		{
			throw new NotSupportedException();
		}
	}

	public class DoubleLiteral : DoubleConstant
	{
		public DoubleLiteral(double val, Location loc)
			: base(val, loc)
		{
		}
	}

	public class ErrorExpression : Expression
	{
		public static readonly ErrorExpression Instance = new ErrorExpression();

		private ErrorExpression()
		{
			Type = InternalType.ErrorType;
			Location = Location.Null;
		}
	}

	public abstract class VariableReference : Expression
	{
		public abstract string Name { get; }
	}

	//public class LocalVariableReference : VariableReference
	//{
	//	public LocalVariable local_info;

	//	public LocalVariableReference(LocalVariable li, Location l)
	//	{
	//		this.local_info = li;
	//		Location = l;
	//	}

	//	public override string Name => local_info.name;
	//}

	public class ParameterReference : VariableReference
	{
		public ParameterSpec pi;

		public ParameterReference(ParameterSpec pi, Location loc)
		{
			this.pi = pi;
			this.Location = loc;
		}

		public override string Name => pi.name;
	}

	public class This : VariableReference
	{
		public This(Location loc)
		{
			this.Location = loc;
		}

		public override string Name => "this";
	}

	public class BaseThis : This
	{
		public BaseThis(Location loc)
			: base (loc)
		{
		}

		public override string Name => "base";
	}

	public class Unary : Expression
	{
		public enum Operator : byte
		{
			UnaryPlus,
			UnaryNegation,
			LogicalNot,
			UnaryNot,
			AddressOf,
			TOP
		}
		public readonly Operator Oper;
		public Expression Expr;

		public Unary(Operator op, Expression expr, Location loc)
		{
			Oper = op;
			Expr = expr;
			this.Location = loc;
		}
	}

	public abstract class MemberExpr : Expression
	{
		public Expression InstanceExpression;

		public abstract string Name { get; }

	}

	public class MethodGroupExpr : MemberExpr
	{
		public List<MemberSpec> Methods;
		public MethodSpec best_candidate;
		public TypeArguments type_arguments;

		public MethodGroupExpr(List<MemberSpec> mi, TypeSpec type, Location loc)
		{
			Methods = mi;
			this.Location = loc;
			this.Type = InternalType.MethodGroup;
		}

		public MethodGroupExpr(MethodSpec m, TypeSpec type, Location loc)
			: this(new List<MemberSpec> { m }, type, loc)
		{
		}

		public override string Name
		{
			get
			{
				if (best_candidate != null)
					return best_candidate.name;

				// TODO: throw ?
				return Methods[0].name;
			}
		}
	}

	public class FieldExpr : MemberExpr
	{
		public FieldSpec spec;

		public FieldExpr(FieldSpec spec, Location loc)
		{
			this.spec = spec;
			this.Location = loc;

			Type = spec.field_type;
		}

		public override string Name => spec.name;
	}

	public abstract class Probe : Expression
	{
		public Expression ProbeType;
		public Expression expr;
		public TypeSpec probe_type_expr;

		protected Probe(Expression expr, Expression probe_type, Location l)
		{
			ProbeType = probe_type;
			Location = l;
			this.expr = expr;
		}
	}

	public class Is : Probe
	{
		public Is(Expression expr, Expression probe_type, Location l)
			: base(expr, probe_type, l)
		{
		}
	}

	public class As : Probe
	{
		public As(Expression expr, Expression probe_type, Location l)
			: base(expr, probe_type, l)
		{
		}
	}

	public class Default : Expression
	{
		public Default(Location l)
		{
			Type = BuildinTypeSpec.Void;
			Location = l;
		}
	}
}
