using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class IR_Link
	{
		public IR_BaseBlock block;
		public IR_Link next;

		public static int CountOf(IR_Link link)
		{
			int count = 0;
			while (link != null)
			{
				++count;
				link = link.next;
			}
			return count;
		}
	}
	public class IR_BaseBlock : IR_Value
	{
		public IR_Method method;
		public IR_Link from;
		public IR_Link to;
		public List<IR_Instruction> instructions = new List<IR_Instruction>();

		public IR_BaseBlock(IR_Method method)
			: base(IR_DataType.Void)
		{
			this.method = method;
		}

		public void Add(IR_Instruction ins)
		{
			instructions.Add(ins);
		}

		public void Remove(IR_Instruction ins)
		{
			instructions.Remove(ins);
		}

		public void LinkTo(IR_BaseBlock bs)
		{
			IR_Link k = new IR_Link();
			k.block = bs;
			k.next = to;
			to = k;

			k = new IR_Link();
			k.block = this;
			k.next = bs.from;
			bs.from = k;
		}

		public int InstructionCount => instructions.Count;

		public int FromCount => IR_Link.CountOf(from);
		public int ToCount => IR_Link.CountOf(to);

		public IR_Instruction TermInstruction => instructions[instructions.Count - 1];

		public override string ToString()
		{
			return "Label_" + id;
		}
		public override void Dump(StringBuilder sb)
		{
			sb.AppendLine("Label_" + id + ":");

			for (int i = 0; i < instructions.Count; i++)
			{
				instructions[i].Dump(sb);
			}
		}
	}
}
