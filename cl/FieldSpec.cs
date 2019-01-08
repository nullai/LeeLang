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
		public IR_Value ir_value;
		public FieldSpec(string name, TypeSpec type, NamespaceSpec declare)
			: base(name, declare)
		{
			field_type = type;
		}
		public override void Resolve(ResolveContext ctx)
		{
			if (field_type.IsStruct)
				field_type.DoResolve(ctx);
		}
		public IR_Value GetIRValue()
		{
			if (ir_value == null)
				ir_value = IR_Value.CreateAlloc(field_type.SizeOf);
			return ir_value;
		}
	}
}
