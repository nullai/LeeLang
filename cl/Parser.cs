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
		public Compiler mCompiler;
		public Parser(Compiler comp)
		{
			mCompiler = comp;
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
			mErrors.Add(new Error("ERROR:" + msg, false));
		}
		public void Error(Location pos, string msg)
		{
			mErrors.Add(new Error(pos, "ERROR:" + msg, false));
		}
		public void Waring(string msg)
		{
			mErrors.Add(new Error("WARING:" + msg, true));
		}
		public void Waring(Location pos, string msg)
		{
			mErrors.Add(new Error(pos, "WARING:" + msg, true));
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

			for (int i = 0; i < errs.Count; i++)
			{
				var e = errs[i];
				if (e.warn)
				{
					if (e.pos.IsValid)
						mCompiler.OutputWarning(string.Format("{0}: {1}", e.pos.ToString(), e.msg));
					else
						mCompiler.OutputWarning(e.msg);
				}
				else
				{
					if (e.pos.IsValid)
						mCompiler.OutputError(string.Format("{0}: {1}", e.pos.ToString(), e.msg));
					else
						mCompiler.OutputError(e.msg);
				}
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
		public FileStatement ParseFile(string pathname)
		{
			mFilePath = pathname;
			PushError();
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

			mIndex = 1;
			while (mTokens[mIndex].token != Token.EOF)
			{
				int next = mIndex + 1;
				var s = ParseDeclareStatement(null);
				if (s != null)
					result.members.Add(s);
				else if (mIndex < next)
				{
					NotExpect();
					mIndex = next;
				}
			}
			FlushError();
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
					var attr = ParseCommonAttribute(true);
					switch (mTokens[mIndex].token)
					{
						case Token.CLASS:
						case Token.STRUCT:
						case Token.ENUM:
						case Token.INTERFACE:
							r = ParseTypeStatement(attr);
							break;
						default:
							r = ParseDeclareFieldMethodStatement(attr, parent);
							break;
					}
					break;
			}
			return r;
		}
		private void ParsePropertyBody(PropertyStatement prop)
		{
			if (!Expect(Token.OPEN_BRACE))
				return;
			do
			{
				var attr = ParseCommonAttribute(false);
				var name = mTokens[mIndex];
				if (!Expect(Token.IDENTIFIER))
					break;

				var method = new MethodStatement(attr, null, null, new NameExpression(name));
				method.body = ParseBlockStatement();
				prop.value.Add(method);
			} while (mTokens[mIndex].token != Token.CLOSE_BRACE);
			Expect(Token.CLOSE_BRACE);
		}
		private Statement ParseDeclareFieldMethodStatement(CommonAttribute attr, Statement parent)
		{
			MethodStatement method = null;
			TypeStatement cls = parent as TypeStatement;
			if (cls != null)
			{
				// 构造函数
				if (mTokens[mIndex].token == Token.IDENTIFIER && mTokens[mIndex].value == cls.name.token.value && mTokens[mIndex + 1].token == Token.OPEN_PARENS)
				{
					mIndex += 2;

					method = new MethodStatement(attr, cls.name, null, cls.name);
					method.parameters = ParseParameters(Token.CLOSE_PARENS);
					if(Expect(Token.CLOSE_PARENS) || mTokens[mIndex].token == Token.OPEN_PARENS)
						method.body = ParseBlockStatement();
					return method;
				}
			}

			var type = ParseTypeExpression(true, false);
			if (type == null)
				return null;

			TokenValue inner = null;
			var name = mTokens[mIndex];
			if (name.token == Token.THIS)
			{
				++mIndex;
				if (!Expect(Token.OPEN_BRACKET))
					return null;
				PropertyStatement prop = new PropertyStatement(attr, type, null, new NameExpression(name));
				prop.parameters = ParseParameters(Token.CLOSE_BRACKET);
				if (Expect(Token.CLOSE_BRACKET) || mTokens[mIndex].token == Token.OPEN_BRACE)
					ParsePropertyBody(prop);
				return prop;
			}
			if (!Expect(Token.IDENTIFIER))
				return null;

			if (mTokens[mIndex].token == Token.DOT)
			{
				++mIndex;
				inner = name;
				name = mTokens[mIndex];
				if (!Expect(Token.IDENTIFIER))
					return null;
			}
			if (mTokens[mIndex].token == Token.OPEN_PARENS)
			{
				++mIndex;
				method = new MethodStatement(attr, type, inner == null ? null : new NameExpression(inner), new NameExpression(name));
				method.parameters = ParseParameters(Token.CLOSE_PARENS);
				if (Expect(Token.CLOSE_PARENS) || mTokens[mIndex].token == Token.OPEN_PARENS)
					method.body = ParseBlockStatement();
				return method;
			}
			if (mTokens[mIndex].token == Token.OPEN_BRACE)
			{
				PropertyStatement prop = new PropertyStatement(attr, type, inner == null ? null : new NameExpression(inner), new NameExpression(name));
				ParsePropertyBody(prop);
				return prop;
			}

			if (inner != null)
			{
				NotExpect();
				return null;
			}

			--mIndex;
			var result = ParseDeclareFieldValueStatement(attr, type);
			Expect(Token.SEMICOLON);
			return result;
		}
		private Statement ParseDeclareFieldStatement()
		{
			CommonAttribute attr = ParseCommonAttribute(false);
			var type = ParseTypeExpression(true, false);
			if (type == null)
				return null;

			var result = ParseDeclareFieldValueStatement(attr, type);
			Expect(Token.SEMICOLON);
			return result;
		}
		private Statement ParseDeclareFieldValueStatement(CommonAttribute attr, Expression type)
		{
			FieldStatement field = null;
			FieldStatement last = null;
			do
			{
				if (field != null)
					++mIndex;

				var name = mTokens[mIndex];
				if (!Expect(Token.IDENTIFIER))
					return field;
				FieldStatement next = new FieldStatement(attr, type, new NameExpression(name));
				if (mTokens[mIndex].token == Token.ASSIGN)
				{
					++mIndex;
					next.value = ParseValueExpression();
				}
				if (field == null)
					field = next;
				else
					last.next = next;
				last = next;
			}
			while (mTokens[mIndex].token == Token.COMMA);

			return field;
		}
		private List<ParameterStatement> ParseParameters(Token term)
		{
			List<ParameterStatement> ps = new List<ParameterStatement>();
			if (mTokens[mIndex].token == term)
				return ps;
			while (true)
			{
				var attr = ParseParamAttribute(true);
				var type = ParseTypeExpression(true, false);
				if (type == null)
					break;
				TokenValue name = null;
				if (mTokens[mIndex].token == Token.IDENTIFIER)
					name = mTokens[mIndex++];

				ParameterStatement p = new ParameterStatement(attr, type, name == null ? null : new NameExpression(name));
				ps.Add(p);
				if (mTokens[mIndex].token != Token.COMMA)
					break;
				++mIndex;
			}
			return ps;
		}
		private BlockStatement ParseBlockStatement()
		{
			if (!Expect(Token.OPEN_BRACE))
				return null;

			BlockStatement result = new BlockStatement();
			while (mTokens[mIndex].token != Token.CLOSE_BRACE && mTokens[mIndex].token != Token.EOF)
			{
				int next = mIndex + 1;
				var s = ParseStatement();
				if (s != null)
					result.values.Add(s);
				else if (mIndex < next)
				{
					NotExpect();
					mIndex = next;
				}
			}
			Expect(Token.CLOSE_BRACE);
			return result;
		}
		private Statement ParseStatement()
		{
			Statement result = null;
			switch (mTokens[mIndex].token)
			{
				case Token.SEMICOLON:
					++mIndex;
					return new EmptyStatement();
				case Token.OPEN_BRACE:
					return ParseBlockStatement();
				case Token.IF:
					return ParseIfStatement();
				case Token.FOR:
					return ParseForStatement();
				case Token.FOREACH:
					return ParseForeachStatement();
				case Token.WHILE:
					return ParseWhileStatement();
				case Token.DO:
					return ParseDoWhileStatement();
				case Token.SWITCH:
					return ParseSwitchStatement();
				case Token.CASE:
					++mIndex;
					result = new CaseStatement(ParseConstantExpression());
					Expect(Token.COLON);
					return result;
				case Token.RETURN:
					++mIndex;
					result = new ReturnStatement(mTokens[mIndex].token == Token.SEMICOLON ? null : ParseExpression());
					Expect(Token.SEMICOLON);
					return result;
				case Token.BREAK:
					++mIndex;
					Expect(Token.SEMICOLON);
					return new BreakStatement();
				case Token.CONTINUE:
					++mIndex;
					Expect(Token.SEMICOLON);
					return new ContinueStatement();
				case Token.IDENTIFIER:
					if (mTokens[mIndex + 1].token == Token.COLON)
					{
						result = new LabelStatement(mTokens[mIndex]);
						mIndex += 2;
						return result;
					}
					break;
				case Token.GOTO:
					++mIndex;
					if (Expect(Token.IDENTIFIER))
						result = new GotoStatement(mTokens[mIndex - 1]);
					Expect(Token.SEMICOLON);
					return result;
			}

			var attr = ParseCommonAttribute(false);
			if (attr != CommonAttribute.NONE)
				return ParseDeclareFieldMethodStatement(attr, null);

			int index = mIndex;
			PushError();
			var decl = ParseDeclareFieldMethodStatement(attr, null);
			if (decl != null)
			{
				AppendErrors(PopError());
				return decl;
			}
			PopError();

			mIndex = index;
			var expr = ParseExpression();
			if (expr == null)
				return null;
			Expect(Token.SEMICOLON);
			return new ExpressionStatement(expr);
		}
		private Statement ParseDeclareExpressionStatement()
		{
			var attr = ParseCommonAttribute(false);
			Expression type = null;
			if (attr != CommonAttribute.NONE)
			{
				type = ParseTypeExpression(true, false);
				if (type == null)
					return null;

				return ParseDeclareFieldValueStatement(attr, type);
			}

			int index = mIndex;
			PushError();
			type = ParseTypeExpression(true, false);
			if (type != null)
			{
				var decl = ParseDeclareFieldValueStatement(attr, type);
				if (decl != null)
				{
					AppendErrors(PopError());
					return decl;
				}
			}
			PopError();

			mIndex = index;
			var expr = ParseExpression();
			if (expr == null)
				return null;
			return new ExpressionStatement(expr);
		}
		private IfStatement ParseIfStatement()
		{
			++mIndex;
			if (!Expect(Token.OPEN_PARENS))
				return null;
			IfStatement result = new IfStatement(ParseExpression());
			if (Expect(Token.CLOSE_PARENS) || mTokens[mIndex].token == Token.OPEN_BRACE)
				result.bodyt = ParseStatement();

			if (mTokens[mIndex].token == Token.ELSE)
			{
				++mIndex;
				result.bodyf = ParseStatement();
			}
			return result;
		}
		private Statement ParseForStatement()
		{
			++mIndex;
			if (!Expect(Token.OPEN_PARENS))
				return null;

			Statement init = null;
			if (mTokens[mIndex].token != Token.SEMICOLON)
				init = ParseDeclareExpressionStatement();
			if (init != null)
			{
				if (mTokens[mIndex].token == Token.COLON)
				{
					++mIndex;
					ForeachStatement r = new ForeachStatement(init, ParseExpression());
					if (Expect(Token.CLOSE_PARENS) || mTokens[mIndex].token == Token.OPEN_BRACE)
						r.body = ParseStatement();
					return r;
				}
			}
			Expect(Token.SEMICOLON);
			Expression cond = null;
			if (mTokens[mIndex].token != Token.SEMICOLON)
			{
				cond = ParseExpression();
			}
			Expect(Token.SEMICOLON);
			Expression iter = null;
			if (mTokens[mIndex].token != Token.CLOSE_PARENS)
				iter = ParseExpression();

			ForStatement result = new ForStatement(init, cond, iter);
			if (Expect(Token.CLOSE_PARENS) || mTokens[mIndex].token == Token.OPEN_BRACE)
				result.body = ParseStatement();

			return result;
		}
		private Statement ParseForeachStatement()
		{
			++mIndex;
			if (!Expect(Token.OPEN_PARENS))
				return null;

			Statement iter = ParseDeclareExpressionStatement();
			Expect(Token.COLON);
			var val = ParseExpression();
			ForeachStatement r = new ForeachStatement(iter, val);
			if (Expect(Token.CLOSE_PARENS) || mTokens[mIndex].token == Token.OPEN_BRACE)
				r.body = ParseStatement();
			return r;
		}
		private Statement ParseWhileStatement()
		{
			++mIndex;
			if (!Expect(Token.OPEN_PARENS))
				return null;
			WhileStatement result = new WhileStatement(ParseExpression());
			if (Expect(Token.CLOSE_PARENS) || mTokens[mIndex].token == Token.OPEN_BRACE)
				result.body = ParseStatement();

			return result;
		}
		private Statement ParseDoWhileStatement()
		{
			++mIndex;
			DoStatement result = new DoStatement();
			result.body = ParseStatement();

			if (!Expect(Token.WHILE) || !Expect(Token.OPEN_PARENS))
				return result;
			result.cond = ParseExpression();
			Expect(Token.CLOSE_PARENS);

			return result;
		}
		private Statement ParseSwitchStatement()
		{
			++mIndex;
			if (!Expect(Token.OPEN_PARENS))
				return null;
			SwitchStatement result = new SwitchStatement(ParseExpression());
			if (Expect(Token.CLOSE_PARENS) || mTokens[mIndex].token == Token.OPEN_BRACE)
				result.body = ParseBlockStatement();

			return result;
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
			val = ParseTypeExpression(false, false);
			if (val == null)
				return null;

			Expect(Token.SEMICOLON);
			return new UsingStatement(name, val);
		}

		private Expression ParseTypeExpression(bool accept_array, bool pure_generic)
		{
			var k = mTokens[mIndex];
			switch (k.token)
			{
				case Token.VAR:
				case Token.VOID:
				case Token.BOOL:
				case Token.CHAR:
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
										if (pure_generic && mTokens[mIndex].token == Token.COMMA)
											arg = null;
										else
											arg = ParseTypeExpression(accept_array, false);
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
								if (mTokens[mIndex + 1].token != Token.CLOSE_BRACKET)
									return expr;
								mIndex += 2;
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
			if (expr != null)
			{
				switch (mTokens[mIndex].token)
				{
					case Token.ASSIGN:
						++mIndex;
						expr = new AssignExpression(expr, ParseExpression());
						break;
					case Token.OP_MULT_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(BinaryExpression.Operator.Multiply, expr, ParseExpression());
						break;
					case Token.OP_DIV_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(BinaryExpression.Operator.Division, expr, ParseExpression());
						break;
					case Token.OP_MOD_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(BinaryExpression.Operator.Modulus, expr, ParseExpression());
						break;
					case Token.OP_ADD_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(BinaryExpression.Operator.Addition, expr, ParseExpression());
						break;
					case Token.OP_SUB_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(BinaryExpression.Operator.Subtraction, expr, ParseExpression());
						break;
					case Token.OP_SHIFT_LEFT_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(BinaryExpression.Operator.LeftShift, expr, ParseExpression());
						break;
					case Token.OP_SHIFT_RIGHT_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(BinaryExpression.Operator.RightShift, expr, ParseExpression());
						break;
					case Token.OP_AND_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(BinaryExpression.Operator.BitwiseAnd, expr, ParseExpression());
						break;
					case Token.OP_XOR_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(BinaryExpression.Operator.ExclusiveOr, expr, ParseExpression());
						break;
					case Token.OP_OR_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(BinaryExpression.Operator.BitwiseOr, expr, ParseExpression());
						break;
				}
			}
			return expr;
		}

		private Expression ParseConstantExpression()
		{
			var expr = ParseLogicalOrExpression();
			if (expr != null)
			{
				if (mTokens[mIndex].token == Token.INTERR)
				{
					var e1 = ParseExpression();
					Expression e2 = null;
					if (Expect(Token.COLON))
						e2 = ParseConstantExpression();

					expr = new ConditionalExpression(expr, e1, e2);
				}
			}
			return expr;
		}

		private Expression ParseValueExpression()
		{
			if (mTokens[mIndex].token == Token.OPEN_BRACE)
			{
				++mIndex;
				ValueListExpression expr = new ValueListExpression();
				while (mTokens[mIndex].token != Token.CLOSE_BRACE)
				{
					expr.Add(ParseConstantExpression());
					if (mTokens[mIndex].token != Token.COMMA)
						break;
					++mIndex;
				}
				Expect(Token.CLOSE_BRACE);
				return expr;
			}
			if (mTokens[mIndex].token == Token.DEFAULT)
			{
				++mIndex;
				return new DefaultExpression();
			}
			return ParseExpression();
		}

		private Expression ParseLogicalOrExpression()
		{
			var expr = ParseLogicalAndExpression();
			if (expr != null)
			{
				while (mTokens[mIndex].token == Token.OP_OR)
				{
					++mIndex;
					expr = new BinaryExpression(BinaryExpression.Operator.LogicalOr, expr, ParseLogicalAndExpression());
				}
			}
			return expr;
		}

		private Expression ParseLogicalAndExpression()
		{
			var expr = ParseOrExpression();
			if (expr != null)
			{
				while (mTokens[mIndex].token == Token.OP_AND)
				{
					++mIndex;
					expr = new BinaryExpression(BinaryExpression.Operator.LogicalAnd, expr, ParseOrExpression());
				}
			}
			return expr;
		}

		private Expression ParseOrExpression()
		{
			var expr = ParseXorExpression();
			if (expr != null)
			{
				while (mTokens[mIndex].token == Token.BITWISE_OR)
				{
					++mIndex;
					expr = new BinaryExpression(BinaryExpression.Operator.BitwiseOr, expr, ParseXorExpression());
				}
			}
			return expr;
		}
		private Expression ParseXorExpression()
		{
			var expr = ParseAndExpression();
			if (expr != null)
			{
				while (mTokens[mIndex].token == Token.XOR)
				{
					++mIndex;
					expr = new BinaryExpression(BinaryExpression.Operator.ExclusiveOr, expr, ParseAndExpression());
				}
			}
			return expr;
		}
		private Expression ParseAndExpression()
		{
			var expr = ParseEqualityExpression();
			if (expr != null)
			{
				while (mTokens[mIndex].token == Token.BITWISE_AND)
				{
					++mIndex;
					expr = new BinaryExpression(BinaryExpression.Operator.BitwiseAnd, expr, ParseEqualityExpression());
				}
			}
			return expr;
		}
		private Expression ParseEqualityExpression()
		{
			var expr = ParseRelationalExpression();
			if (expr != null)
			{
				_loop:
				switch (mTokens[mIndex].token)
				{
					case Token.OP_EQ:
						++mIndex;
						expr = new BinaryExpression(BinaryExpression.Operator.Equality, expr, ParseRelationalExpression());
						goto _loop;
					case Token.OP_NE:
						++mIndex;
						expr = new BinaryExpression(BinaryExpression.Operator.Inequality, expr, ParseRelationalExpression());
						goto _loop;
				}
			}
			return expr;
		}
		private Expression ParseRelationalExpression()
		{
			var expr = ParseShiftExpression();
			if (expr != null)
			{
				_loop:
				switch (mTokens[mIndex].token)
				{
					case Token.OP_LT:
						++mIndex;
						expr = new BinaryExpression(BinaryExpression.Operator.LessThan, expr, ParseShiftExpression());
						goto _loop;
					case Token.OP_GT:
						++mIndex;
						expr = new BinaryExpression(BinaryExpression.Operator.GreaterThan, expr, ParseShiftExpression());
						goto _loop;
					case Token.OP_LE:
						++mIndex;
						expr = new BinaryExpression(BinaryExpression.Operator.LessThanOrEqual, expr, ParseShiftExpression());
						goto _loop;
					case Token.OP_GE:
						++mIndex;
						expr = new BinaryExpression(BinaryExpression.Operator.GreaterThanOrEqual, expr, ParseShiftExpression());
						goto _loop;
				}
			}
			return expr;
		}
		private Expression ParseShiftExpression()
		{
			var expr = ParseAdditiveExpression();
			if (expr != null)
			{
				_loop:
				switch (mTokens[mIndex].token)
				{
					case Token.OP_SHIFT_LEFT:
						++mIndex;
						expr = new BinaryExpression(BinaryExpression.Operator.LeftShift, expr, ParseAdditiveExpression());
						goto _loop;
					case Token.OP_SHIFT_RIGHT:
						++mIndex;
						expr = new BinaryExpression(BinaryExpression.Operator.RightShift, expr, ParseAdditiveExpression());
						goto _loop;
				}
			}
			return expr;
		}
		private Expression ParseAdditiveExpression()
		{
			var expr = ParseMultiplicativeExpression();
			if (expr != null)
			{
				_loop:
				switch (mTokens[mIndex].token)
				{
					case Token.PLUS:
						++mIndex;
						expr = new BinaryExpression(BinaryExpression.Operator.Addition, expr, ParseMultiplicativeExpression());
						goto _loop;
					case Token.MINUS:
						++mIndex;
						expr = new BinaryExpression(BinaryExpression.Operator.Subtraction, expr, ParseMultiplicativeExpression());
						goto _loop;
				}
			}
			return expr;
		}
		private Expression ParseMultiplicativeExpression()
		{
			var expr = ParseCastExpression();
			if (expr != null)
			{
				_loop:
				switch (mTokens[mIndex].token)
				{
					case Token.STAR:
						++mIndex;
						expr = new BinaryExpression(BinaryExpression.Operator.Multiply, expr, ParseCastExpression());
						goto _loop;
					case Token.DIV:
						++mIndex;
						expr = new BinaryExpression(BinaryExpression.Operator.Division, expr, ParseCastExpression());
						goto _loop;
					case Token.PERCENT:
						++mIndex;
						expr = new BinaryExpression(BinaryExpression.Operator.Modulus, expr, ParseCastExpression());
						goto _loop;
				}
			}
			return expr;
		}
		private Expression ParseCastExpression()
		{
			if (mTokens[mIndex].token == Token.OPEN_PARENS)
			{
				var index = mIndex;
				PushError();
				++mIndex;
				var type = ParseTypeExpression(true, false);
				if (type != null && mTokens[mIndex].token == Token.CLOSE_PARENS)
				{
					Expression val = ParseCastExpression();
					if (val != null)
					{
						AppendErrors(PopError());
						return new TypeCastExpression(type, val);
					}
				}
				PopError();
				mIndex = index;
			}
			return ParseUnaryExpression();
		}
		private Expression ParseUnaryExpression()
		{
			switch (mTokens[mIndex].token)
			{
				case Token.OP_INC:
					++mIndex;
					return new UnaryMutator(UnaryMutator.Mode.PreIncrement, ParseUnaryExpression());
				case Token.OP_DEC:
					++mIndex;
					return new UnaryMutator(UnaryMutator.Mode.PreDecrement, ParseUnaryExpression());
				case Token.MINUS:
					++mIndex;
					return new NegExpression(ParseCastExpression());
				case Token.TILDE:
					++mIndex;
					return new NotExpression(ParseCastExpression());
				case Token.BANG:
					++mIndex;
					return new LogicNotExpression(ParseCastExpression());
			}
			return ParsePostfixExpression();
		}
		private Expression ParsePostfixExpression()
		{
			var expr = ParsePrimaryExpression();
			if (expr != null)
			{
				ValueListExpression va;
				_loop:
				switch (mTokens[mIndex].token)
				{
					case Token.OPEN_BRACKET:
						++mIndex;
						va = new ValueListExpression();
						do
						{
							va.Add(ParseExpression());
							if (mTokens[mIndex].token != Token.COMMA)
								break;
							++mIndex;
						} while (mTokens[mIndex].token != Token.CLOSE_BRACKET);
						Expect(Token.CLOSE_BRACKET);
						expr = new IndexExpression(expr, va);
						goto _loop;
					case Token.OPEN_PARENS:
						++mIndex;
						va = new ValueListExpression();
						while (mTokens[mIndex].token != Token.CLOSE_BRACKET)
						{
							va.Add(ParseExpression());
							if (mTokens[mIndex].token != Token.COMMA)
								break;
							++mIndex;
						}
						Expect(Token.CLOSE_BRACKET);
						expr = new InvocationExpression(expr, va);
						goto _loop;
					case Token.DOT:
						++mIndex;
						var name = mTokens[mIndex];
						if (Expect(Token.IDENTIFIER))
						{
							expr = new AccessExpression(expr, name);
							goto _loop;
						}
						break;
					case Token.OP_INC:
						++mIndex;
						expr = new UnaryMutator(UnaryMutator.Mode.PostIncrement, expr);
						goto _loop;
					case Token.OP_DEC:
						++mIndex;
						expr = new UnaryMutator(UnaryMutator.Mode.PostDecrement, expr);
						goto _loop;
				}
			}
			return expr;
		}
		private Expression ParsePrimaryExpression()
		{
			Expression type;
			switch (mTokens[mIndex].token)
			{
				case Token.IDENTIFIER:
					return ParseTypeExpression(false, false);
				case Token.LITERAL:
				case Token.TRUE:
				case Token.FALSE:
				case Token.NULL:
				case Token.THIS:
				case Token.BASE:
					return new LiteralExpression(mTokens[mIndex++]);
				case Token.OPEN_PARENS:
					++mIndex;
					var expr = ParseExpression();
					Expect(Token.CLOSE_PARENS);
					return expr;
				case Token.NEW:
					++mIndex;
					type = ParseTypeExpression(true, false);
					if (type == null)
						break;
					bool is_array = false;
					ValueListExpression args = null;
					ValueListExpression value = null;
					if (mTokens[mIndex].token == Token.OPEN_PARENS)
					{
						++mIndex;
						args = new ValueListExpression();
						while (mTokens[mIndex].token != Token.CLOSE_PARENS)
						{
							args.Add(ParseExpression());
							if (mTokens[mIndex].token != Token.COMMA)
								break;
							++mIndex;
						}
						Expect(Token.CLOSE_PARENS);
					}
					else if (mTokens[mIndex].token == Token.OPEN_BRACKET)
					{
						is_array = true;
						++mIndex;
						args = new ValueListExpression();
						do
						{
							args.Add(ParseExpression());
							if (mTokens[mIndex].token != Token.COMMA)
								break;
							++mIndex;
						} while (mTokens[mIndex].token != Token.CLOSE_BRACKET);
						Expect(Token.CLOSE_BRACKET);
					}
					if (mTokens[mIndex].token == Token.OPEN_BRACE)
					{
						++mIndex;
						value = new ValueListExpression();
						while (mTokens[mIndex].token != Token.CLOSE_BRACE)
						{
							value.Add(ParseExpression());
							if (mTokens[mIndex].token != Token.COMMA)
								break;
							++mIndex;
						}
						Expect(Token.CLOSE_BRACE);
					}
					return new NewExpression(type, args, value, is_array);
				case Token.TYPEOF:
					++mIndex;
					if (Expect(Token.OPEN_PARENS))
					{
						int index = mIndex;
						PushError();
						type = ParseTypeExpression(true, false);
						if (type == null || mTokens[mIndex].token != Token.CLOSE_PARENS)
						{
							PopError();
							mIndex = index;
							type = ParseExpression();
						}
						else
							AppendErrors(PopError());
						Expect(Token.CLOSE_PARENS);
						return new TypeofExpression(type);
					}
					return null;
			}

			Expect(Token.EXPRESSION);
			return null;
		}
		private CommonAttribute ParseCommonAttribute(bool acc_new)
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
					case Token.NEW:
						if (!acc_new)
							return attr;
						add = CommonAttribute.NEW;
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
				int next = mIndex + 1;
				var s = ParseDeclareStatement(result);
				if (s != null)
					result.members.Add(s);
				else if (mIndex < next)
				{
					NotExpect();
					mIndex = next;
				}
			}

			Expect(Token.CLOSE_BRACE);
			return result;
		}

		private TypeStatement ParseTypeStatement(CommonAttribute attr)
		{
			var cls_or_strct = mTokens[mIndex].token;
			++mIndex;
			var name = mTokens[mIndex];
			if (!Expect(Token.IDENTIFIER))
				return null;

			TypeStatement result = new TypeStatement(attr, cls_or_strct, new NameExpression(name));
			if (mTokens[mIndex].token == Token.GENERIC_LT)
			{
				result.generics = new List<TokenValue>();
				do
				{
					++mIndex;
					name = mTokens[mIndex];
					if (Expect(Token.IDENTIFIER))
						result.generics.Add(name);
				} while (mTokens[mIndex].token == Token.COMMA);

				Expect(Token.GENERIC_GT);
			}

			if (mTokens[mIndex].token == Token.COLON)
			{
				result.base_type = new List<Expression>();
				do
				{
					++mIndex;
					var bs = ParseTypeExpression(true, false);
					result.base_type.Add(bs);
				} while (mTokens[mIndex].token == Token.COMMA);
			}

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
				int next = mIndex + 1;
				var s = ParseDeclareStatement(result);
				if (s != null)
					result.members.Add(s);
				else if (mIndex < next)
				{
					NotExpect();
					mIndex = next;
				}
			}

			Expect(Token.CLOSE_BRACE);
			return result;
		}
	}
}
