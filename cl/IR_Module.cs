using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class IR_Module
	{
		public List<IR_Method> methods = new List<IR_Method>();

		public void Dump(StringBuilder sb)
		{
			for (int i = 0; i < methods.Count; i++)
			{
				methods[i].Dump(sb);
			}
		}
	}
}
