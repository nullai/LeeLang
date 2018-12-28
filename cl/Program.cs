using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	class Program
	{
		static void Main(string[] args)
		{
			Compiler cc = new Compiler("test");
			cc.AddFile(args[0]);
			try
			{
				var s = cc.Build();
				if (s)
					Console.WriteLine("编译成功，共 {0} 个警告。", cc.warn_count);
				else
					Console.WriteLine("编译失败，共 {0} 个错误，共 {1} 个警告。", cc.error_count, cc.warn_count);
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		static void PrintLine(string val, int tab)
		{
			Console.WriteLine(new string(' ', tab) + val);
		}
		static void PrintValue(string name, object v, int tab)
		{
			if (v == null)
			{
				PrintLine(name + " : null", tab);
				return;
			}
			var t = v.GetType();
			if (v is string || v is int || t.IsEnum)
			{
				PrintLine(name + " : " + v, tab);
				return;
			}
			if (v is Location)
				return;
			if (v is TokenValue)
			{
				TokenValue k = v as TokenValue;
				PrintLine(name + " : " + k.value, tab);
				return;
			}
			if (t.Name == "List`1")
			{
				var count = (int)t.GetMethod("get_Count").Invoke(v, null);
				PrintLine(name + " : List(" + count + ")", tab);
				var m = t.GetMethod("get_Item");
				for (int i = 0; i < count; i++)
				{
					PrintValue("[" + i + "]", m.Invoke(v, new object[] { i }), tab + 1);
				}
				return;
			}
			PrintLine(name + " : " + t.Name, tab);
			PrintAst(v, tab + 1);
		}
		public static void PrintAst(object s, int tab)
		{
			if (s == null)
			{
				PrintLine("null", tab);
				return;
			}
			Type t = s.GetType();
			var fields = t.GetFields();
			for (int i = 0; i < fields.Length; i++)
			{
				var f = fields[i];
				var v = f.GetValue(s);
				PrintValue(f.Name, v, tab);
			}
		}
	}
}
