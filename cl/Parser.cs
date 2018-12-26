using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class Parser
	{
		public string mFilePath;
		public TokenValue[] mTokens;
		public int mIndex = 0;
		public Stack<List<Error>> mErrorStack = new Stack<List<Error>>();
		private List<Error> mErrors;
		public Parser(string pathname)
		{
			mFilePath = pathname;
			PushError();
		}
		public void Skip(Token token)
		{
			if (mTokens[mIndex].token == token)
				++mIndex;
		}
		public bool Expect(Token token)
		{
			if (mTokens[mIndex].token != token)
			{
				Error(mTokens[mIndex - 1].loc.EndLocation, "期望 " + TokenValue.GetName(token));
				return false;
			}
			++mIndex;
			return true;
		}
		public void NotExpect()
		{
			Error(mTokens[mIndex].loc, "不期望的 " + mTokens[mIndex].ToString());
		}
		public void Error(string msg)
		{
			mErrors.Add(new Error("ERROR:" + msg));
		}
		public void Error(Location pos, string msg)
		{
			mErrors.Add(new Error(pos, "ERROR:" + msg));
		}
		public void Waring(string msg)
		{
			mErrors.Add(new Error("WARING:" + msg));
		}
		public void Waring(Location pos, string msg)
		{
			mErrors.Add(new Error(pos, "WARING:" + msg));
		}
		public void FlushError()
		{
			List<Error> errs = null;
			while (mErrorStack.Count > 0)
			{
				var e = mErrorStack.Pop();
				if (errs == null)
					errs = e;
				else
					errs.InsertRange(0, e);
			}
			PushError(errs);

			for (int i = 0; i < errs.Count; i++)
			{
				var e = errs[i];
				if (e.pos.IsValid)
					Console.WriteLine("{0}: {1}", e.pos.ToString(), e.msg);
				else
					Console.WriteLine(e.msg);
			}
		}
		private void PushError()
		{
			mErrorStack.Push(mErrors = new List<Error>());
		}
		private void PushError(List<Error> errs)
		{
			mErrorStack.Push(mErrors = errs);
		}
		private List<Error> PopError()
		{
			var r = mErrorStack.Pop();
			mErrors = mErrorStack.Peek();
			return r;
		}
		private void AppendErrors(List<Error> errs)
		{
			mErrors.AddRange(errs);
		}
		public FileStatement ParseFile()
		{
			try
			{
				mTokens = TokenReader.Load(mFilePath);
			}
			catch(Exception ex)
			{
				Error("打开文件失败:" + ex.Message);
				return null;
			}
			FileStatement result = new FileStatement(mFilePath);

			while (mTokens[mIndex].token != Token.EOF)
			{
				var s = ParseDeclareStatement(null);
				if (s != null)
					result.members.Add(s);
			}

			return result;
		}

		private Statement ParseDeclareStatement(Statement parent)
		{
			Statement r = null;
			int start = mIndex;
			switch (mTokens[mIndex].token)
			{
				case Token.SEMICOLON:
					++mIndex;
					return null;
				case Token.USING:
					r = ParseUsingStatement();
					break;
				case Token.NAMESPACE:
					if (parent != null && !(parent is NamespaceStatement))
					{
						NotExpect();
						++mIndex;
						return null;
					}
					r = ParseNamespaceStatement();
					break;
				default:
					var attr = ParseCommonAttribute();
					switch (mTokens[mIndex].token)
					{
						case Token.CLASS:
						case Token.STRUCT:
						case Token.ENUM:
							r = ParseTypeStatement(attr);
							break;
						default:
							Error(mTokens[mIndex].loc, "不期望的 " + mTokens[mIndex].ToString());
							++mIndex;
							break;
					}
					break;
			}
			return r;
		}
		private Statement ParseUsingStatement()
		{
			++mIndex;
			NameExpression name = null;
			Expression val;
			if (mTokens[mIndex].token == Token.IDENTIFIER && mTokens[mIndex + 1].token == Token.ASSIGN)
			{
				name = new NameExpression(mTokens[mIndex]);
				mIndex += 2;
			}
			val = ParseTypeExpression(false);
			if (val == null)
				return null;

			Expect(Token.SEMICOLON);
			return new UsingStatement(name, val);
		}
		private Expression ParseTypeExpression(bool accept_array)
		{
			var k = mTokens[mIndex];
			switch (k.token)
			{
				case Token.VOID:
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
				case Token.OBJECT:
				case Token.IDENTIFIER:
					Expression expr = new NameExpression(k);
					++mIndex;
					while (true)
					{
						switch (mTokens[mIndex].token)
						{
							case Token.DOT:
								++mIndex;
								k = mTokens[mIndex];
								if (!Expect(Token.IDENTIFIER))
									return expr;
								expr = new AccessExpression(expr, k);
								break;
							case Token.GENERIC_LT:
								++mIndex;
								GenericExpression ge = new GenericExpression(expr);
								Expression arg;
								if (mTokens[mIndex].token != Token.GENERIC_GT)
								{
									while (true)
									{
										arg = ParseTypeExpression(accept_array);
										if (arg == null)
											++mIndex;
										ge.Add(arg);

										if (mTokens[mIndex].token != Token.GENERIC_GT)
											Expect(Token.COMMA);
										else
											break;
									}
								}
								Skip(Token.GENERIC_GT);
								expr = ge;
								break;
							case Token.OPEN_BRACKET:
								if (!accept_array)
									return expr;
								++mIndex;
								Expect(Token.CLOSE_BRACKET);
								expr = new ArrayExpression(expr);
								break;
							case Token.STAR:
								if (!accept_array)
									return expr;
								++mIndex;
								expr = new PointerExpression(expr);
								break;
							default:
								return expr;
						}
					}
				default:
					Error(mTokens[mIndex - 1].loc.EndLocation, "期望一个标识符");
					return null;
			}
		}
		private Expression ParseNameExpression()
		{
			var k = mTokens[mIndex];
			if (!Expect(Token.IDENTIFIER))
				return null;

			Expression expr = new NameExpression(k);
			while (true)
			{
				switch (mTokens[mIndex].token)
				{
					case Token.DOT:
						++mIndex;
						k = mTokens[mIndex];
						if (Expect(Token.IDENTIFIER))
							expr = new AccessExpression(expr, k);
						else
							return expr;
						break;
					default:
						return expr;
				}
			}
		}
		private Expression ParseExpression()
		{
			var expr = ParseConstantExpression();
			if (expr != null && expr.IsLeftExpression)
			{
				switch (mTokens[mIndex].token)
				{
					case Token.ASSIGN:
						++mIndex;
						expr = new AssignExpression(expr, ParseExpression());
						break;
					case Token.OP_MULT_ASSIGN:
						++mIndex;
						expr = new MultAssignExpression(expr, ParseExpression());
						break;
					case Token.OP_DIV_ASSIGN:
						++mIndex;
						expr = new DivAssignExpression(expr, ParseExpression());
						break;
					case Token.OP_MOD_ASSIGN:
						++mIndex;
						expr = new ModAssignExpression(expr, ParseExpression());
						break;
					case Token.OP_ADD_ASSIGN:
						++mIndex;
						expr = new AddAssignExpression(expr, ParseExpression());
						break;
					case Token.OP_SUB_ASSIGN:
						++mIndex;
						expr = new SubAssignExpression(expr, ParseExpression());
						break;
					case Token.OP_SHIFT_LEFT_ASSIGN:
						++mIndex;
						expr = new ShiftLeftAssignExpression(expr, ParseExpression());
						break;
					case Token.OP_SHIFT_RIGHT_ASSIGN:
						++mIndex;
						expr = new ShiftRightAssignExpression(expr, ParseExpression());
						break;
					case Token.OP_AND_ASSIGN:
						++mIndex;
						expr = new AndAssignExpression(expr, ParseExpression());
						break;
					case Token.OP_XOR_ASSIGN:
						++mIndex;
						expr = new XorAssignExpression(expr, ParseExpression());
						break;
					case Token.OP_OR_ASSIGN:
						++mIndex;
						expr = new OrAssignExpression(expr, ParseExpression());
						break;
				}
			}
			return expr;
		}

		private Expression ParseConstantExpression()
		{
			return null;
		}
		private CommonAttribute ParseCommonAttribute()
		{
			CommonAttribute attr = CommonAttribute.NONE;
			while (true)
			{
				CommonAttribute add = CommonAttribute.NONE;
				switch (mTokens[mIndex].token)
				{
					case Token.PUBLIC:
						add = CommonAttribute.PUBLIC;
						break;
					case Token.PROTECTED:
						add = CommonAttribute.PROTECTED;
						break;
					case Token.PRIVATE:
						add = CommonAttribute.PRIVATE;
						break;
					case Token.STATIC:
						add = CommonAttribute.STATIC;
						break;
					case Token.CONST:
						add = CommonAttribute.CONST;
						break;
					case Token.FINALLY:
						add = CommonAttribute.FINALLY;
						break;
					case Token.EXPLICIT:
						add = CommonAttribute.EXPLICIT;
						break;
					case Token.EXTERN:
						add = CommonAttribute.EXTERN;
						break;
					case Token.IMPLICIT:
						add = CommonAttribute.IMPLICIT;
						break;
					case Token.INTERNAL:
						add = CommonAttribute.INTERNAL;
						break;
					case Token.OVERRIDE:
						add = CommonAttribute.OVERRIDE;
						break;
					case Token.SEALED:
						add = CommonAttribute.SEALED;
						break;
					case Token.VIRTUAL:
						add = CommonAttribute.VIRTUAL;
						break;
					case Token.VOLATILE:
						add = CommonAttribute.VOLATILE;
						break;
					case Token.PARTIAL:
						add = CommonAttribute.PARTIAL;
						break;
					default:
						return attr;
				}

				if ((attr & add) != CommonAttribute.NONE)
					Error(mTokens[mIndex].loc, "重复声明属性 " + mTokens[mIndex].ToString());
				++mIndex;
				attr |= add;
			}
		}

		private ParamAttribute ParseParamAttribute(bool have_this)
		{
			ParamAttribute attr = ParamAttribute.NONE;
			while (true)
			{
				ParamAttribute add = ParamAttribute.NONE;
				switch (mTokens[mIndex].token)
				{
					case Token.IN:
						add = ParamAttribute.IN;
						break;
					case Token.OUT:
						add = ParamAttribute.OUT;
						break;
					case Token.REF:
						add = ParamAttribute.REF;
						break;
					case Token.PARAMS:
						add = ParamAttribute.PARAMS;
						break;
					case Token.THIS:
						if (!have_this)
							return attr;
						add = ParamAttribute.THIS;
						break;
					default:
						return attr;
				}

				if ((attr & add) != ParamAttribute.NONE)
					Error(mTokens[mIndex].loc, "重复声明属性 " + mTokens[mIndex].ToString());
				++mIndex;
				attr |= add;
			}
		}

		private Statement ParseNamespaceStatement()
		{
			++mIndex;
			var name = ParseNameExpression();
			if (name == null)
				return null;

			if (!Expect(Token.OPEN_BRACE))
				return null;

			NamespaceStatement result = new NamespaceStatement(name);

			while (mTokens[mIndex].token != Token.CLOSE_BRACE && mTokens[mIndex].token != Token.EOF)
			{
				var s = ParseDeclareStatement(result);
				if (s != null)
					result.members.Add(s);
			}

			Expect(Token.CLOSE_BRACE);
			return result;
		}

		private TypeStatement ParseTypeStatement(CommonAttribute attr)
		{
			var cls_or_strct = mTokens[mIndex].token;
			++mIndex;
			var name = ParseTypeExpression(false);
			if (name == null)
				return null;

			TypeStatement result = new TypeStatement(attr, cls_or_strct, name);
			if (!Expect(Token.OPEN_BRACE))
				return result;

			if (cls_or_strct == Token.ENUM)
			{
				while (mTokens[mIndex].token == Token.IDENTIFIER)
				{
					var id = mTokens[mIndex];
					++mIndex;
					Expression val = null;
					if (mTokens[mIndex].token == Token.ASSIGN)
					{
						++mIndex;
						val = ParseExpression();
					}

					result.members.Add(new EnumFieldStatement(new NameExpression(id), val));

					if (mTokens[mIndex].token != Token.COMMA)
						break;
					++mIndex;
				}

				if (mTokens[mIndex].token != Token.CLOSE_BRACE && mTokens[mIndex].token != Token.SEMICOLON)
					Expect(Token.SEMICOLON);
			}

			while (mTokens[mIndex].token != Token.CLOSE_BRACE && mTokens[mIndex].token != Token.EOF)
			{
				var s = ParseDeclareStatement(result);
				if (s != null)
					result.members.Add(s);
			}

			Expect(Token.CLOSE_BRACE);
			return result;
		}
	}
}
