using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class Parser
	{
		public Tokenizer m_tokener;
		public LocatedToken m_current;

		public Parser(SourceFile file)
		{
			m_tokener.file = file;
			m_tokener.Fixup();
			m_current = m_tokener.Next();
		}
		public void Next()
		{
			m_current = m_tokener.Next();
		}
		public void Skip(Token token)
		{
			if (m_current.token == token)
				m_current = m_tokener.Next();
		}
		public bool Expect(Token token)
		{
			if (m_current.token == token)
			{
				m_current = m_tokener.Next();
				return true;
			}
			else
			{
				Error("丢失:" + token);
				return false;
			}
		}
		public void Error(string msg)
		{
			Console.WriteLine("ERROR:" + msg);
		}
		public void Error(Location pos, string msg)
		{
			Console.WriteLine("ERROR:" + msg);
		}
		public void Waring(string msg)
		{
			Console.WriteLine("WARING:" + msg);
		}
		public void Waring(Location pos, string msg)
		{
			Console.WriteLine("WARING:" + msg);
		}
		public FileStatement ParseFile()
		{
			FileStatement result = new FileStatement(m_tokener.file.FullPathName);

			while (m_current.token != Token.EOF)
			{
				result.members.Add(ParseTypeStatement());
			}

			return result;
		}
		private Attributes AddAttribute(Attributes attr, LocatedToken val)
		{
			if (attr == null)
				attr = new Attributes();
			if (attr.sys == null)
				attr.sys = new List<LocatedToken>();

			if (attr.sys.FindIndex(x => x.token == val.token) >= 0)
				Error("重复的属性");
			else
				attr.sys.Add(val);
			return attr;
		}
		private void RejectAttribute(Attributes attr)
		{
			if (attr == null)
				return;
			if (attr.sys == null)
				return;
			for (int i = 0; i < attr.sys.Count; i++)
			{
				var v = attr.sys[i];
				Error(v.loc, "不支持此属性:" + v);
			}
		}
		public Statement ParseTypeStatement()
		{
			Attributes attr = null;
			_begin:
			switch (m_current.token)
			{
				case Token.USING:
					RejectAttribute(attr);
					Next();
					return ParseUsing();
				case Token.NAMESPACE:
					RejectAttribute(attr);
					Next();
					return ParseNamespace();
				case Token.PUBLIC:
				case Token.PROTECTED:
				case Token.PRIVATE:
				case Token.STATIC:
				case Token.CONST:
				case Token.FINALLY:
				case Token.EXPLICIT:
				case Token.EXTERN:
				case Token.IMPLICIT:
				case Token.INTERNAL:
				case Token.OVERRIDE:
				case Token.PARAMS:
				case Token.SEALED:
				case Token.VIRTUAL:
				case Token.VOLATILE:
				case Token.PARTIAL:
				case Token.WEEK:
					attr = AddAttribute(attr, m_current);
					Next();
					goto _begin;
				case Token.CLASS:
				case Token.STRUCT:
				case Token.INTERFACE:
					return ParseClass(attr);
				case Token.ENUM:
					return ParseEnum(attr);
				case Token.VOID:
				case Token.OBJECT:
				case Token.BOOL:
				case Token.SBYTE:
				case Token.BYTE:
				case Token.SHORT:
				case Token.USHORT:
				case Token.INT:
				case Token.UINT:
				case Token.LONG:
				case Token.ULONG:
				case Token.FLOAT:
				case Token.DOUBLE:
				case Token.STRING:
				case Token.IDENTIFIER:
					return ParseBeginIdentifier(attr);
				default:
					// TODO:
					Error("未知Token:" + m_current);
					Next();
					return new ErrorStatement();
			}
		}

		public Statement ParseUsing()
		{
			NamesExpression name = ParseNamesExpression();
			if (name.IsEmpty)
			{
				Error("期望一个名字。");
				Skip(Token.SEMICOLON);
				return new ErrorStatement();
			}

			if (name.IsSamepleName && m_current.token == Token.ASSIGN)
			{
				Next();
				NamesExpression val = ParseNamesExpression();
				if (val.IsEmpty)
				{
					Error("期望一个名字。");
					Skip(Token.SEMICOLON);
					return new ErrorStatement();
				}

				Expect(Token.SEMICOLON);
				return new UsingStatement(name[0], val);
			}
			Expect(Token.SEMICOLON);
			return new UsingStatement(null, name);
		}

		public Statement ParseNamespace()
		{
			NamesExpression name = ParseNamesExpression();
			if (name.IsEmpty)
				Error("期望一个名字。");

			if (m_current.token != Token.OPEN_BRACE)
				return new ErrorStatement();
			Next();

			NamespaceStatement space = new NamespaceStatement(name);
			while (m_current.token != Token.EOF && m_current.token != Token.CLOSE_BRACE)
			{
				space.members.Add(ParseTypeStatement());
			}
			Expect(Token.CLOSE_BRACE);
			return space;
		}

		public Statement ParseClass(Attributes attr)
		{
			var key = m_current;
			Next();
			NamesExpression name = ParseNamesExpression();
			if (name.IsEmpty)
				Error("期望一个名字。");

			ClassStatement result = new ClassStatement(attr, key, name);
			if (m_current.token == Token.COLON)
			{
				result.base_type = new List<NamesExpression>();
				do
				{
					Next();
					name = ParseNamesExpression();

					if (name.IsEmpty)
					{
						Error("期望一个名字。");
						break;
					}

					result.base_type.Add(name);
				} while (m_current.token == Token.COMMA);
			}

			if (m_current.token != Token.OPEN_BRACE)
				return new ErrorStatement();
			Next();

			while (m_current.token != Token.EOF && m_current.token != Token.CLOSE_BRACE)
			{
				result.members.Add(ParseTypeStatement());
			}
			Expect(Token.CLOSE_BRACE);
			return result;
		}

		public Statement ParseEnum(Attributes attr)
		{
			var key = m_current;
			Next();
			NamesExpression name = ParseNamesExpression();
			if (name.IsEmpty)
				Error("期望一个名字。");

			EnumStatement result = new EnumStatement(attr, key, name);
			if (m_current.token == Token.COLON)
			{
				result.base_type = new List<NamesExpression>();
				do
				{
					Next();
					name = ParseNamesExpression();

					if (name.IsEmpty)
					{
						Error("期望一个名字。");
						break;
					}

					result.base_type.Add(name);
				} while (m_current.token == Token.COMMA);
			}

			if (m_current.token != Token.OPEN_BRACE)
				return new ErrorStatement();
			Next();

			while (m_current.token != Token.EOF && m_current.token != Token.CLOSE_BRACE)
			{
				result.members.Add(ParseExpression());
				if (m_current.token == Token.COMMA)
					Next();
				else if (m_current.token != Token.CLOSE_BRACE)
				{
					Error("不期望的Token:" + m_current);
					Next();
				}
			}
			Expect(Token.CLOSE_BRACE);
			return result;
		}

		public Statement ParseBeginIdentifier(Attributes attr)
		{
			NamesExpression a = ParseNamesExpression();
			switch (m_current.token)
			{
				case Token.IDENTIFIER:
					return ParseDeclare(attr, a, ParseNamesExpression());
				case Token.THIS:
					return ParseDeclareProperty(attr, a, ParseNamesExpression());
				case Token.OPEN_PARENS:
					return ParseDeclareMethod(attr, null, a);
				default:
					Error("不期望的Token:" + m_current);
					return new ErrorStatement();
			}
		}

		public Statement ParseDeclare(Attributes attr, NamesExpression type, NamesExpression name)
		{
			switch (m_current.token)
			{
				case Token.OPEN_PARENS:
					return ParseDeclareMethod(attr, type, name);
				case Token.ASSIGN:
				case Token.SEMICOLON:
					return ParseDeclareField(attr, type, name);
				case Token.OPEN_BRACE:
				case Token.ARROW:
					return ParseDeclareProperty(attr, type, name);
				default:
					Error("不期望的Token:" + m_current);
					return new ErrorStatement();
			}
		}

		public Statement ParseDeclareMethod(Attributes attr, NamesExpression type, NamesExpression name)
		{
			MethodStatement result = new MethodStatement(attr, type, name);
			Expect(Token.OPEN_PARENS);
			if (m_current.token != Token.CLOSE_PARENS)
			{
				while (true)
				{
					var p = ParseParameter();
					if (p == null)
						Error("期望参数类型");
					else
						result.AddParameter(p);

					if (m_current.token != Token.COMMA)
						break;
					Next();
				}
			}
			Expect(Token.CLOSE_PARENS);
			return ParseMethodBody(result);
		}

		public Statement ParseMethodBody(MethodStatement result)
		{
			if (m_current.token == Token.OPEN_BRACE)
				result.body = ParseBlockStatement();
			else
				Expect(Token.SEMICOLON);
			return result;
		}

		public Statement ParseDeclareField(Attributes attr, NamesExpression type, NamesExpression name)
		{
			FieldStatement result = new FieldStatement(attr, type, name);
			if (m_current.token == Token.ASSIGN)
			{
				Next();
				result.value = ParseExpression();
			}
			Expect(Token.SEMICOLON);
			return result;
		}

		public Statement ParseDeclareProperty(Attributes attr, NamesExpression type, NamesExpression name)
		{
			PropertyStatement result = new PropertyStatement(attr, type, name);
			// TODO:array[]
			if (m_current.token == Token.ARROW)
			{
				Next();

				// TODO:=>
			}

			Expect(Token.OPEN_BRACE);

			while (true)
			{
				attr = null;
				while (m_current.token == Token.PUBLIC || m_current.token == Token.PRIVATE || m_current.token == Token.PROTECTED || m_current.token == Token.INTERNAL)
				{
					attr = AddAttribute(attr, m_current);
					Next();
				}

				if (m_current.token != Token.IDENTIFIER)
					break;

				MethodStatement method;
				switch (m_current.value)
				{
					case "get":
						method = new MethodStatement(attr, type, new NamesExpression(m_current));
						Next();
						ParseMethodBody(method);
						result.value.Add(method);
						break;
					case "set":
						method = new MethodStatement(attr, new NamesExpression("void"), new NamesExpression(m_current));
						method.AddParameter(new ParameterStatement(null, type, new NamesExpression("value")));
						Next();
						ParseMethodBody(method);
						result.value.Add(method);
						break;
					default:
						Error("不支持的属性方法:" + m_current.value);
						method = new MethodStatement(attr, new NamesExpression("void"), new NamesExpression(m_current));
						Next();
						ParseMethodBody(method);
						result.value.Add(method);
						break;
				}
			}

			Expect(Token.CLOSE_BRACE);

			return result;
		}

		public ParameterStatement ParseParameter()
		{
			Attributes attr = null;
			while (m_current.token == Token.IN || m_current.token == Token.OUT || m_current.token == Token.THIS)
			{
				attr = AddAttribute(attr, m_current);
				Next();
			}
			var type = ParseNamesExpression();
			if (type.IsEmpty)
			{
				RejectAttribute(attr);
				return null;
			}

			var name = ParseNamesExpression();

			return new ParameterStatement(attr, type, name);
		}

		public Statement ParseExpressionStatement()
		{
			switch (m_current.token)
			{
				case Token.SEMICOLON:
					Next();
					return new EmptyStatement();
				case Token.OPEN_BRACE:
					return ParseBlockStatement();
				case Token.IF:
					return ParseIfStatement();
				case Token.ELSE:
					return ParseElseStatement();
				case Token.FOR:
					return ParseForStatement();
				case Token.WHILE:
					return ParseWhileStatement();
				case Token.DO:
					return ParseDoStatement();
				case Token.RETURN:
					return ParseReturnStatement();
				case Token.BREAK:
					Next();
					Expect(Token.SEMICOLON);
					return new BreakStatement();
				case Token.CONTINUE:
					Next();
					Expect(Token.SEMICOLON);
					return new ContinueStatement();
			}

			// TODO:
			return null;
		}

		public BlockStatement ParseBlockStatement()
		{
			BlockStatement result = new BlockStatement();
			Expect(Token.OPEN_BRACE);

			while (m_current.token != Token.EOF && m_current.token != Token.CLOSE_BRACE)
			{
				result.values.Add(ParseExpressionStatement());
			}

			Expect(Token.CLOSE_BRACE);
			return result;
		}

		public Statement ParseIfStatement()
		{
			Expect(Token.IF);

			if (!Expect(Token.OPEN_PARENS))
			{
				return new ErrorStatement();
			}
			Next();
			Statement cond = ParseExpression();
			Expect(Token.CLOSE_PARENS);

			IfStatement result = new IfStatement(cond);
			result.body = ParseExpressionStatement();

			return result;
		}

		public Statement ParseElseStatement()
		{
			Expect(Token.ELSE);

			ElseStatement result = new ElseStatement();
			result.body = ParseExpressionStatement();

			return result;
		}

		public Statement ParseForStatement()
		{
			Expect(Token.FOR);

			if (!Expect(Token.OPEN_PARENS))
			{
				return new ErrorStatement();
			}
			Next();
			Statement init = null;
			Statement cond = null;
			Statement iter = null;
			if (m_current.token != Token.SEMICOLON)
				init = ParseExpressionStatement();
			Expect(Token.SEMICOLON);
			if (m_current.token != Token.SEMICOLON)
				cond = ParseExpression();
			Expect(Token.SEMICOLON);
			if (m_current.token != Token.CLOSE_PARENS)
				iter = ParseExpressionStatement();
			Expect(Token.CLOSE_PARENS);

			ForStatement result = new ForStatement(init, cond, iter);
			result.body = ParseExpressionStatement();

			return result;
		}

		public Statement ParseWhileStatement()
		{
			Expect(Token.WHILE);

			if (!Expect(Token.OPEN_PARENS))
			{
				return new ErrorStatement();
			}
			Next();
			Statement cond = ParseExpression();
			Expect(Token.CLOSE_PARENS);

			WhileStatement result = new WhileStatement(cond);
			result.body = ParseExpressionStatement();

			return result;
		}

		public Statement ParseDoStatement()
		{
			Expect(Token.DO);

			DoStatement result = new DoStatement();
			result.body = ParseExpressionStatement();
			if (Expect(Token.WHILE))
			{
				if (Expect(Token.OPEN_PARENS))
				{
					Next();
					result.cond = ParseExpression();
					Expect(Token.CLOSE_PARENS);
				}
			}

			return result;
		}

		public Statement ParseReturnStatement()
		{
			Expect(Token.RETURN);

			ReturnStatement result = new ReturnStatement(ParseExpression());
			Expect(Token.SEMICOLON);
			return result;
		}

		public Statement ParseExpression()
		{
			return null;
		}

		public NamesExpression ParseNamesExpression()
		{
			NamesExpression result = new NamesExpression();
			while (true)
			{
				switch (m_current.token)
				{
					case Token.VOID:
					case Token.OBJECT:
					case Token.BOOL:
					case Token.SBYTE:
					case Token.BYTE:
					case Token.SHORT:
					case Token.USHORT:
					case Token.INT:
					case Token.UINT:
					case Token.LONG:
					case Token.ULONG:
					case Token.FLOAT:
					case Token.DOUBLE:
					case Token.STRING:
					case Token.IDENTIFIER:
					case Token.THIS:
						result.Add(m_current);
						Next();
						break;
					default:
						if (!result.IsEmpty)
							Error("期望一个标识符");
						return result;
				}
				if (m_current.token != Token.DOT)
					return result;
				Next();
			}
		}
	}
}
