using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class PropertySpec : MemberSpec
	{
		public TypeSpec property_type;
		public Dictionary<string, MethodSpec> methods = new Dictionary<string, MethodSpec>();
		public PropertySpec(string name, TypeSpec type, NamespaceSpec declare)
			: base(name, declare)
		{
			property_type = type;
		}
	}
}
