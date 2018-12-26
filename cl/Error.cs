using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class Error
	{
		public Location pos;
		public string msg;

		public Error(string msg)
		{
			this.msg = msg;
		}

		public Error(Location pos, string msg)
		{
			this.pos = pos;
			this.msg = msg;
		}
	}
}
