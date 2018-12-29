using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class MemberSpec
	{
		public CommonAttribute attr;
		public string prefix;
		public string name;
		public NamespaceSpec Declaring;
		public virtual int Arity => 0;

		public delegate bool VerifyMember(MemberSpec member);

		public MemberSpec(string name, NamespaceSpec declare)
		{
			this.name = name;
			this.Declaring = declare;
		}

		public virtual List<MemberSpec> ResolveName(string name)
		{
			throw new Exception("ResolveName In " + GetType().Name);
		}
	}
}
