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

		public Location NextColumn
		{
			get
			{
				return new Location(File, row, col + 1);
			}
		}
	}
}
