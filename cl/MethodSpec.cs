using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class ParameterSpec
	{
		public ParameterAttribute attr;
		public TypeSpec type;
		public string name;
	}
	public class MethodSpec : MemberSpec
	{
		public TypeSpec return_type;
		public ParameterSpec[] parameters;
		public MethodSpec(string name, TypeSpec type, TypeSpec declare)
			: base(name, declare)
		{
			return_type = type;
		}
	}
}
