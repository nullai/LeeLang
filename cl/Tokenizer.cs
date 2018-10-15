using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace leelang
{
	public enum Token
	{
		EOF,

		ABSTRACT,
		AS,
		BASE,
		BOOL,
		BREAK,
		BYTE,
		CASE,
		CATCH,
		CHAR,
		CLASS,
		CONST,
		CONTINUE,
		DEFAULT,
		DELEGATE,
		DO,
		DOUBLE,
		ELSE,
		ENUM,
		EXPLICIT,
		EXTERN,
		FALSE,
		FINALLY,
		FLOAT,
		FOR,
		GOTO,
		IF,
		IMPLICIT,
		IN,
		INT,
		INTERFACE,
		INTERNAL,
		IS,
		LOCK,
		LONG,
		NAMESPACE,
		NEW,
		NULL,
		OBJECT,
		OPERATOR,
		OUT,
		OVERRIDE,
		PARAMS,
		PRIVATE,
		PROTECTED,
		PUBLIC,
		REF,
		RETURN,
		SBYTE,
		SEALED,
		SHORT,
		SIZEOF,
		NAMEOF,
		STATIC,
		STRING,
		STRUCT,
		SWITCH,
		THIS,
		THROW,
		TRUE,
		TRY,
		TYPEOF,
		UINT,
		ULONG,
		USHORT,
		USING,
		VIRTUAL,
		VOID,
		VOLATILE,
		WHERE,
		WHILE,
		PARTIAL,
		ARROW,
		OPEN_BRACE,		// {
		CLOSE_BRACE,	// }
		OPEN_BRACKET,	// [
		CLOSE_BRACKET,	// ]
		OPEN_PARENS,	// (
		CLOSE_PARENS,	// )
		DOT,			// .
		COMMA,			// ,
		COLON,			// :
		SEMICOLON,		// ;
		TILDE,			// ~
		PLUS,			// +
		MINUS,			// -
		BANG,			// !
		ASSIGN,			// =
		OP_LT,
		OP_GT,
		GENERIC_LT,
		GENERIC_GT,
		BITWISE_AND,
		BITWISE_OR,
		STAR,			// *
		PERCENT,		// %
		DIV,			// /
		XOR,			// ^
		INTERR,			// ?
		OP_INC,
		OP_DEC,
		POST_INC,
		POST_DEC,
		OP_SHIFT_LEFT,
		OP_SHIFT_RIGHT,
		OP_LE,
		OP_GE,
		OP_EQ,
		OP_NE,
		OP_AND,
		OP_OR,
		OP_MULT_ASSIGN,
		OP_DIV_ASSIGN,
		OP_MOD_ASSIGN,
		OP_ADD_ASSIGN,
		OP_SUB_ASSIGN,
		OP_SHIFT_LEFT_ASSIGN,
		OP_SHIFT_RIGHT_ASSIGN,
		OP_AND_ASSIGN,
		OP_XOR_ASSIGN,
		OP_OR_ASSIGN,

		LITERAL,
		IDENTIFIER,
	}

	public class LocatedToken
	{
		public Location loc;
		public string value;
		public Token token;

		public LocatedToken(string val, string file, int row, int column)
			: this(val, new Location(file, row, column))
		{
		}

		public LocatedToken(string val, Location loc)
		{
			this.loc = loc;
			this.value = val;

			if (val != null)
				token = TokenFromName(val);
			else
				token = Token.EOF;
		}

		public LocatedToken(Token token, string val, string file, int row, int column)
			: this(token, val, new Location(file, row, column))
		{
		}

		public LocatedToken(Token token, string val, Location loc)
		{
			this.token = token;
			this.loc = loc;
			this.value = val;
		}

		public override string ToString()
		{
			return value;
		}

		public static Token TokenFromName(string name)
		{
			switch (name)
			{
				case "abstract":
					return Token.ABSTRACT;
				case "as":
					return Token.AS;
				case "base":
					return Token.BASE;
				case "bool":
					return Token.BOOL;
				case "break":
					return Token.BREAK;
				case "byte":
					return Token.BYTE;
				case "case":
					return Token.CASE;
				case "catch":
					return Token.CATCH;
				case "char":
					return Token.CHAR;
				case "class":
					return Token.CLASS;
				case "const":
					return Token.CONST;
				case "continue":
					return Token.CONTINUE;
				case "default":
					return Token.DEFAULT;
				case "delegate":
					return Token.DELEGATE;
				case "do":
					return Token.DO;
				case "double":
					return Token.DOUBLE;
				case "else":
					return Token.ELSE;
				case "enum":
					return Token.ENUM;
				case "explicit":
					return Token.EXPLICIT;
				case "extern":
					return Token.EXTERN;
				case "false":
					return Token.FALSE;
				case "finally":
					return Token.FINALLY;
				case "float":
					return Token.FLOAT;
				case "for":
					return Token.FOR;
				case "goto":
					return Token.GOTO;
				case "if":
					return Token.IF;
				case "implicit":
					return Token.IMPLICIT;
				case "in":
					return Token.IN;
				case "int":
					return Token.INT;
				case "interface":
					return Token.INTERFACE;
				case "internal":
					return Token.INTERNAL;
				case "is":
					return Token.IS;
				case "lock":
					return Token.LOCK;
				case "long":
					return Token.LONG;
				case "namespace":
					return Token.NAMESPACE;
				case "new":
					return Token.NEW;
				case "null":
					return Token.NULL;
				case "object":
					return Token.OBJECT;
				case "operator":
					return Token.OPERATOR;
				case "out":
					return Token.OUT;
				case "override":
					return Token.OVERRIDE;
				case "params":
					return Token.PARAMS;
				case "private":
					return Token.PRIVATE;
				case "protected":
					return Token.PROTECTED;
				case "public":
					return Token.PUBLIC;
				case "ref":
					return Token.REF;
				case "return":
					return Token.RETURN;
				case "sbyte":
					return Token.SBYTE;
				case "sealed":
					return Token.SEALED;
				case "short":
					return Token.SHORT;
				case "sizeof":
					return Token.SIZEOF;
				case "nameof":
					return Token.NAMEOF;
				case "static":
					return Token.STATIC;
				case "string":
					return Token.STRING;
				case "struct":
					return Token.STRUCT;
				case "switch":
					return Token.SWITCH;
				case "this":
					return Token.THIS;
				case "throw":
					return Token.THROW;
				case "true":
					return Token.TRUE;
				case "try":
					return Token.TRY;
				case "typeof":
					return Token.TYPEOF;
				case "uint":
					return Token.UINT;
				case "ulong":
					return Token.ULONG;
				case "ushort":
					return Token.USHORT;
				case "using":
					return Token.USING;
				case "virtual":
					return Token.VIRTUAL;
				case "void":
					return Token.VOID;
				case "volatile":
					return Token.VOLATILE;
				case "where":
					return Token.WHERE;
				case "while":
					return Token.WHILE;
				case "partial":
					return Token.PARTIAL;
				case "=>":
					return Token.ARROW;
				case "{":
					return Token.OPEN_BRACE;
				case "}":
					return Token.CLOSE_BRACE;
				case "[":
					return Token.OPEN_BRACKET;
				case "]":
					return Token.CLOSE_BRACKET;
				case "(":
					return Token.OPEN_PARENS;
				case ")":
					return Token.CLOSE_PARENS;
				case ".":
					return Token.DOT;
				case ",":
					return Token.COMMA;
				case ":":
					return Token.COLON;
				case ";":
					return Token.SEMICOLON;
				case "~":
					return Token.TILDE;
				case "+":
					return Token.PLUS;
				case "-":
					return Token.MINUS;
				case "!":
					return Token.BANG;
				case "=":
					return Token.ASSIGN;
				case "<":
					return Token.OP_LT;
				case ">":
					return Token.OP_GT;
				case "&":
					return Token.BITWISE_AND;
				case "|":
					return Token.BITWISE_OR;
				case "*":
					return Token.STAR;
				case "%":
					return Token.PERCENT;
				case "/":
					return Token.DIV;
				case "^":
					return Token.XOR;
				case "?":
					return Token.INTERR;
				case "++":
					return Token.OP_INC;
				case "--":
					return Token.OP_DEC;
				case "<<":
					return Token.OP_SHIFT_LEFT;
				case ">>":
					return Token.OP_SHIFT_RIGHT;
				case "<=":
					return Token.OP_LE;
				case ">=":
					return Token.OP_GE;
				case "==":
					return Token.OP_EQ;
				case "!=":
					return Token.OP_NE;
				case "&&":
					return Token.OP_AND;
				case "||":
					return Token.OP_OR;
				case "*=":
					return Token.OP_MULT_ASSIGN;
				case "/=":
					return Token.OP_DIV_ASSIGN;
				case "%=":
					return Token.OP_MOD_ASSIGN;
				case "+=":
					return Token.OP_ADD_ASSIGN;
				case "-=":
					return Token.OP_SUB_ASSIGN;
				case "<<=":
					return Token.OP_SHIFT_LEFT_ASSIGN;
				case ">>=":
					return Token.OP_SHIFT_RIGHT_ASSIGN;
				case "&=":
					return Token.OP_AND_ASSIGN;
				case "^=":
					return Token.OP_XOR_ASSIGN;
				case "|=":
					return Token.OP_OR_ASSIGN;
				default:
					return Token.IDENTIFIER;
			}
		}
	}
	public struct Tokenizer
	{
		public SourceFile file;
		public int curIndex;

		public LocatedToken Next()
		{
			int idx = curIndex++;
			if (curIndex >= file.Tokens.Count)
				--curIndex;
			return file.Tokens[idx];
		}
		public LocatedToken Current => file.Tokens[curIndex];
		public void Back()
		{
			--curIndex;
		}

		public void Fixup()
		{
			curIndex = 0;
			CloseToken(Token.EOF, Token.EOF);
			curIndex = 0;
		}

		public int Backup()
		{
			return curIndex;
		}
		public void Restore(int val)
		{
			curIndex = val;
		}

		public void NextLine()
		{
			var loc = Current.loc;
			while (curIndex < file.Tokens.Count)
			{
				++curIndex;
				var now = Current.loc;
				if (now.row != loc.row || now.File != loc.File)
					break;
			}
			if (curIndex >= file.Tokens.Count)
				--curIndex;
		}

		private bool CloseToken(Token tk, Token tk2)
		{
			while (true)
			{
				var t = file.Tokens[curIndex];
				if (t.token == Token.EOF)
					return false;
				int idx = curIndex;
				++curIndex;
				if (t.token == tk)
				{
					if (tk == Token.OP_GT)
						t.token = Token.GENERIC_GT;
					return true;
				}

				switch (t.token)
				{
					case Token.OPEN_BRACE:
						CloseToken(Token.CLOSE_BRACE, tk);
						break;
					case Token.OPEN_BRACKET:
						CloseToken(Token.CLOSE_BRACKET, tk);
						break;
					case Token.OPEN_PARENS:
						CloseToken(Token.CLOSE_PARENS, tk);
						break;
					case Token.OP_LT:
						if (idx > 0 && file.Tokens[idx - 1].token != Token.OPERATOR)
						{
							if (CloseToken(Token.OP_GT, tk))
								t.token = Token.GENERIC_LT;
						}
						break;
					case Token.CLOSE_BRACE:
						--curIndex;
						return false;
					case Token.CLOSE_BRACKET:
					case Token.CLOSE_PARENS:
					case Token.SEMICOLON:
						if (tk == Token.OP_GT)
						{
							--curIndex;
							return false;
						}
						break;
					case Token.OP_SHIFT_RIGHT:
						if (tk == Token.OP_GT && tk2 == Token.OP_GT)
						{
							var a = new LocatedToken(Token.GENERIC_GT, ">", t.loc);
							var b = new LocatedToken(Token.GENERIC_GT, ">", t.loc.NextColumn);

							file.Tokens.RemoveAt(idx);
							file.Tokens.Insert(idx, a);
							file.Tokens.Insert(idx + 1, b);
							return true;
						}
						break;
					case Token.GENERIC_GT:
						return true;
				}
			}
		}
	}
	public class TokenExpression
	{
		public LocatedToken opcode;
		public TokenExpression[] value;

		public TokenExpression(LocatedToken opcode, TokenExpression[] value)
		{
			this.opcode = opcode;
			this.value = value;
		}
	}
}
