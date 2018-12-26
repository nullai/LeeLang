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
		public int begin_row;
		public int begin_col;
		public int end_row;
		public int end_col;

		public static Location Null = new Location();

		public Location(string f, int r, int c, int er, int ec)
		{
			File = f;
			begin_row = r;
			begin_col = c;
			end_row = er;
			end_col = ec;
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
			return string.Format("{0}({1},{2},{3},{4})", File, begin_row, begin_col, end_row, end_col);
		}


		public Location StartLocation
		{
			get
			{
				return new Location(File, begin_row, begin_col, begin_row, begin_col);
			}
		}

		public Location EndLocation
		{
			get
			{
				return new Location(File, end_row, end_col, end_row, end_col);
			}
		}
	}
}
