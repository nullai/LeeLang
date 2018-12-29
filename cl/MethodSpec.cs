using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class MethodSpec : MemberSpec
	{
		public TypeSpec return_type;
		public MethodSpec(string name, TypeSpec type, NamespaceSpec declare)
			: base(name, declare)
		{
			return_type = type;
		}
	}
}
