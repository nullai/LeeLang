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
		public virtual void ResolveType(ResolveContext ctx)
		{
		}
		public virtual void ResolveUsing(ResolveContext ctx)
		{
		}
		public virtual void ResolveMember(ResolveContext ctx)
		{
		}
		public virtual void CodeGen(CodeGenContext ctx)
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

		public override void CodeGen(CodeGenContext ctx)
		{
			for (int i = 0; i < values.Count; i++)
			{
				values[i].CodeGen(ctx);
			}
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
		public override void ResolveMember(ResolveContext ctx)
		{
			var scope = ctx.scope;
			ctx.scope = file_spec;

			for (int i = 0; i < members.Count; i++)
			{
				members[i].ResolveMember(ctx);
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
		public override void ResolveMember(ResolveContext ctx)
		{
			var scope = ctx.scope;
			ctx.scope = ns_spec;

			for (int i = 0; i < members.Count; i++)
			{
				members[i].ResolveMember(ctx);
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
		public override void ResolveMember(ResolveContext ctx)
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
				members[i].ResolveMember(ctx);
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
		public override void ResolveMember(ResolveContext ctx)
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
				next.ResolveMember(ctx);
		}
		public override void CodeGen(CodeGenContext ctx)
		{
			ctx.block.Add((IR_Instruction)field_spec.GetIRValue());
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
		public override void ResolveMember(ResolveContext ctx)
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
		public override void ResolveMember(ResolveContext ctx)
		{
			var scope = ctx.scope;
			property_type = type.ResolveToType(ctx);
			if (property_type == null)
				return;

			if (value.Count == 0)
			{
				ctx.complier.OutputError(string.Format("没有为属性\"{0}\"定义方法。", this));
				return;
			}

			property_spec = new PropertySpec(name.token.value, property_type, scope);
			property_spec.attr = attr;
			scope.AddMember(property_spec, ctx.complier);

			ParameterStatement pval = null;
			for (int i = 0; i < value.Count; i++)
			{
				var v = value[i];
				switch (v.name.token.value)
				{
					case "get":
						v.name.token.value = "get_" + name.token.value;
						v.parameters = parameters;
						v.type = type;
						break;
					case "set":
					case "add":
					case "remove":
						v.name.token.value += "_" + name.token.value;
						v.parameters = new List<ParameterStatement>();
						if (pval == null)
							pval = new ParameterStatement(ParameterAttribute.NONE, type, "value");
						if (parameters != null)
							v.parameters.AddRange(parameters);
						v.parameters.Add(pval);
						v.type = NameExpression.Void;
						break;
					default:
						ctx.complier.OutputError(string.Format("无效的属性方法名\"{0}\"。", v));
						continue;
				}

				v.ResolveMember(ctx);
			}
		}
	}

	public class ParameterStatement : Statement
	{
		public ParameterAttribute attr;
		public Expression type;
		public string name;
		public Expression value;

		public ParameterStatement(ParameterAttribute attr, Expression type, string name)
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
		public override void ResolveMember(ResolveContext ctx)
		{
			var scope = ctx.scope;
			return_type = type.ResolveToType(ctx);
			if (return_type == null)
				return;

			method_spec = new MethodSpec(name.token.value, return_type, scope);
			int ps_count = 0;
			bool have_this = false;
			if ((attr & CommonAttribute.STATIC) != CommonAttribute.STATIC && scope is TypeSpec)
			{
				have_this = true;
				ps_count = 1;
			}
			if (parameters != null)
				ps_count += parameters.Count;
			if (ps_count > 0)
				method_spec.parameters = new ParameterSpec[ps_count];

			int idx = 0;
			if (have_this)
			{
				ParameterSpec pthis = new ParameterSpec();
				pthis.type = scope as TypeSpec;
				pthis.name = "this";
				method_spec.parameters[0] = pthis;
				idx = 1;
			}

			if (parameters != null)
			{
				for (int i = 0; i < parameters.Count; i++)
				{
					ParameterSpec p = new ParameterSpec();
					p.attr = parameters[i].attr;
					p.type = parameters[i].type.ResolveToType(ctx);
					p.name = parameters[i].name;
					method_spec.parameters[i + idx] = p;
				}
			}
			method_spec.body = body;
			scope.AddMember(method_spec, ctx.complier);
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
		public override void CodeGen(CodeGenContext ctx)
		{
			var v = cond.CodeGen(ctx);

			IR_BaseBlock t = new IR_BaseBlock(ctx.method);
			IR_BaseBlock f = new IR_BaseBlock(ctx.method);

			ctx.block.LinkTo(t);
			ctx.block.LinkTo(f);

			ctx.block.Add(IR_Value.CreateBr(v, t, f));

			ctx.block = t;
			bodyt.CodeGen(ctx);

			if (bodyf != null)
			{
				ctx.block = f;
				bodyf.CodeGen(ctx);

				t = new IR_BaseBlock(ctx.method);
				f.Add(IR_Value.CreateBr(null, t, null));
				f.LinkTo(t);
			}
			ctx.block = t;
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

		public override void CodeGen(CodeGenContext ctx)
		{
			if (init != null)
				init.CodeGen(ctx);

			IR_BaseBlock b = new IR_BaseBlock(ctx.method);
			IR_BaseBlock e = new IR_BaseBlock(ctx.method);
			if (cond != null)
			{
				var v = cond.CodeGen(ctx);
				ctx.block.Add(IR_Value.CreateBr(v, b, e));
				ctx.block.LinkTo(e);
			}
			else
			{
				ctx.block.Add(IR_Value.CreateBr(null, b, null));
			}
			ctx.block = b;
			ctx.block.LinkTo(b);

			body.CodeGen(ctx);

			if (cond != null)
			{
				var v = cond.CodeGen(ctx);
				ctx.block.Add(IR_Value.CreateBr(v, b, e));
				ctx.block.LinkTo(b);
				ctx.block.LinkTo(e);
			}
			else
			{
				ctx.block.Add(IR_Value.CreateBr(null, b, null));
				ctx.block.LinkTo(b);
			}

			ctx.block = e;
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
		public override void CodeGen(CodeGenContext ctx)
		{
			throw new Exception("不支持");
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
		public override void CodeGen(CodeGenContext ctx)
		{
			throw new Exception("不支持");
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
		public override void CodeGen(CodeGenContext ctx)
		{
			ctx.block.Add(IR_Value.CreateReturn(null));
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

