using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace leelang
{
	public class LoadTokenContext
	{
		public string file;
		public string text;
		public int idx;
		public int row;
		public int column;

		public LocatedToken LoadToken()
		{
			LocatedToken r;
			_start:
			int x = row;
			int y = column;
			switch (peek_char(0))
			{
				case '\0':
					return null;
				case ' ':
				case '\r':
				case '\n':
				case '\t':
					get_char();
					goto _start;
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					r = new LocatedToken(Token.LITERAL, LoadNumber(), file, x, y);
					return r;
				case 'a':
				case 'b':
				case 'c':
				case 'd':
				case 'e':
				case 'f':
				case 'g':
				case 'h':
				case 'i':
				case 'j':
				case 'k':
				case 'l':
				case 'm':
				case 'n':
				case 'o':
				case 'p':
				case 'q':
				case 'r':
				case 's':
				case 't':
				case 'u':
				case 'v':
				case 'w':
				case 'x':
				case 'y':
				case 'z':
				case 'A':
				case 'B':
				case 'C':
				case 'D':
				case 'E':
				case 'F':
				case 'G':
				case 'H':
				case 'I':
				case 'J':
				case 'K':
				case 'L':
				case 'M':
				case 'N':
				case 'O':
				case 'P':
				case 'Q':
				case 'R':
				case 'S':
				case 'T':
				case 'U':
				case 'V':
				case 'W':
				case 'X':
				case 'Y':
				case 'Z':
				case '_':
					r = new LocatedToken(LoadIdentifier(), file, x, y);
					return r;
				case '{':
					r = new LocatedToken(Token.OPEN_BRACE, "{", file, x, y);
					get_char();
					return r;
				case '}':
					r = new LocatedToken(Token.CLOSE_BRACE, "}", file, x, y);
					get_char();
					return r;
				case '(':
					r = new LocatedToken(Token.OPEN_PARENS, "(", file, x, y);
					get_char();
					return r;
				case ')':
					r = new LocatedToken(Token.CLOSE_PARENS, ")", file, x, y);
					get_char();
					return r;
				case '[':
					r = new LocatedToken(Token.OPEN_BRACKET, "[", file, x, y);
					get_char();
					return r;
				case ']':
					r = new LocatedToken(Token.CLOSE_BRACKET, "]", file, x, y);
					get_char();
					return r;
				case '.':
					r = new LocatedToken(Token.DOT, ".", file, x, y);
					get_char();
					return r;
				case ',':
					r = new LocatedToken(Token.COMMA, ",", file, x, y);
					get_char();
					return r;
				case ':':
					r = new LocatedToken(Token.COLON, ":", file, x, y);
					get_char();
					return r;
				case ';':
					r = new LocatedToken(Token.SEMICOLON, ";", file, x, y);
					get_char();
					return r;
				case '~':
					r = new LocatedToken(Token.TILDE, "~", file, x, y);
					get_char();
					return r;
				case '+':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new LocatedToken(Token.OP_ADD_ASSIGN, "+=", file, x, y);
					}
					else if (peek_char(0) == '+')
					{
						get_char();
						r = new LocatedToken(Token.OP_INC, "++", file, x, y);
					}
					else
						r = new LocatedToken(Token.PLUS, "+", file, x, y);
					return r;
				case '-':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new LocatedToken(Token.OP_SUB_ASSIGN, "-=", file, x, y);
					}
					else if (peek_char(0) == '-')
					{
						get_char();
						r = new LocatedToken(Token.OP_DEC, "--", file, x, y);
					}
					else
						r = new LocatedToken(Token.MINUS, "-", file, x, y);
					return r;
				case '*':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new LocatedToken(Token.OP_MULT_ASSIGN, "*=", file, x, y);
					}
					else
						r = new LocatedToken(Token.STAR, "*", file, x, y);
					return r;
				case '%':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new LocatedToken(Token.OP_MOD_ASSIGN, "%=", file, x, y);
					}
					else
						r = new LocatedToken(Token.PERCENT, "%", file, x, y);
					return r;
				case '/':
					get_char();
					if (peek_char(0) == '/')
					{
						while (true)
						{
							char ch = get_char();
							if (ch == '\n')
								goto _start;
							if (ch == '\0')
								return null;
						}
					}
					if (peek_char(0) == '*')
					{
						get_char();
						while (true)
						{
							char ch = get_char();
							if (ch == '*' && peek_char(0) == '/')
							{
								get_char();
								goto _start;
							}
							if (ch == '\0')
								throw new Exception("不期望的文件结尾");
						}
					}
					if (peek_char(0) == '=')
					{
						get_char();
						r = new LocatedToken(Token.OP_DIV_ASSIGN, "/=", file, x, y);
					}
					else
						r = new LocatedToken(Token.DIV, "/", file, x, y);
					return r;
				case '!':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new LocatedToken(Token.OP_NE, "!=", file, x, y);
					}
					else
						r = new LocatedToken(Token.BANG, "!", file, x, y);
					return r;
				case '=':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new LocatedToken(Token.OP_EQ, "==", file, x, y);
					}
					else if (peek_char(0) == '>')
					{
						get_char();
						r = new LocatedToken(Token.ARROW, "=>", file, x, y);
					}
					else
						r = new LocatedToken(Token.ASSIGN, "!", file, x, y);
					return r;
				case '<':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new LocatedToken(Token.OP_LE, "<=", file, x, y);
					}
					else if (peek_char(0) == '<')
					{
						get_char();
						if (peek_char(0) == '=')
						{
							get_char();
							r = new LocatedToken(Token.OP_SHIFT_LEFT_ASSIGN, "<<=", file, x, y);
						}
						else
							r = new LocatedToken(Token.OP_SHIFT_LEFT, "<<", file, x, y);
					}
					else
						r = new LocatedToken(Token.OP_LT, "<", file, x, y);
					return r;
				case '>':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new LocatedToken(Token.OP_GE, ">=", file, x, y);
					}
					else if (peek_char(0) == '>')
					{
						get_char();
						if (peek_char(0) == '=')
						{
							get_char();
							r = new LocatedToken(Token.OP_SHIFT_RIGHT_ASSIGN, ">>=", file, x, y);
						}
						else
							r = new LocatedToken(Token.OP_SHIFT_RIGHT, ">>", file, x, y);
					}
					else
						r = new LocatedToken(Token.OP_GT, ">", file, x, y);
					return r;
				case '&':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new LocatedToken(Token.OP_AND_ASSIGN, "&=", file, x, y);
					}
					else if (peek_char(0) == '&')
					{
						get_char();
						r = new LocatedToken(Token.OP_AND, "&&", file, x, y);
					}
					else
						r = new LocatedToken(Token.BITWISE_AND, "&", file, x, y);
					return r;
				case '|':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new LocatedToken(Token.OP_OR_ASSIGN, "|=", file, x, y);
					}
					else if (peek_char(0) == '|')
					{
						get_char();
						r = new LocatedToken(Token.OP_OR, "||", file, x, y);
					}
					else
						r = new LocatedToken(Token.BITWISE_OR, "|", file, x, y);
					return r;
				case '^':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new LocatedToken(Token.OP_XOR_ASSIGN, "^=", file, x, y);
					}
					else
						r = new LocatedToken(Token.XOR, "^", file, x, y);
					return r;
				case '?':
					get_char();
					r = new LocatedToken(Token.INTERR, "?", file, x, y);
					return r;
				case '\'':
				case '\"':
					r = new LocatedToken(Token.LITERAL, LoadString(false), file, x, y);
					return r;
				case '@':
					if (peek_char(0) == '\'' || peek_char(0) == '\"')
					{
						get_char();
						r = new LocatedToken(Token.LITERAL, LoadString(true), file, x, y);
						return r;
					}
					goto default;
				default:
					throw new Exception("无效字符:" + peek_char(0));
			}
		}
		private char peek_char(int offset)
		{
			int i = idx + offset;
			if (i < text.Length)
				return text[i];
			return '\0';
		}
		private char get_char()
		{
			char ch = text[idx++];
			switch (ch)
			{
				case '\n':
					++row;
					column = 1;
					break;
				case '\r':
					break;
				case '\t':
					column = (column + 1) & ~3;
					break;
				default:
					++column;
					break;
			}
			return ch;
		}
		private string LoadNumber()
		{
			int a = idx;
			if (peek_char(0) == '0')
			{
				get_char();
				if (peek_char(0) == 'x' || peek_char(0) == 'X')
				{
					get_char();
					while (IsHexChar(peek_char(0)))
						get_char();

					return text.Substring(a, idx - a);
				}

				while (IsOtcChar(peek_char(0)))
					get_char();

				if (peek_char(0) == 'u')
					get_char();
				return text.Substring(a, idx - a);
			}

			while (IsNumChar(peek_char(0)))
				get_char();

			bool real = false;
			if (peek_char(0) == '.' && IsNumChar(peek_char(1)))
			{
				real = true;
				get_char();
				get_char();
				while (IsNumChar(peek_char(0)))
					get_char();
			}

			switch (peek_char(0))
			{
				case 'f':
					if (real)
						get_char();
					break;
				case 'e':
				case 'E':
					if (real)
					{
						get_char();
						if (peek_char(0) == '-')
							get_char();
						while (IsNumChar(peek_char(0)))
							get_char();
					}
					break;
				case 'u':
					if (!real)
						get_char();
					break;
			}

			return text.Substring(a, idx - a);
		}
		private static bool IsHexChar(char ch)
		{
			if (ch >= '0' && ch <= '9')
				return true;
			if (ch >= 'A' && ch <= 'F')
				return true;
			if (ch >= 'a' && ch <= 'f')
				return true;
			return false;
		}

		private static bool IsOtcChar(char ch)
		{
			if (ch >= '0' && ch <= '7')
				return true;
			return false;
		}

		private static bool IsNumChar(char ch)
		{
			if (ch >= '0' && ch <= '7')
				return true;
			return false;
		}
		private string LoadIdentifier()
		{
			int a = idx;
			get_char();
			while (IsIdentifierChar(peek_char(0)))
			{
				get_char();
			}
			return text.Substring(a, idx - a);
		}
		private static bool IsIdentifierChar(char ch)
		{
			if (ch >= 'a' && ch <= 'z')
				return true;
			if (ch >= 'A' && ch <= 'Z')
				return true;
			if (ch >= '0' && ch <= '9')
				return true;
			return ch == '_';
		}
		private string LoadString(bool raw)
		{
			int a = idx;
			if (raw)
				--a;
			char end = get_char();
			while (true)
			{
				char ch = get_char();
				if (ch == end)
					return text.Substring(a, idx - a);

				if (!raw && ch == '\\')
				{
					ch = get_char();
				}

				if (ch == '\0')
				{
					throw new Exception("不期望的文件结尾");
				}
			}
		}
	}
	public class SourceFile
	{
		public string Name;
		public string FullPathName;
		public List<LocatedToken> Tokens;

		public SourceFile(string pathName)
		{
			FullPathName = pathName;
			Name = Path.GetFileName(pathName);
		}

		public void Load()
		{
			Tokens = new List<LocatedToken>();
			string text = File.ReadAllText(FullPathName);
			LoadTokenContext ctx = new LoadTokenContext();
			ctx.file = FullPathName;
			ctx.text = text;
			ctx.idx = 0;
			ctx.row = 1;
			ctx.column = 1;
			while (true)
			{
				var tk = ctx.LoadToken();
				if (tk == null)
					break;
				Tokens.Add(tk);
			}
			Tokens.Add(new LocatedToken(Token.EOF, null, FullPathName, ctx.row, ctx.column));
		}
	}
}
