using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class LoadTokenContext
	{
		public string file;
		public string text;
		public int idx;
		public int row = 1;
		public int column = 1;
		public Stack<TokenValue> close_token = new Stack<TokenValue>();

		private void PopCloseToen(Token tk)
		{
			while (close_token.Count > 0)
			{
				var p = close_token.Peek();
				if (p.token == tk)
				{
					close_token.Pop();
					break;
				}
				if (p.token == Token.OP_LT)
					close_token.Pop();
				else
					break;
			}
		}

		public TokenValue LoadToken()
		{
			TokenValue r;
			string val;
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
					val = LoadNumber();
					r = new TokenValue(Token.NUMBER_LITERAL, val, new Location(file, x, y));
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
					val = LoadIdentifier();
					r = new TokenValue(val, new Location(file, x, y));
					return r;
				case '{':
					get_char();
					r = new TokenValue(Token.OPEN_BRACE, "{", new Location(file, x, y));
					close_token.Push(r);
					return r;
				case '}':
					get_char();
					r = new TokenValue(Token.CLOSE_BRACE, "}", new Location(file, x, y));
					PopCloseToen(Token.OPEN_BRACE);
					return r;
				case '(':
					get_char();
					r = new TokenValue(Token.OPEN_PARENS, "(", new Location(file, x, y));
					close_token.Push(r);
					return r;
				case ')':
					get_char();
					r = new TokenValue(Token.CLOSE_PARENS, ")", new Location(file, x, y));
					PopCloseToen(Token.OPEN_PARENS);
					return r;
				case '[':
					get_char();
					r = new TokenValue(Token.OPEN_BRACKET, "[", new Location(file, x, y));
					close_token.Push(r);
					return r;
				case ']':
					get_char();
					r = new TokenValue(Token.CLOSE_BRACKET, "]", new Location(file, x, y));
					PopCloseToen(Token.OPEN_BRACKET);
					return r;
				case '.':
					get_char();
					r = new TokenValue(Token.DOT, ".", new Location(file, x, y));
					return r;
				case ',':
					get_char();
					r = new TokenValue(Token.COMMA, ",", new Location(file, x, y));
					return r;
				case ':':
					get_char();
					r = new TokenValue(Token.COLON, ":", new Location(file, x, y));
					return r;
				case ';':
					get_char();
					r = new TokenValue(Token.SEMICOLON, ";", new Location(file, x, y));
					PopCloseToen(Token.SEMICOLON);
					return r;
				case '~':
					r = new TokenValue(Token.TILDE, "~", new Location(file, x, y));
					get_char();
					return r;
				case '+':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new TokenValue(Token.OP_ADD_ASSIGN, "+=", new Location(file, x, y));
					}
					else if (peek_char(0) == '+')
					{
						get_char();
						r = new TokenValue(Token.OP_INC, "++", new Location(file, x, y));
					}
					else
						r = new TokenValue(Token.PLUS, "+", new Location(file, x, y));
					return r;
				case '-':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new TokenValue(Token.OP_SUB_ASSIGN, "-=", new Location(file, x, y));
					}
					else if (peek_char(0) == '-')
					{
						get_char();
						r = new TokenValue(Token.OP_DEC, "--", new Location(file, x, y));
					}
					else
						r = new TokenValue(Token.MINUS, "-", new Location(file, x, y));
					return r;
				case '*':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new TokenValue(Token.OP_MULT_ASSIGN, "*=", new Location(file, x, y));
					}
					else
						r = new TokenValue(Token.STAR, "*", new Location(file, x, y));
					return r;
				case '%':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new TokenValue(Token.OP_MOD_ASSIGN, "%=", new Location(file, x, y));
					}
					else
						r = new TokenValue(Token.PERCENT, "%", new Location(file, x, y));
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
						r = new TokenValue(Token.OP_DIV_ASSIGN, "/=", new Location(file, x, y));
					}
					else
						r = new TokenValue(Token.DIV, "/", new Location(file, x, y));
					return r;
				case '!':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new TokenValue(Token.OP_NE, "!=", new Location(file, x, y));
					}
					else
						r = new TokenValue(Token.BANG, "!", new Location(file, x, y));
					return r;
				case '=':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new TokenValue(Token.OP_EQ, "==", new Location(file, x, y));
					}
					else if (peek_char(0) == '>')
					{
						get_char();
						r = new TokenValue(Token.ARROW, "=>", new Location(file, x, y));
					}
					else
						r = new TokenValue(Token.ASSIGN, "!", new Location(file, x, y));
					return r;
				case '<':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new TokenValue(Token.OP_LE, "<=", new Location(file, x, y));
					}
					else if (peek_char(0) == '<')
					{
						get_char();
						if (peek_char(0) == '=')
						{
							get_char();
							r = new TokenValue(Token.OP_SHIFT_LEFT_ASSIGN, "<<=", new Location(file, x, y));
						}
						else
							r = new TokenValue(Token.OP_SHIFT_LEFT, "<<", new Location(file, x, y));
					}
					else
					{
						r = new TokenValue(Token.OP_LT, "<", new Location(file, x, y));
						close_token.Push(r);
					}
					return r;
				case '>':
					get_char();
					if (close_token.Count > 0)
					{
						var p = close_token.Peek();
						if (p.token == Token.OP_LT)
						{
							close_token.Pop();
							p.token = Token.GENERIC_LT;
							r = new TokenValue(Token.GENERIC_GT, ">", new Location(file, x, y));
							return r;
						}
					}
					if (peek_char(0) == '=')
					{
						get_char();
						r = new TokenValue(Token.OP_GE, ">=", new Location(file, x, y));
					}
					else if (peek_char(0) == '>')
					{
						get_char();
						if (peek_char(0) == '=')
						{
							get_char();
							r = new TokenValue(Token.OP_SHIFT_RIGHT_ASSIGN, ">>=", new Location(file, x, y));
						}
						else
							r = new TokenValue(Token.OP_SHIFT_RIGHT, ">>", new Location(file, x, y));
					}
					else
						r = new TokenValue(Token.OP_GT, ">", new Location(file, x, y));
					return r;
				case '&':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new TokenValue(Token.OP_AND_ASSIGN, "&=", new Location(file, x, y));
					}
					else if (peek_char(0) == '&')
					{
						get_char();
						r = new TokenValue(Token.OP_AND, "&&", new Location(file, x, y));
					}
					else
						r = new TokenValue(Token.BITWISE_AND, "&", new Location(file, x, y));
					return r;
				case '|':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new TokenValue(Token.OP_OR_ASSIGN, "|=", new Location(file, x, y));
					}
					else if (peek_char(0) == '|')
					{
						get_char();
						r = new TokenValue(Token.OP_OR, "||", new Location(file, x, y));
					}
					else
						r = new TokenValue(Token.BITWISE_OR, "|", new Location(file, x, y));
					return r;
				case '^':
					get_char();
					if (peek_char(0) == '=')
					{
						get_char();
						r = new TokenValue(Token.OP_XOR_ASSIGN, "^=", new Location(file, x, y));
					}
					else
						r = new TokenValue(Token.XOR, "^", new Location(file, x, y));
					return r;
				case '?':
					get_char();
					r = new TokenValue(Token.INTERR, "?", new Location(file, x, y));
					return r;
				case '\'':
				case '\"':
					val = LoadString(false);
					r = new TokenValue(Token.STRING_LITERAL, val, new Location(file, x, y));
					return r;
				case '@':
					if (peek_char(0) == '\'' || peek_char(0) == '\"')
					{
						get_char();
						val = LoadString(true);
						r = new TokenValue(Token.STRING_LITERAL, val, new Location(file, x, y));
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
	public static class TokenReader
	{
		public static TokenValue[] Load(string pathname)
		{
			List<TokenValue> tokens = new List<TokenValue>();
			tokens.Add(new TokenValue(Token.EOF, null, new Location(pathname, 1, 1)));
			string text = File.ReadAllText(pathname);
			LoadTokenContext ctx = new LoadTokenContext();
			ctx.file = pathname;
			ctx.text = text;
			ctx.idx = 0;
			ctx.row = 1;
			ctx.column = 1;
			while (true)
			{
				var tk = ctx.LoadToken();
				if (tk == null)
					break;
				tokens.Add(tk);
			}
			tokens.Add(new TokenValue(Token.EOF, null, new Location(pathname, ctx.row, ctx.column)));
			return tokens.ToArray();
		}
	}
}
