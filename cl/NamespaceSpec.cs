using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class NamespaceSpec : MemberSpec
	{
		public string fullname;
		public Dictionary<string, List<MemberSpec>> members = new Dictionary<string, List<MemberSpec>>();
		public List<NamespaceSpec> usings = null;

		public NamespaceSpec(string name, NamespaceSpec declare)
			: base(name, declare)
		{
			fullname = (declare == null || declare is FileSpec) ? name : declare.fullname + "." + name;
		}
		public virtual void AddMember(MemberSpec member, Compiler compiler)
		{
			bool is_ns = member is NamespaceSpec;
			List<MemberSpec> vals;
			if (members.TryGetValue(member.name, out vals))
			{
				for (int i = 0; i < vals.Count; i++)
				{
					var item = vals[i];
					if (item.name == member.name && item.Arity == member.Arity && item.prefix == member.prefix && (item is NamespaceSpec) == is_ns)
					{
						compiler.OutputError(string.Format("已经存在一个同名项\"{0}\"。", member));
						return;
					}
				}
				vals.Add(member);
			}
			else
			{
				vals = new List<MemberSpec>() { member };
				members.Add(member.name, vals);
			}
		}
		public void AddUsing(NamespaceSpec ns)
		{
			if (usings == null)
			{
				usings = new List<NamespaceSpec>() { ns };
				return;
			}
			if (!usings.Contains(ns))
				usings.Add(ns);
		}
		public override List<MemberSpec> ResolveName(string name)
		{
			if (resolving)
				return null;
			resolving = true;

			List<MemberSpec> r;
			if (members.TryGetValue(name, out r))
			{
				for (int i = 0; i < r.Count; i++)
				{
					var item = r[i];
					if (item.name == name && item.prefix == null)
					{
						resolving = false;
						r.Clear();
						r.Add(item);
						return r;
					}
				}
			}

			if (usings != null)
			{
				for (int i = 0; i < usings.Count; i++)
				{
					var t = usings[i].ResolveName(name);
					if (t != null)
					{
						if (r != null)
							r.AddRange(t);
						else
							r = t;
					}
				}
			}

			if (r == null && Declaring != null)
				r = Declaring.ResolveName(name);

			resolving = false;
			return r;
		}

		public static NamespaceSpec CreateFromName(Expression expr, NamespaceSpec declare)
		{
			AccessExpression a = expr as AccessExpression;
			if (a != null)
			{
				NamespaceSpec left = CreateFromName(a.left, declare);
				return new NamespaceSpec(a.name.value, left);
			}

			return new NamespaceSpec((expr as NameExpression).token.value, declare);
		}
		public override void Resolve(ResolveContext ctx)
		{
			var scope = ctx.scope;
			ctx.scope = this;

			foreach(var ps in members.Values)
			{
				for (int i = 0; i < ps.Count; i++)
					ps[i].DoResolve(ctx);
			}

			ctx.scope = scope;
		}
		public override void CodeGen(CodeGenContext ctx)
		{
			var scope = ctx.scope;
			ctx.scope = this;

			foreach (var ps in members.Values)
			{
				for (int i = 0; i < ps.Count; i++)
					ps[i].CodeGen(ctx);
			}
			ctx.scope = scope;
		}
	}

	public class UsingSpec : MemberSpec
	{
		public NamespaceSpec value;
		public UsingSpec(string name, NamespaceSpec value, NamespaceSpec declare)
			: base(name, declare)
		{
			this.value = value;
		}
	}
}
