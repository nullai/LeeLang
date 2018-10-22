﻿using System;
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
		public void Expect(Token token)
		{
			if (m_current.token == token)
				m_current = m_tokener.Next();
			else
				Error("丢失:" + token);
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

			if (m_current.token == Token.OPEN_BRACE)
			{
				Next();

				result.body = new List<Statement>();
				while (m_current.token != Token.EOF && m_current.token != Token.CLOSE_BRACE)
				{
					result.body.Add(ParseExpressionStatement());
				}

				Expect(Token.CLOSE_BRACE);
			}
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
			return null;
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
			return null;
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
