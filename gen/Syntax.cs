using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace gen
{
	public class Syntax
	{
		public static List<SyntaxType> ParseTypes(string text)
		{
			List<SyntaxType> result = new List<SyntaxType>();

			int offset = SkipSpace(text, 0);
			while (offset < text.Length)
			{
				SyntaxType type = new SyntaxType();
				type.comments = ParseComment(text, ref offset);

				string id = ParseName(text, ref offset);
				if (id != "class")
					throw new Exception(id);

				type.name = ParseName(text, ref offset);

				offset = Except(text, offset, ':');
				type.basename = ParseName(text, ref offset);
				if (text[offset] == '=')
				{
					offset = Except(text, offset, '=');
					offset = Except(text, offset, '0');
					type.is_abstract = true;
				}
				offset = Except(text, offset, '{');

				while (text[offset] != '}')
				{
					SyntaxField field = new SyntaxField();
					field.comments = ParseComment(text, ref offset);
					field.type = ParseName(text, ref offset);
					field.name = ParseName(text, ref offset);
					if (text[offset] == '=')
					{
						offset = Except(text, offset, '=');
						ParseName(text, ref offset);

						field.is_option = true;
					}
					offset = Except(text, offset, ';');
					type.fields.Add(field);
				}
				offset = Except(text, offset, '}');

				result.Add(type);
			}

			return result;
		}

		static int Except(string text, int offset, char ch)
		{
			if (text[offset] != ch)
				throw new Exception("期望 " + ch);

			return SkipSpace(text, offset + 1);
		}

		static string ParseComment(string text, ref int offset)
		{
			if (text[offset] == '/' && text[offset + 1] == '/')
			{
				offset += 2;
				int e = text.IndexOf('\n', offset);
				string c = text.Substring(offset, e - offset).Trim();
				offset = SkipSpace(text, e + 1);
				return c;
			}
			return null;
		}

		static string ParseName(string text, ref int offset)
		{
			int s = offset;
			while (offset < text.Length)
			{
				char ch = text[offset];
				if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9') || ch == '_' || ch == '<' || ch == '>')
				{
					++offset;
					continue;
				}
				break;
			}
			if (offset <= s)
				throw new Exception("期望一个标识符");
			string name = text.Substring(s, offset - s);
			offset = SkipSpace(text, offset);
			return name;
		}

		static int SkipSpace(string text, int offset)
		{
			while (offset < text.Length)
			{
				char ch = text[offset];
				if (ch != ' ' && ch != '\t' && ch != '\r' && ch != '\n')
					return offset;
				++offset;
			}
			return offset;
		}

		public static void Gen(string input, string output)
		{
			var types = Syntax.ParseTypes(File.ReadAllText(input));

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < types.Count; i++)
			{
				sb.AppendLine();
				var type = types[i];
				if (type.comments != null)
					sb.AppendLine("// " + type.comments);

				if (type.is_abstract)
					sb.AppendLine("public abstract class " + type.name + " : " + type.basename);
				else
					sb.AppendLine("public class " + type.name + " : " + type.basename);
				sb.AppendLine("{");
				for (int j = 0; j < type.fields.Count; j++)
				{
					if (j > 0)
						sb.AppendLine();
					var field = type.fields[j];
					if (field.comments != null)
						sb.AppendLine("\t// " + field.comments);
					sb.AppendLine("\tpublic " + field.type + " m" + field.name + ";");
				}
				sb.AppendLine();
				sb.Append("\tpublic " + type.name + "(");
				var fs = GetAllFields(types, type);
				for (int j = 0; j < fs.Count; j++)
				{
					if (j > 0)
						sb.Append(", ");
					var field = fs[j];
					sb.Append(field.type + " " + field.name);
				}
				sb.AppendLine(")");
				if (fs.Count != type.fields.Count)
				{
					sb.Append("\t\t: base(");
					for (int j = 0; j < fs.Count - type.fields.Count; j++)
					{
						if (j > 0)
							sb.Append(", ");
						var field = fs[j];
						sb.Append(field.name);
					}
					sb.AppendLine(")");
				}
				sb.AppendLine("\t{");
				for (int j = fs.Count - type.fields.Count; j < fs.Count; j++)
				{
					var field = fs[j];
					sb.AppendLine("\t\tthis.m" + field.name + " = " + field.name + ";");
				}
				sb.AppendLine("\t}");
				if (!type.is_abstract)
				{
					sb.AppendLine();
					sb.AppendLine("\tpublic override int Count => " + fs.Count + ";");
					sb.AppendLine("\tpublic override SyntaxNode GetAt(int index)");
					sb.AppendLine("\t{");
					if (fs.Count > 0)
					{
						sb.AppendLine("\t\tswitch (index)");
						sb.AppendLine("\t\t{");
						for (int j = 0; j < fs.Count; j++)
						{
							var field = fs[j];
							sb.AppendLine("\t\tcase " + j + ":");
							sb.AppendLine("\t\t\treturn m" + field.name + ";");
						}
						sb.AppendLine("\t\t}");
					}
					sb.AppendLine("\t\treturn null;");
					sb.AppendLine("\t}");
					sb.AppendLine("\tpublic override void Accept(SyntaxVisitor visitor)");
					sb.AppendLine("\t{");
					sb.AppendLine("\t\tvisitor.Visit" + type.name + "(this);");
					sb.AppendLine("\t}");
				}
				sb.AppendLine("}");
			}

			File.WriteAllText(output + "gen_Syntax.cs", sb.ToString());

			sb.Clear();

			sb.AppendLine("public abstract class SyntaxVisitor");
			sb.AppendLine("{");
			sb.AppendLine("\tpublic virtual void DefaultVisit(SyntaxNode value)");
			sb.AppendLine("\t{");
			sb.AppendLine("\t}");
			for (int i = 0; i < types.Count; i++)
			{
				sb.AppendLine();
				var type = types[i];
				if (type.comments != null)
					sb.AppendLine("\t// " + type.comments);
				sb.AppendLine("\tpublic virtual void Visit" + type.name + "(" + type.name + " value)");
				sb.AppendLine("\t{");
				sb.AppendLine("\t\tDefaultVisit(value);");
				sb.AppendLine("\t}");
			}
			sb.AppendLine("}");
			File.WriteAllText(output + "gen_SyntaxVisitor.cs", sb.ToString());
		}
		static List<SyntaxField> GetAllFields(List<SyntaxType> types, SyntaxType type)
		{
			SyntaxType bs = types.Find(x => x.name == type.basename);
			if (bs == null)
				return type.fields;

			List<SyntaxField> bsf = GetAllFields(types, bs);
			if (type.fields.Count == 0)
				return bsf;

			List<SyntaxField> r = new List<SyntaxField>();
			r.AddRange(bsf);
			r.AddRange(type.fields);
			return r;
		}
	}

	public class SyntaxType
	{
		public string comments = null;
		public bool is_abstract = false;
		public string name;
		public string basename;
		public List<SyntaxField> fields = new List<SyntaxField>();
	}
	public class SyntaxField
	{
		public bool is_option;
		public string comments = null;
		public string type;
		public string name;
	}
}
