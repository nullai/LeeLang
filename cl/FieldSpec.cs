using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class FieldSpec : MemberSpec
	{
		public TypeSpec field_type;
		public Expression value;
		public bool is_enum_field;
		public long enum_field_value;
		public FieldSpec(string name, TypeSpec type, NamespaceSpec declare)
			: base(name, declare)
		{
			field_type = type;
		}
	}
}
