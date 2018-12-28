using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class TypeSpec : NamespaceSpec
	{
		protected TypeSpec ArrayType;
		protected TypeSpec PointerType;
		public virtual bool IsArray => false;
		public virtual bool IsPointer => false;
		public virtual bool IsGeneric => false;
		public virtual int Arity => 0;
		public virtual List<TypeSpec> BaseTypes => null;
		public virtual TypeSpec ElementType => null;

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
		public override List<MemberSpec> GetMembers(string name)
		{
			List<MemberSpec> r;
			if(members.TryGetValue(name, out r))
				return r;

			var bs = BaseTypes;
			if (bs != null)
			{
				for (int i = 0; i < bs.Count; i++)
				{
					var t = bs[i].GetMembers(name);
					if (t != null)
					{
						if (r == null)
							r = t;
						else
							r.AddRange(t);
					}
				}
			}
			return r;
		}
	}

	public class ClassStructSpec : TypeSpec
	{
		public List<TypeSpec> base_types = new List<TypeSpec>();
		public ClassStructSpec(string name, NamespaceSpec declare)
			: base(name, declare)
		{
		}

		public override List<TypeSpec> BaseTypes => base_types;
	}

	public class EnumSpec : TypeSpec
	{
		public List<TypeSpec> base_types = new List<TypeSpec>();
		public EnumSpec(string name, NamespaceSpec declare)
			: base(name, declare)
		{
		}

		public override List<TypeSpec> BaseTypes => base_types;
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


		public BuildinType type;
		public bool is_value_type;
		public BuildinTypeSpec(BuildinType type, string name, bool is_value_type = true)
			: base(name, null)
		{
			this.type = type;
		}
	}

	public class PointerSpec : TypeSpec
	{
		public TypeSpec element_type;
		public override bool IsPointer => true;
		public override TypeSpec ElementType => element_type;

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
		public ArraySpec(TypeSpec et)
			: base(et.name + "[]", et.Declaring)
		{
			element_type = et;
		}
	}

	public class GenericTypeSpec : TypeSpec
	{
		public GenericParamterSpec[] paramters;
		public Dictionary<TypeSpec[], TypeSpec> InsTypes;
		public override bool IsGeneric => true;
		public override int Arity => paramters.Length;

		public GenericTypeSpec(string name, int arity, NamespaceSpec declare)
			: base(name, declare)
		{
			fullname += "`" + arity;
			paramters = new GenericParamterSpec[arity];
		}

		public TypeSpec CreateType(TypeSpec[] args)
		{
			if (InsTypes == null)
				InsTypes = new Dictionary<TypeSpec[], TypeSpec>();
			else
			{
				foreach(var p in InsTypes)
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
		public GenericTypeSpec declaring_type;
		public override bool IsGeneric => true;

		public GenericParamterSpec(GenericTypeSpec dt, string name)
			: base(name, dt)
		{
			declaring_type = dt;
		}
	}
}
