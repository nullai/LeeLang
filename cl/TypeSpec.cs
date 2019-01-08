using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class TypeSpec : NamespaceSpec
	{
		public TypeSpec base_type = null;
		public List<TypeSpec> interface_types = null;
		protected TypeSpec ArrayType;
		protected TypeSpec PointerType;
		public virtual bool IsArray => false;
		public virtual bool IsPointer => false;
		public virtual bool IsGeneric => false;
		public virtual bool IsStruct => false;
		public virtual bool IsClass => false;
		public virtual bool IsInterface => false;
		public virtual bool IsEnum => false;
		public virtual TypeSpec ElementType => null;
		public virtual IR_DataType IRType => IR_DataType.Void;
		public virtual int SizeOf => 0;

		public TypeSpec(string name, NamespaceSpec declare)
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
		public override List<MemberSpec> ResolveName(string name)
		{
			if (resolving)
				return null;
			resolving = true;
			List<MemberSpec> r;
			if (members.TryGetValue(name, out r))
			{
				for (int i = 0; i < r.Count; i++)
				{
					var item = r[i];
					if (item.name == name && item.prefix == null)
					{
						resolving = false;
						r.Clear();
						r.Add(item);
						return r;
					}
				}
			}
			if (usings != null)
			{
				for (int i = 0; i < usings.Count; i++)
				{
					var t = usings[i].ResolveName(name);
					if (t != null)
					{
						if (r != null)
							r.AddRange(t);
						else
							r = t;
					}
				}
			}

			if (r == null && base_type != null)
				r = base_type.ResolveName(name);

			if (r == null && Declaring != null)
				r = Declaring.ResolveName(name);

			resolving = false;
			return r;
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
	}

	public class ClassStructSpec : TypeSpec
	{
		public GenericParamterSpec[] generic_paramters;
		public Dictionary<TypeSpec[], TypeSpec> InsTypes;
		public bool is_interface = false;
		public bool is_struct = false;
		public override bool IsClass => !is_struct && !is_interface;
		public override bool IsStruct => is_struct;
		public override bool IsInterface => is_interface;
		public override bool IsGeneric => generic_paramters != null;
		public override int Arity => generic_paramters != null ? generic_paramters.Length : 0;
		public override IR_DataType IRType => IR_DataType.Pointer;
		public override int SizeOf
		{
			get
			{
				if (is_interface)
					return BuildinTypeSpec.PointerSize * 2;
				if (!is_struct)
					return BuildinTypeSpec.PointerSize;

				if (resolving)
					throw new Exception("循环依赖");

				resolving = true;

				int size = 0;
				foreach (var m in members.Values)
				{
					for (int i = 0; i < m.Count; i++)
					{
						var p = m[i] as FieldSpec;
						if (p != null && !p.IsStatic)
							size += p.field_type.SizeOf;
					}
				}

				resolving = false;
				return size;
			}
		}
		public ClassStructSpec(CommonAttribute attr, string name, NamespaceSpec declare, bool ins, bool str)
			: base(name, declare)
		{
			this.attr = attr;
			is_interface = ins;
			is_struct = str;
		}
		public void SetArity(int arity)
		{
			fullname += "`" + arity;
			generic_paramters = new GenericParamterSpec[arity];
		}
		public TypeSpec CreateGenericType(TypeSpec[] args)
		{
			if (InsTypes == null)
				InsTypes = new Dictionary<TypeSpec[], TypeSpec>();
			else
			{
				foreach (var p in InsTypes)
				{
					if (CompareTypes(p.Key, args))
					{
						return p.Value;
					}
				}
			}

			StringBuilder sb = new StringBuilder();
			sb.Append(name);
			sb.Append('<');
			for (int i = 0; i < args.Length; i++)
			{
				if (i != 0)
					sb.Append(',');
				sb.Append(args[i].name);
			}
			sb.Append('>');
			var v = new TypeSpec(sb.ToString(), Declaring);
			InsTypes.Add(args, v);
			return v;
		}
	}

	public class GenericParamterSpec : TypeSpec
	{
		public ClassStructSpec declaring_type;
		public override bool IsGeneric => true;
		public override IR_DataType IRType => throw new Exception("未实例化");
		public override int SizeOf => throw new Exception("未实例化");

		public GenericParamterSpec(ClassStructSpec dt, string name)
			: base(name, dt)
		{
			declaring_type = dt;
		}
	}

	public class EnumSpec : TypeSpec
	{
		public override bool IsEnum => true;
		public override IR_DataType IRType => base_type.IRType;
		public override int SizeOf => base_type.SizeOf;
		public EnumSpec(CommonAttribute attr, string name, NamespaceSpec declare)
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
		}
		public override bool IsStruct => type < BuildinType.String;
		public override bool IsClass => type >= BuildinType.String;
		public override IR_DataType IRType
		{
			get
			{
				switch (type)
				{
					case BuildinType.Void:
						return IR_DataType.Void;
					case BuildinType.Bool:
					case BuildinType.SByte:
					case BuildinType.Byte:
						return IR_DataType.I1;
					case BuildinType.Short:
					case BuildinType.UShort:
						return IR_DataType.I2;
					case BuildinType.Int:
					case BuildinType.UInt:
						return IR_DataType.I4;
					case BuildinType.Long:
					case BuildinType.ULong:
						return IR_DataType.I8;
					case BuildinType.Float:
						return IR_DataType.R4;
					case BuildinType.Double:
						return IR_DataType.R8;
					default:
						return IR_DataType.Pointer;
				}
			}
		}
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

	public class PointerSpec : TypeSpec
	{
		public TypeSpec element_type;
		public override bool IsPointer => true;
		public override TypeSpec ElementType => element_type;
		public override IR_DataType IRType => IR_DataType.Pointer;
		public override int SizeOf => BuildinTypeSpec.PointerSize;

		public PointerSpec(TypeSpec et)
			: base(et.name + "*", et.Declaring)
		{
			element_type = et;
		}
	}

	public class ArraySpec : TypeSpec
	{
		public TypeSpec element_type;
		public override bool IsArray => true;
		public override TypeSpec ElementType => element_type;
		public override IR_DataType IRType => IR_DataType.Pointer;
		public override int SizeOf => BuildinTypeSpec.PointerSize;
		public ArraySpec(TypeSpec et)
			: base(et.name + "[]", et.Declaring)
		{
			element_type = et;
		}
	}
}
