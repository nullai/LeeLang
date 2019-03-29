using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	[Flags]
	public enum MemberKind
	{
		Constructor = 1,
		Field = 1 << 2,
		Method = 1 << 3,
		Property = 1 << 4,
		Indexer = 1 << 5,
		Operator = 1 << 6,
		Destructor = 1 << 7,

		Class = 1 << 11,
		Struct = 1 << 12,
		Delegate = 1 << 13,
		Enum = 1 << 14,
		Interface = 1 << 15,
		TypeParameter = 1 << 16,
		ByRef = 1 << 17,

		ArrayType = 1 << 19,
		PointerType = 1 << 20,
		InternalCompilerType = 1 << 21,
		MissingType = 1 << 22,
		Void = 1 << 23,
		Namespace = 1 << 24,

	}

	public class MemberSpec
	{
		public CommonAttribute attr;
		public string name;
		public TypeSpec declaringType;


		public MemberSpec(string name, TypeSpec declare)
		{
			this.name = name;
			this.declaringType = declare;
		}
	}
}
