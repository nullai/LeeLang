using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class TypeSpec : MemberSpec
	{
		public TypeSpec base_type = null;
		public List<TypeSpec> interface_types = null;
		protected TypeSpec ArrayType;
		protected TypeSpec PointerType;
		public virtual bool IsArray => false;
		public virtual bool IsPointer => false;
		public virtual bool IsReference => false;
		public virtual bool IsGeneric => false;
		public virtual bool IsStruct => false;
		public virtual bool IsClass => false;
		public virtual bool IsInterface => false;
		public virtual bool IsEnum => false;
		public virtual TypeSpec ElementType => null;
		public virtual int SizeOf => 0;

		public TypeSpec(string name, TypeSpec declare)
			: base(name, declare)
		{
		}
		public TypeSpec MakeArray()
		{
			if (ArrayType == null)
				ArrayType = new ArraySpec(this);
			return ArrayType;
		}
		public TypeSpec MakePointer()
		{
			if (PointerType == null)
				PointerType = new PointerSpec(this);
			return PointerType;
		}

		public static bool CompareTypes(TypeSpec[] a, TypeSpec[] b)
		{
			if (a.Length != b.Length)
				return false;

			for (int i = 0; i < a.Length; i++)
			{
				if (a[i] != b[i])
					return false;
			}
			return true;
		}
		public bool AddInterface(TypeSpec ins)
		{
			if (interface_types == null)
			{
				interface_types = new List<TypeSpec>() { ins };
				return true;
			}

			if (interface_types.Contains(ins))
				return false;

			interface_types.Add(ins);
			return true;
		}

		public override string ToString()
		{
			return name;
		}
	}

	public class InternalType : TypeSpec
	{
		public static readonly InternalType AnonymousMethod = new InternalType("anonymous method");
		public static readonly InternalType Arglist = new InternalType("__arglist");
		public static readonly InternalType MethodGroup = new InternalType("method group");
		public static readonly InternalType NullLiteral = new InternalType("null");
		public static readonly InternalType FakeInternalType = new InternalType("<fake$type>");
		public static readonly InternalType Namespace = new InternalType("<namespace>");
		public static readonly InternalType ErrorType = new InternalType("<error>");
		public static readonly InternalType VarOutType = new InternalType("var out");
		public static readonly InternalType ThrowExpr = new InternalType("throw expression");

		InternalType(string name)
			: base(name, null)
		{
		}
	}
	
	public class EnumSpec : TypeSpec
	{
		public override bool IsEnum => true;
		public override int SizeOf => base_type.SizeOf;
		public EnumSpec(CommonAttribute attr, string name, TypeSpec declare)
			: base(name, declare)
		{
			this.attr = attr;
		}
	}

	public class BuildinTypeSpec : TypeSpec
	{
		public enum BuildinType
		{
			Void,
			Bool,
			Char,
			SByte,
			Byte,
			Short,
			UShort,
			Int,
			UInt,
			Long,
			ULong,
			Float,
			Double,
			String,
			Object,
		}
		public static BuildinTypeSpec Void = new BuildinTypeSpec(BuildinType.Void, "void");
		public static BuildinTypeSpec Bool = new BuildinTypeSpec(BuildinType.Bool, "bool");
		public static BuildinTypeSpec Char = new BuildinTypeSpec(BuildinType.Char, "char");
		public static BuildinTypeSpec SByte = new BuildinTypeSpec(BuildinType.SByte, "sbyte");
		public static BuildinTypeSpec Byte = new BuildinTypeSpec(BuildinType.Byte, "byte");
		public static BuildinTypeSpec Short = new BuildinTypeSpec(BuildinType.Short, "short");
		public static BuildinTypeSpec UShort = new BuildinTypeSpec(BuildinType.UShort, "ushort");
		public static BuildinTypeSpec Int = new BuildinTypeSpec(BuildinType.Int, "int");
		public static BuildinTypeSpec UInt = new BuildinTypeSpec(BuildinType.UInt, "uint");
		public static BuildinTypeSpec Long = new BuildinTypeSpec(BuildinType.Long, "long");
		public static BuildinTypeSpec ULong = new BuildinTypeSpec(BuildinType.ULong, "ulong");
		public static BuildinTypeSpec Float = new BuildinTypeSpec(BuildinType.Float, "float");
		public static BuildinTypeSpec Double = new BuildinTypeSpec(BuildinType.Double, "double");
		public static BuildinTypeSpec String = new BuildinTypeSpec(BuildinType.String, "string", false);
		public static BuildinTypeSpec Object = new BuildinTypeSpec(BuildinType.Object, "object", false);

		public const int PointerSize = 4;


		public BuildinType type;
		public bool is_value_type;
		public BuildinTypeSpec(BuildinType type, string name, bool is_value_type = true)
			: base(name, null)
		{
			this.type = type;
			this.is_value_type = is_value_type;
		}
		public override bool IsStruct => type < BuildinType.String;
		public override bool IsClass => type >= BuildinType.String;
		public override int SizeOf
		{
			get
			{
				switch (type)
				{
					case BuildinType.Void:
						return 0;
					case BuildinType.Bool:
					case BuildinType.SByte:
					case BuildinType.Byte:
						return 1;
					case BuildinType.Short:
					case BuildinType.UShort:
						return 2;
					case BuildinType.Int:
					case BuildinType.UInt:
					case BuildinType.Float:
						return 4;
					case BuildinType.Long:
					case BuildinType.ULong:
					case BuildinType.Double:
						return 8;
					default:
						return PointerSize;
				}
			}
		}
	}

	public abstract class ElementTypeSpec : TypeSpec
	{
		public TypeSpec element_type;
		public override TypeSpec ElementType => element_type;
		public override int SizeOf => BuildinTypeSpec.PointerSize;

		public ElementTypeSpec(string name, TypeSpec et)
			: base(name, et.declaringType)
		{
			element_type = et;
		}
	}

	public class PointerSpec : ElementTypeSpec
	{
		public override bool IsPointer => true;

		public PointerSpec(TypeSpec et)
			: base(et.name + "*", et)
		{
		}
	}

	public class ArraySpec : ElementTypeSpec
	{
		public override bool IsArray => true;
		public ArraySpec(TypeSpec et)
			: base(et.name + "[]", et)
		{
		}
	}

	public class ReferenceSpec : ElementTypeSpec
	{
		public override bool IsReference => true;

		public ReferenceSpec(TypeSpec et)
			: base(et.name + "&", et)
		{
		}
	}

	public class TypeParameterSpec : TypeSpec
	{
		public MemberSpec Owner;
		public TypeParameterSpec(MemberSpec owner, string name)
			: base(name, owner.declaringType)
		{
			Owner = owner;
		}
	}
}
