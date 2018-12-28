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
		public bool warn;

		public Error(string msg, bool warn)
		{
			this.msg = msg;
			this.warn = warn;
		}

		public Error(Location pos, string msg, bool warn)
		{
			this.pos = pos;
			this.msg = msg;
			this.warn = warn;
		}
	}
}
