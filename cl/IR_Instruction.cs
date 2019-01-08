using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public enum IR_Opcodes
	{
		Nop,

		Ret,
		Br,
		Switch,
		Unreachable,

		Alloca,
		Load,
		Store,

		Add,
		FAdd,
		Sub,
		FSub,
		Mul,
		FMul,
		UDiv,
		SDiv,
		FDiv,
		URem,
		SRem,
		FRem,

		And,
		Or,
		Xor,
		Not,
		Neg,
		Shl,
		LShr,
		AShr,

		Trunc,			// 截断整数
		ZExt,
		SExt,
		FPToUI,
		FPToSI,
		UIToFP,
		SIToFP,
		FPTrunc,
		FPExt,
		PtrToInt,
		IntToPtr,
		BitCast,

		ICmp,
		FCmp,
		PHI,
	}
	public enum IR_DataType
	{
		Void,
		Pointer,
		I1,
		I2,
		I4,
		I8,
		R4,
		R8,
	}
	public abstract class IR_Value
	{
		public IR_DataType type;
		public int id;
		private static int value_id = 0;

		public virtual bool IsConstent => false;

		public IR_Value(IR_DataType type)
		{
			this.type = type;
			this.id = ++value_id;
		}

		public static void ResetID()
		{
			value_id = 0;
		}

		public override string ToString()
		{
			return type + " %" + id;
		}
		public virtual void Dump(StringBuilder sb)
		{
		}

		public static IR_Instruction CreateAlloc(int size)
		{
			IR_Instruction r = new IR_Instruction(IR_DataType.Pointer, IR_Opcodes.Alloca);
			r.operands = new IR_Value[1] { new IR_ConstentData((uint)size)};
			return r;
		}
		public static IR_Instruction CreateReturn(IR_Value v)
		{
			IR_Instruction r = new IR_Instruction(IR_DataType.Void, IR_Opcodes.Ret);
			if (v != null)
				r.operands = new IR_Value[1] { v };
			return r;
		}
		public static IR_Instruction CreateBr(IR_Value v, IR_Value t, IR_Value f)
		{
			IR_Instruction r = new IR_Instruction(IR_DataType.Void, IR_Opcodes.Br);
			if (f != null)
				r.operands = new IR_Value[3] { v, t, f };
			else if (v != null)
				r.operands = new IR_Value[2] { v, t };
			else
				r.operands = new IR_Value[1] { t };
			return r;
		}
	}
	public class IR_Instruction : IR_Value
	{
		public IR_Opcodes opcode;
		public IR_Value[] operands;

		public int OperandCount => operands == null ? 0 : operands.Length;
		public IR_Value GetOperand(int idx)
		{
			return operands[idx];
		}

		public IR_Instruction(IR_DataType type, IR_Opcodes opcode)
			: base(type)
		{
			this.opcode = opcode;
		}
		public override void Dump(StringBuilder sb)
		{
			if (operands != null)
			{
				for (int i = operands.Length - 1; i >= 0; i--)
				{
					operands[i].Dump(sb);
				}
			}
			sb.Append('\t');
			if (type != IR_DataType.Void)
			{
				sb.Append("%" + id);
				sb.Append('=');
			}
			sb.Append(opcode.ToString());
			if (operands != null)
			{
				sb.Append(' ');
				for (int i = 0; i < operands.Length; i++)
				{
					if (i != 0)
						sb.Append(',');
					sb.Append(operands[i].ToString());
				}
			}
			sb.AppendLine();
		}
	}
	public abstract class IR_Constent : IR_Value
	{
		public override bool IsConstent => true;

		public IR_Constent(IR_DataType type)
			: base(type)
		{
		}
	}

	public class IR_GlobalValue : IR_Constent
	{
		public string name;
		public IR_GlobalValue(string name, IR_DataType type)
			: base(type)
		{
			this.name = name;
		}
		public override string ToString()
		{
			return type + " @" + id;
		}
	}

	public class IR_ConstentExpr : IR_Constent
	{
		public IR_Opcodes opcode;
		public IR_Constent lv;
		public IR_Constent rv;
		public IR_ConstentExpr(IR_DataType type, IR_Opcodes opcode)
			: base(type)
		{
			this.opcode = opcode;
		}
		public override string ToString()
		{
			switch (opcode)
			{
				case IR_Opcodes.Add:
					return lv.ToString() + " + " + rv.ToString();
				case IR_Opcodes.Sub:
					return lv.ToString() + " - " + rv.ToString();
				default:
					throw new Exception("不支持的常量表达式");
			}
		}
	}

	public class IR_ConstentData : IR_Constent
	{
		public object value;

		public IR_ConstentData(byte val)
			:base(IR_DataType.I1)
		{
			value = val;
		}
		public IR_ConstentData(ushort val)
			: base(IR_DataType.I2)
		{
			value = val;
		}
		public IR_ConstentData(uint val)
			: base(IR_DataType.I4)
		{
			value = val;
		}
		public IR_ConstentData(ulong val)
			: base(IR_DataType.I8)
		{
			value = val;
		}
		public IR_ConstentData(float val)
			: base(IR_DataType.R4)
		{
			value = val;
		}
		public IR_ConstentData(double val)
			: base(IR_DataType.R8)
		{
			value = val;
		}
		public override string ToString()
		{
			return type + " " + value;
		}
	}

	public class IR_BlockAddr : IR_Constent
	{
		public IR_BaseBlock block;
		public IR_BlockAddr(IR_BaseBlock block)
			: base(IR_DataType.Pointer)
		{
			this.block = block;
		}
		public override string ToString()
		{
			return "Label_" + block.id;
		}
	}
	public class IR_Argument : IR_Value
	{
		public IR_Argument(IR_DataType type)
			: base(type)
		{
		}
	}
}
