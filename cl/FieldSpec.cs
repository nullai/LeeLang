using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class FieldSpec : MemberSpec
	{
		public FieldSpec(string name, NamespaceSpec declare)
			: base(name, declare)
		{
		}
	}
}
