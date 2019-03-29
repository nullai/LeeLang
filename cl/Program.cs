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
					Console.WriteLine("编译成功，共 {0} 个警告。", cc.Context.warn_count);
				else
					Console.WriteLine("编译失败，共 {0} 个错误，共 {1} 个警告。", cc.Context.error_count, cc.Context.warn_count);
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
		static List<object> printed = new List<object>();
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
			if (v is string || v is int || t.IsEnum || v is bool)
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
				System.Collections.IList dict = v as System.Collections.IList;
				var count = dict.Count;
				PrintLine(name + " : List[" + count + "]", tab);

				int i = 0;
				foreach (object p in dict)
				{
					PrintValue("[" + i + "]", p, tab + 1);
					++i;
				}

				return;
			}
			if (t.IsArray)
			{
				Array a = v as Array;
				var count = a.Length;
				PrintLine(name + " : Array[" + count + "]", tab);
				for (int i = 0; i < count; i++)
				{
					PrintValue("[" + i + "]", a.GetValue(i), tab + 1);
				}
				return;
			}
			if (t.Name == "Dictionary`2")
			{
				System.Collections.IDictionary dict = v as System.Collections.IDictionary;
				var count = dict.Count;
				PrintLine(name + " : Dictionary[" + count + "]", tab);
				foreach (System.Collections.DictionaryEntry p in dict)
				{
					PrintValue("[" + p.Key + "]", p.Value, tab + 1);
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
			if (t.IsClass)
			{
				if (printed.Contains(s))
				{
					PrintLine("{...}", tab);
					return;
				}
				printed.Add(s);
			}
			var fields = t.GetFields();
			for (int i = 0; i < fields.Length; i++)
			{
				var f = fields[i];
				if (f.IsStatic)
					continue;
				var v = f.GetValue(s);
				PrintValue(f.Name, v, tab);
			}
		}
	}
}
