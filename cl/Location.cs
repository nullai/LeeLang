using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public struct Location
	{
		public string File;
		public int row;
		public int col;

		public static Location Null = new Location();

		public Location(string f, int r, int c)
		{
			File = f;
			row = r;
			col = c;
		}

		public bool IsValid
		{
			get
			{
				return File != null;
			}
		}

		public override string ToString()
		{
			return string.Format("{0}({1},{2})", File, row, col);
		}
	}
}
