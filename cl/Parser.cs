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
		public void Waring(string msg)
		{
			Console.WriteLine("WARING:" + msg);
		}
		public FileStatement ParseFile()
		{
			FileStatement result = new FileStatement(m_tokener.file.FullPathName);

			while (m_current.token != Token.EOF)
			{
				result.members.Add(ParseFileMember());
			}

			return result;
		}

		public Statement ParseFileMember()
		{
			switch (m_current.token)
			{
				case Token.USING:
					Next();
					return ParseUsing();
				default:
					return ParseNamespaceMember();
			}
		}

		public Statement ParseNamespaceMember()
		{
			switch (m_current.token)
			{
				case Token.NAMESPACE:
					Next();
					return ParseNamespace();
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
				space.members.Add(ParseNamespaceMember());
			}
			Expect(Token.CLOSE_BRACE);
			return space;
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
