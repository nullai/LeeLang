using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class MemberSpec
	{
		public bool resolving = false;
		public CommonAttribute attr;
		public string prefix;
		public string name;
		public NamespaceSpec Declaring;
		public virtual int Arity => 0;

		public bool IsStatic => (attr & CommonAttribute.STATIC) != CommonAttribute.NONE;


		public MemberSpec(string name, NamespaceSpec declare)
		{
			this.name = name;
			this.Declaring = declare;
		}

		public virtual List<MemberSpec> ResolveName(string name)
		{
			throw new Exception("ResolveName In " + GetType().Name);
		}

		public virtual void Resolve(ResolveContext ctx)
		{
		}
		public void DoResolve(ResolveContext ctx)
		{
			if (resolving)
			{
				ctx.complier.OutputError("存在循环引用！");
				return;
			}

			resolving = true;
			Resolve(ctx);
			resolving = false;
		}
		public virtual void CodeGen(CodeGenContext ctx)
		{
		}
	}
}
