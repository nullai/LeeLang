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
		public virtual void ResolveType(ResolveContext ctx)
		{
		}
		public virtual void ResolveUsing(ResolveContext ctx)
		{
		}
		public virtual void Resolve(ResolveContext ctx)
		{
		}
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
		public BlockSpec block_spec;

		public override void Resolve(ResolveContext ctx)
		{
			var scope = ctx.scope;
			block_spec = new BlockSpec(ctx.scope);

			ctx.scope = block_spec;

			for (int i = 0; i < values.Count; i++)
			{
				values[i].Resolve(ctx);
			}
			ctx.scope = scope;
		}
	}

	public class FileStatement : Statement
	{
		public string file_name;
		public List<Statement> members = new List<Statement>();
		public FileSpec file_spec;

		public FileStatement(string file_name)
		{
			this.file_name = file_name;
		}
		public override void ResolveType(ResolveContext ctx)
		{
			file_spec = new FileSpec(file_name, ctx.scope as ModuleSpec);
			ctx.scope.AddMember(file_spec, ctx.complier);
			var scope = ctx.scope;
			ctx.scope = file_spec;
			for (int i = 0; i < members.Count; i++)
			{
				members[i].ResolveType(ctx);
			}
			ctx.scope = scope;
		}
		public override void ResolveUsing(ResolveContext ctx)
		{
			var scope = ctx.scope;
			ctx.scope = file_spec;
			for (int i = 0; i < members.Count; i++)
			{
				members[i].ResolveUsing(ctx);
			}
			ctx.scope = scope;
		}
		public override void Resolve(ResolveContext ctx)
		{
			var scope = ctx.scope;
			ctx.scope = file_spec;

			for (int i = 0; i < members.Count; i++)
			{
				members[i].Resolve(ctx);
			}
			ctx.scope = scope;
		}
	}

	public class UsingStatement : Statement
	{
		public NameExpression name;
		public Expression value;
		public NamespaceSpec ns_spec;

		public UsingStatement(NameExpression name, Expression value)
		{
			this.name = name;
			this.value = value;
		}
		public override void ResolveUsing(ResolveContext ctx)
		{
			var val = value.ResolveType(ctx);
			if (val == null)
				return;

			ns_spec = null;
			for (int i = 0; i < val.Count; i++)
			{
				NamespaceSpec p = val[i] as NamespaceSpec;
				if (p != null)
				{
					if (ns_spec != null)
						ctx.complier.OutputError(string.Format("不能确定是\"{0}\"或\"{1}\"。", ns_spec, p));
					else
						ns_spec = p;
				}
			}
			if (ns_spec == null)
			{
				ctx.complier.OutputError(string.Format("未能找到类型或命名空间\"{0}\"(是否缺少 using 指令或程序集引用?)", value));
				return;
			}

			if (name == null)
				ctx.scope.AddUsing(ns_spec);
			else
				ctx.scope.AddMember(new UsingSpec(name.token.value, ns_spec, ctx.scope), ctx.complier);
		}
	}

	public class NamespaceStatement : Statement
	{
		public Expression name;
		public List<Statement> members = new List<Statement>();
		public NamespaceSpec ns_spec;

		public NamespaceStatement(Expression name)
		{
			this.name = name;
		}
		public override void ResolveType(ResolveContext ctx)
		{
			var scope = ctx.scope;
			ns_spec = NamespaceSpec.CreateFromName(name, scope);
			scope.AddMember(ns_spec, ctx.complier);
			ctx.scope = ns_spec;
			for (int i = 0; i < members.Count; i++)
			{
				members[i].ResolveType(ctx);
			}
			ctx.scope = scope;
		}
		public override void ResolveUsing(ResolveContext ctx)
		{
			var scope = ctx.scope;
			ctx.scope = ns_spec;
			for (int i = 0; i < members.Count; i++)
			{
				members[i].ResolveUsing(ctx);
			}
			ctx.scope = scope;
		}
		public override void Resolve(ResolveContext ctx)
		{
			var scope = ctx.scope;
			ctx.scope = ns_spec;

			for (int i = 0; i < members.Count; i++)
			{
				members[i].Resolve(ctx);
			}
			ctx.scope = scope;
		}
	}

	public class TypeStatement : Statement
	{
		public Token mToken;
		public CommonAttribute attr;
		public NameExpression name;
		public List<Expression> base_type;
		public List<TokenValue> generics = null;
		public List<Statement> members = new List<Statement>();
		public TypeSpec type_spec;

		public TypeStatement(CommonAttribute attr, Token token, NameExpression name)
		{
			this.mToken = token;
			this.attr = attr;
			this.name = name;
		}

		public override void ResolveType(ResolveContext ctx)
		{
			var scope = ctx.scope;
			switch (mToken)
			{
				case Token.CLASS:
					type_spec = new ClassStructSpec(attr, name.token.value, scope, false, false);
					break;
				case Token.STRUCT:
					type_spec = new ClassStructSpec(attr, name.token.value, scope, false, true);
					break;
				case Token.INTERFACE:
					type_spec = new ClassStructSpec(attr, name.token.value, scope, true, false);
					break;
				case Token.ENUM:
					type_spec = new EnumSpec(attr, name.token.value, scope);
					break;
				default:
					return;
			}
			if (generics != null)
			{
				ClassStructSpec cls = type_spec as ClassStructSpec;
				if (cls == null)
					ctx.complier.OutputError(string.Format("枚举\"{0}\"不支持泛型。", type_spec));
				else
				{
					cls.SetArity(generics.Count);
					for (int i = 0; i < generics.Count; i++)
					{
						cls.generic_paramters[i] = new GenericParamterSpec(cls, generics[i].value);
					}
				}
			}
			scope.AddMember(type_spec, ctx.complier);
			ctx.scope = type_spec;
			for (int i = 0; i < members.Count; i++)
			{
				members[i].ResolveType(ctx);
			}
			ctx.scope = scope;
		}
		public override void ResolveUsing(ResolveContext ctx)
		{
			var scope = ctx.scope;
			ctx.scope = type_spec;
			for (int i = 0; i < members.Count; i++)
			{
				members[i].ResolveUsing(ctx);
			}
			ctx.scope = scope;
		}
		public override void Resolve(ResolveContext ctx)
		{
			bool error = false;
			if (base_type != null)
			{
				for (int i = 0; i < base_type.Count; i++)
				{
					var type = base_type[i].ResolveType(ctx);
					if (type == null)
					{
						error = true;
						continue;
					}

					bool resolved = false;
					for (int j = 0; j < type.Count; j++)
					{
						var t = type[j] as TypeSpec;
						if (t == null)
							continue;
						resolved = true;
						if (t.IsInterface)
						{
							if (!type_spec.AddInterface(t))
								ctx.complier.OutputError(string.Format("继承的接口\"{0}\"已经存在。", t));
							continue;
						}
						if (mToken == Token.ENUM)
						{
							if (t != BuildinTypeSpec.SByte && t != BuildinTypeSpec.Byte && t != BuildinTypeSpec.Short && t != BuildinTypeSpec.UShort && t != BuildinTypeSpec.Int && t != BuildinTypeSpec.UInt)
								ctx.complier.OutputError(string.Format("枚举不能继承自类型\"{0}\"。", t));
							else if (type_spec.base_type != null)
								ctx.complier.OutputError(string.Format("已经继承了类型\"{0}\"，不能再继承类型\"{1}\"已经存在。", type_spec.base_type, t));
							else
								type_spec.base_type = t;
							continue;
						}
						if (mToken == Token.CLASS && t.IsClass)
						{
							if (type_spec.base_type != null)
								ctx.complier.OutputError(string.Format("已经继承了类型\"{0}\"，不能再继承类型\"{1}\"已经存在。", type_spec.base_type, t));
							else
								type_spec.base_type = t;
							continue;
						}
						ctx.complier.OutputError(string.Format("不能继承自类型\"{0}\"。", t));
					}
					if (!resolved)
					{
						ctx.complier.OutputError(string.Format("未能找到类型名\"{0}\"(是否缺少 using 指令或程序集引用?)", base_type[i]));
						error = true;
					}
				}
			}
			if (error)
				return;

			if (type_spec.base_type == null)
			{
				if (mToken == Token.CLASS || mToken == Token.STRUCT)
					type_spec.base_type = BuildinTypeSpec.Object;
				else if (mToken == Token.ENUM)
					type_spec.base_type = BuildinTypeSpec.Int;
			}

			var scope = ctx.scope;
			ctx.scope = type_spec;

			for (int i = 0; i < members.Count; i++)
			{
				members[i].Resolve(ctx);
			}
			ctx.scope = scope;
		}
	}

	public class FieldStatement : Statement
	{
		public CommonAttribute attr;
		public Expression type;
		public NameExpression name;
		public Expression value;
		public FieldStatement next;
		public TypeSpec field_type;
		public FieldSpec field_spec;

		public FieldStatement(CommonAttribute attr, Expression type, NameExpression name)
		{
			this.attr = attr;
			this.type = type;
			this.name = name;
		}
		public override void Resolve(ResolveContext ctx)
		{
			var scope = ctx.scope;
			field_type = type.ResolveToType(ctx);
			if (field_type == null)
				return;

			field_spec = new FieldSpec(name.token.value, field_type, scope);
			field_spec.attr = attr;
			field_spec.value = value;
			scope.AddMember(field_spec, ctx.complier);

			if (next != null)
				next.Resolve(ctx);
		}
	}

	public class EnumFieldStatement : Statement
	{
		public NameExpression name;
		public Expression value;
		public FieldSpec field_spec;

		public EnumFieldStatement(NameExpression name, Expression value)
		{
			this.name = name;
			this.value = value;
		}
		public override void Resolve(ResolveContext ctx)
		{
			var scope = ctx.scope;
			field_spec = new FieldSpec(name.token.value, scope as TypeSpec, scope);
			field_spec.attr = CommonAttribute.PUBLIC | CommonAttribute.CONST;
			field_spec.value = value;
			field_spec.is_enum_field = true;
			scope.AddMember(field_spec, ctx.complier);
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
		public TypeSpec property_type;
		public PropertySpec property_spec;

		public PropertyStatement(CommonAttribute attr, Expression type, NameExpression inter, NameExpression name)
		{
			this.attr = attr;
			this.type = type;
			this.inter = inter;
			this.name = name;
		}
		public override void Resolve(ResolveContext ctx)
		{
			var scope = ctx.scope;
			property_type = type.ResolveToType(ctx);
			if (property_type == null)
				return;

			property_spec = new PropertySpec(name.token.value, property_type, scope);
			property_spec.attr = attr;
			scope.AddMember(property_spec, ctx.complier);
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
		public TypeSpec return_type;
		public MethodSpec method_spec;

		public MethodStatement(CommonAttribute attr, Expression type, NameExpression inter, NameExpression name)
		{
			this.attr = attr;
			this.type = type;
			this.inter = inter;
			this.name = name;
		}
		public override void Resolve(ResolveContext ctx)
		{
			var scope = ctx.scope;
			return_type = type.ResolveToType(ctx);
			if (return_type == null)
				return;
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

