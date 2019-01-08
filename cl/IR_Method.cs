using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class IR_Method
	{
		public IR_Module module;
		public string name;
		public IR_DataType ret_type;
		public IR_Argument ret_arg;
		public List<IR_Argument> parameters;
		public List<IR_BaseBlock> blocks = new List<IR_BaseBlock>();

		public IR_BaseBlock EntryBlock => blocks[0];
		public int BlockCount => blocks.Count;

		public IR_Method(string name, IR_DataType type)
		{
			this.name = name;
			ret_type = type;
		}

		public void AddParameter(IR_Argument arg)
		{
			if (parameters == null)
				parameters = new List<IR_Argument>();
			parameters.Add(arg);
		}

		public void Dump(StringBuilder sb)
		{
			sb.Append("method ");
			sb.Append(ret_type.ToString());
			sb.Append(' ');
			sb.Append(name);
			sb.Append('(');
			if (parameters != null)
			{
				for (int i = 0; i < parameters.Count; i++)
				{
					if (i != 0)
						sb.Append(',');
					sb.Append(parameters[i].type.ToString());
					sb.Append(' ');
					parameters[i].Dump(sb);
				}
			}
			sb.Append(')');
			sb.AppendLine();

			for (int i = 0; i < blocks.Count; i++)
				blocks[i].Dump(sb);

			sb.AppendLine("end");
		}
	}
}
