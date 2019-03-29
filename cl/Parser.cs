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
		public Stack<List<AbstractMessage>> mErrorStack = new Stack<List<AbstractMessage>>();
		private List<AbstractMessage> mErrors;
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
				Error(mTokens[mIndex].loc, "期望 " + TokenValue.GetName(token));
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
			mErrors.Add(new ErrorMessage(1, Location.Null, msg, null));
		}
		public void Error(Location pos, string msg)
		{
			mErrors.Add(new ErrorMessage(1, pos, msg, null));
		}
		public void Waring(string msg)
		{
			mErrors.Add(new WarningMessage(1, Location.Null, msg, null));
		}
		public void Waring(Location pos, string msg)
		{
			mErrors.Add(new WarningMessage(1, pos, msg, null));
		}
		public void FlushError()
		{
			List<AbstractMessage> errs = null;
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
				mCompiler.Context.Message(e);
			}
		}
		private void PushError()
		{
			mErrorStack.Push(mErrors = new List<AbstractMessage>());
		}
		private List<AbstractMessage> PopError()
		{
			var r = mErrorStack.Pop();
			mErrors = mErrorStack.Peek();
			return r;
		}
		private void AppendErrors(List<AbstractMessage> errs)
		{
			mErrors.AddRange(errs);
		}
		public void ParseFile(string pathname)
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
				return;
			}

			mIndex = 1;
			while (mTokens[mIndex].token != Token.EOF)
			{
				int next = mIndex + 1;
				ParseDeclareStatement(mCompiler.AstRoot);
				if (mIndex < next)
				{
					NotExpect();
					mIndex = next;
				}
			}
			FlushError();
		}

		private void ParseDeclareStatement(AstContainer parent)
		{
			Ast s = null;
			int start = mIndex;
			switch (mTokens[mIndex].token)
			{
				case Token.SEMICOLON:
					++mIndex;
					return;
				case Token.USING:
					s = ParseUsingStatement(parent);
					break;
				case Token.NAMESPACE:
					if (!(parent is AstNamespace))
					{
						NotExpect();
						++mIndex;
						return;
					}
					s = ParseNamespaceStatement((AstNamespace)parent);
					break;
				default:
					var attr = ParseCommonAttribute(true);
					switch (mTokens[mIndex].token)
					{
						case Token.CLASS:
							s = ParseTypeStatement(attr, parent, AstKind.Class);
							break;
						case Token.STRUCT:
							s = ParseTypeStatement(attr, parent, AstKind.Struct);
							break;
						case Token.ENUM:
							s = ParseTypeStatement(attr, parent, AstKind.Enum);
							break;
						case Token.INTERFACE:
							s = ParseTypeStatement(attr, parent, AstKind.Interface);
							break;
						default:
							s = ParseDeclareFieldMethodStatement(attr, parent);
							break;
					}
					break;
			}

			if (s != null)
				parent.Add(s);
		}
		private void ParsePropertyBody(AstProperty prop)
		{
			throw new Exception();
			//if (!Expect(Token.OPEN_BRACE))
			//	return;
			//do
			//{
			//	var attr = ParseCommonAttribute(false);
			//	var name = mTokens[mIndex];
			//	if (!Expect(Token.IDENTIFIER))
			//		break;

			//	var method = new Method(prop.Declaring, prop.type_expr, new MemberName(name.value, name.loc));
			//	if (mTokens[mIndex].token == Token.SEMICOLON)
			//		++mIndex;
			//	else
			//		method.block = ParseToplevelBlock();
			//	prop.methods.Add(method);
			//} while (mTokens[mIndex].token != Token.CLOSE_BRACE);
			//Expect(Token.CLOSE_BRACE);
		}
		private AstName ParseMemberName()
		{
			var k = mTokens[mIndex];
			if (k.token != Token.IDENTIFIER)
			{
				Error(mTokens[mIndex].loc, "期望一个标识符");
				return null;
			}
			AstName expr = new AstName(k.value, k.loc);
			bool accept_generic = true;
			++mIndex;
			while (true)
			{
				switch (mTokens[mIndex].token)
				{
					case Token.DOT:
						++mIndex;
						var right = ParseMemberName();
						if (right == null)
							return expr;
						right.Left = expr;
						return right;
					case Token.GENERIC_LT:
						if (!accept_generic)
							return expr;
						++mIndex;
						if (mTokens[mIndex].token != Token.GENERIC_GT)
						{
							while (true)
							{
								var v = mTokens[mIndex];
								if (Expect(Token.IDENTIFIER))
									expr.AddTypeParameter(new AstName(v.value, v.loc));

								if (mTokens[mIndex].token != Token.GENERIC_GT)
									Expect(Token.COMMA);
								else
									break;
							}
						}
						Skip(Token.GENERIC_GT);
						accept_generic = false;
						break;
					default:
						return expr;
				}
			}
		}
		private Ast ParseDeclareFieldMethodStatement(CommonAttribute attr, AstContainer parent)
		{
			AstMethod method = null;
			AstTypeDefine cls = parent as AstTypeDefine;
			FullNamedExpression type = null;
			// 构造函数
			if (mTokens[mIndex].token == Token.IDENTIFIER && mTokens[mIndex + 1].token == Token.OPEN_PARENS)
			{
				if (cls != null && cls.Name.Name == mTokens[mIndex].value)
				{
					method = new AstMethod(cls, null, cls.Name, mTokens[mIndex].loc);
					mIndex += 2;
					method.Parameters = ParseParameters(Token.CLOSE_PARENS);
					if (Expect(Token.CLOSE_PARENS) || mTokens[mIndex].token == Token.OPEN_PARENS)
						method.TopBlock = ParseToplevelBlock();
					return method;
				}

				Error(mTokens[mIndex].loc, "函数丢失返回类型");
			}
			else
			{
				type = ParseTypeExpression();
			}

			if (type == null)
				type = new TypeExpr(BuildinTypeSpec.Void, Location.Null);

			var loc = mTokens[mIndex].loc;
			AstName name = ParseMemberName();
			if (name == null)
				return null;

			if (mTokens[mIndex].token == Token.OPEN_PARENS)
			{
				++mIndex;
				method = new AstMethod(parent, type, name, loc);
				method.Parameters = ParseParameters(Token.CLOSE_PARENS);
				if (Expect(Token.CLOSE_PARENS) || mTokens[mIndex].token == Token.OPEN_PARENS)
					method.TopBlock = ParseToplevelBlock();
				return method;
			}
			if (mTokens[mIndex].token == Token.OPEN_BRACE)
			{
				AstProperty prop = new AstProperty(parent, type, name, loc);
				ParsePropertyBody(prop);
				return prop;
			}

			if (name.Left != null)
			{
				NotExpect();
				return null;
			}

			--mIndex;
			ParseDeclareFieldValueStatement(parent, attr, type);
			Expect(Token.SEMICOLON);
			return null;
		}
		private void ParseDeclareFieldStatement(AstContainer parent)
		{
			CommonAttribute attr = ParseCommonAttribute(false);
			var type = ParseTypeExpression();
			if (type == null)
				return;

			ParseDeclareFieldValueStatement(parent, attr, type);
			Expect(Token.SEMICOLON);
		}
		private void ParseDeclareFieldValueStatement(AstContainer parent, CommonAttribute attr, FullNamedExpression type)
		{
			AstField field = null;
			do
			{
				if (field != null)
					++mIndex;

				var name = mTokens[mIndex];
				if (!Expect(Token.IDENTIFIER))
					return;
				field = new AstField(parent, type, new AstName(name.value, name.loc), name.loc);
				if (mTokens[mIndex].token == Token.ASSIGN)
				{
					++mIndex;
					field.Value = ParseValueExpression();
				}
				parent.Add(field);
			}
			while (mTokens[mIndex].token == Token.COMMA);
		}
		private List<AstParameter> ParseParameters(Token term)
		{
			List<AstParameter> ps = new List<AstParameter>();
			if (mTokens[mIndex].token == term)
				return ps;
			while (true)
			{
				var attr = ParseParamAttribute(true);
				var type = ParseTypeExpression();
				if (type == null)
					break;
				TokenValue name = null;
				var loc = mTokens[mIndex].loc;
				if (mTokens[mIndex].token == Token.IDENTIFIER)
					name = mTokens[mIndex++];

				AstParameter p = new AstParameter(type, name == null ? null : name.value, loc);
				ps.Add(p);
				if (mTokens[mIndex].token != Token.COMMA)
					break;
				++mIndex;
			}
			return ps;
		}
		private AstBlock ParseToplevelBlock()
		{
			return ParseBlockStatement(null);
		}
		private AstBlock ParseBlockStatement(AstBlock parent)
		{
			var loc = mTokens[mIndex].loc;
			if (!Expect(Token.OPEN_BRACE))
				return null;

			AstBlock result = new AstBlock(parent, loc);
			while (mTokens[mIndex].token != Token.CLOSE_BRACE && mTokens[mIndex].token != Token.EOF)
			{
				int next = mIndex + 1;
				var s = ParseStatement(result);
				if (s != null)
					result.Add(s);
				else if (mIndex < next)
				{
					NotExpect();
					mIndex = next;
				}
			}
			Expect(Token.CLOSE_BRACE);
			return result;
		}
		private Ast ParseStatement(AstBlock parent)
		{
			Ast result = null;
			Location loc;
			switch (mTokens[mIndex].token)
			{
				case Token.SEMICOLON:
					loc = mTokens[mIndex].loc;
					++mIndex;
					return new AstEmpty(loc);
				case Token.OPEN_BRACE:
					return ParseBlockStatement(parent);
				case Token.IF:
					return ParseIfStatement(parent);
				case Token.FOR:
					throw new Exception();
					//return ParseForStatement(parent);
				case Token.FOREACH:
					throw new Exception();
					//return ParseForeachStatement();
				case Token.WHILE:
					throw new Exception();
					//return ParseWhileStatement(parent);
				case Token.DO:
					throw new Exception();
					//return ParseDoWhileStatement(parent);
				case Token.SWITCH:
					throw new Exception();
					//return ParseSwitchStatement();
				case Token.CASE:
					throw new Exception();
					//loc = mTokens[mIndex].loc;
					//++mIndex;
					//result = new SwitchLabel(ParseConstantExpression(), loc);
					//Expect(Token.COLON);
					//return result;
				case Token.RETURN:
					loc = mTokens[mIndex].loc;
					++mIndex;
					result = new AstReturn(parent, mTokens[mIndex].token == Token.SEMICOLON ? null : ParseExpression(), loc);
					Expect(Token.SEMICOLON);
					return result;
				case Token.BREAK:
					throw new Exception();
					//loc = mTokens[mIndex].loc;
					//++mIndex;
					//Expect(Token.SEMICOLON);
					//return new Break(loc);
				case Token.CONTINUE:
					throw new Exception();
					//loc = mTokens[mIndex].loc;
					//++mIndex;
					//Expect(Token.SEMICOLON);
					//return new Continue(loc);
				case Token.IDENTIFIER:
					if (mTokens[mIndex + 1].token == Token.COLON)
					{
						throw new Exception();
						//result = new LabeledStatement(mTokens[mIndex].value, parent, mTokens[mIndex].loc);
						//mIndex += 2;
						//return result;
					}
					break;
				case Token.GOTO:
					throw new Exception();
					//loc = mTokens[mIndex].loc;
					//++mIndex;
					//if (Expect(Token.IDENTIFIER))
					//	result = new Goto(mTokens[mIndex - 1].value, loc);
					//Expect(Token.SEMICOLON);
					//return result;
			}

			return ParseDeclareExpressionStatement(parent);
		}
		private Ast ParseDeclareExpressionStatement(AstBlock parent)
		{
			int index = mIndex;
			PushError();

			var attr = ParseCommonAttribute(false);
			var type = ParseTypeExpression();
			if (type != null)
			{
				if (mTokens[mIndex].token == Token.IDENTIFIER)
				{
					do
					{
						AstDeclare lv = new AstDeclare(parent, type, mTokens[mIndex].value, mTokens[mIndex].loc);
						++mIndex;

						if (mTokens[mIndex].token != Token.ASSIGN)
						{
							++mIndex;
							lv.Value = ParseValueExpression();
						}

						parent.Add(lv);
						if (mTokens[mIndex].token != Token.COMMA)
							break;
						++mIndex;
					} while (true);

					AppendErrors(PopError());
					return null;
				}
			}
			PopError();

			mIndex = index;
			var expr = ParseExpression();
			if (expr == null)
				return null;
			Expect(Token.SEMICOLON);
			return new AstExpression(expr);
		}
		private AstIf ParseIfStatement(AstBlock parent)
		{
			var loc = mTokens[mIndex].loc;
			++mIndex;
			if (!Expect(Token.OPEN_PARENS))
				return null;

			var bool_expr = ParseExpression();
			Ast true_block = null;
			Ast false_block = null;
			if (Expect(Token.CLOSE_PARENS) || mTokens[mIndex].token == Token.OPEN_BRACE)
				true_block = ParseStatement(parent);

			if (mTokens[mIndex].token == Token.ELSE)
			{
				++mIndex;
				false_block = ParseStatement(parent);
			}
			return new AstIf(parent, bool_expr, true_block, false_block, loc);
		}
		/*
		private Statement ParseForStatement(Block parent)
		{
			var loc = mTokens[mIndex].loc;
			++mIndex;
			if (!Expect(Token.OPEN_PARENS))
				return null;

			Statement init = null;
			if (mTokens[mIndex].token != Token.SEMICOLON)
				init = ParseDeclareExpressionStatement(parent);
			if (init != null)
			{
				//if (mTokens[mIndex].token == Token.COLON)
				//{
				//	++mIndex;
				//	Foreach r = new Foreach(init, ParseExpression());
				//	if (Expect(Token.CLOSE_PARENS) || mTokens[mIndex].token == Token.OPEN_BRACE)
				//		r.body = ParseStatement();
				//	return r;
				//}
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

			For result = new For(init, cond, iter, loc);
			if (Expect(Token.CLOSE_PARENS) || mTokens[mIndex].token == Token.OPEN_BRACE)
				result.Statement = ParseStatement(parent);

			return result;
		}
		private Statement ParseForeachStatement()
		{
			throw new Exception();
			//++mIndex;
			//if (!Expect(Token.OPEN_PARENS))
			//	return null;

			//Statement iter = ParseDeclareExpressionStatement();
			//Expect(Token.COLON);
			//var val = ParseExpression();
			//Foreach r = new Foreach(iter, val);
			//if (Expect(Token.CLOSE_PARENS) || mTokens[mIndex].token == Token.OPEN_BRACE)
			//	r.body = ParseStatement();
			//return r;
		}
		private Statement ParseWhileStatement(Block parent)
		{
			var loc = mTokens[mIndex].loc;
			++mIndex;
			if (!Expect(Token.OPEN_PARENS))
				return null;

			var cond = ParseExpression();
			Statement body = null;
			if (Expect(Token.CLOSE_PARENS) || mTokens[mIndex].token == Token.OPEN_BRACE)
				body = ParseStatement(parent);

			return new While(cond, body, loc);
		}
		private Statement ParseDoWhileStatement(Block parent)
		{
			var loc = mTokens[mIndex].loc;
			++mIndex;
			var body = ParseStatement(parent);

			var loc2 = mTokens[mIndex].loc;
			if (!Expect(Token.WHILE) || !Expect(Token.OPEN_PARENS))
				return body;
			var cond = ParseExpression();
			Expect(Token.CLOSE_PARENS);

			return new Do(body, cond, loc, loc2);
		}
		private Statement ParseSwitchStatement()
		{
			throw new Exception();
			//++mIndex;
			//if (!Expect(Token.OPEN_PARENS))
			//	return null;
			//Switch result = new Switch(ParseExpression());
			//if (Expect(Token.CLOSE_PARENS) || mTokens[mIndex].token == Token.OPEN_BRACE)
			//	result.block = ParseBlockStatement();

			//return result;
		}
		*/
		private AstUsing ParseUsingStatement(Ast parent)
		{
			string alias = null;
			FullNamedExpression expr = null;

			var loc = mTokens[mIndex].loc;
			++mIndex;

			if (mTokens[mIndex].token == Token.IDENTIFIER && mTokens[mIndex + 1].token == Token.ASSIGN)
			{
				alias = mTokens[mIndex].value;
				mIndex += 2;
			}

			var loc2 = mTokens[mIndex].loc;
			expr = ParseTypeExpression();
			if (expr == null)
				return null;

			Expect(Token.SEMICOLON);
			return new AstUsing(parent, alias, expr as SimpleName, loc);
		}

		private FullNamedExpression ParseTypeExpression()
		{
			var k = mTokens[mIndex];
			switch (k.token)
			{
				case Token.VAR:
					++mIndex;
					return new VarExpr(k.loc);
				case Token.VOID:
					++mIndex;
					return new TypeExpr(BuildinTypeSpec.Void, k.loc);
				case Token.BOOL:
					++mIndex;
					return new TypeExpr(BuildinTypeSpec.Bool, k.loc);
				case Token.CHAR:
					++mIndex;
					return new TypeExpr(BuildinTypeSpec.Char, k.loc);
				case Token.SBYTE:
					++mIndex;
					return new TypeExpr(BuildinTypeSpec.SByte, k.loc);
				case Token.BYTE:
					++mIndex;
					return new TypeExpr(BuildinTypeSpec.Byte, k.loc);
				case Token.SHORT:
					++mIndex;
					return new TypeExpr(BuildinTypeSpec.Short, k.loc);
				case Token.USHORT:
					++mIndex;
					return new TypeExpr(BuildinTypeSpec.UShort, k.loc);
				case Token.INT:
					++mIndex;
					return new TypeExpr(BuildinTypeSpec.Int, k.loc);
				case Token.UINT:
					++mIndex;
					return new TypeExpr(BuildinTypeSpec.UInt, k.loc);
				case Token.LONG:
					++mIndex;
					return new TypeExpr(BuildinTypeSpec.Long, k.loc);
				case Token.ULONG:
					++mIndex;
					return new TypeExpr(BuildinTypeSpec.ULong, k.loc);
				case Token.FLOAT:
					++mIndex;
					return new TypeExpr(BuildinTypeSpec.Float, k.loc);
				case Token.DOUBLE:
					++mIndex;
					return new TypeExpr(BuildinTypeSpec.Double, k.loc);
				case Token.STRING:
					++mIndex;
					return new TypeExpr(BuildinTypeSpec.String, k.loc);
				case Token.OBJECT:
					++mIndex;
					return new TypeExpr(BuildinTypeSpec.Object, k.loc);
				case Token.IDENTIFIER:
					FullNamedExpression expr = new SimpleName(k.value, k.loc);
					bool accept_generic = true;
					++mIndex;
					while (true)
					{
						switch (mTokens[mIndex].token)
						{
							case Token.DOT:
								++mIndex;
								var right = ParseTypeExpression();
								if (right == null)
									return expr;
								right.Left = expr;
								return right;
							case Token.GENERIC_LT:
								if (!accept_generic)
									return expr;
								++mIndex;
								var args = expr.GetTypeArguments();
								if (mTokens[mIndex].token != Token.GENERIC_GT)
								{
									while (true)
									{
										FullNamedExpression arg;
										if (mTokens[mIndex].token == Token.COMMA)
											arg = null;
										else
											arg = ParseTypeExpression();
										args.Add(arg);

										if (mTokens[mIndex].token != Token.GENERIC_GT)
											Expect(Token.COMMA);
										else
											break;
									}
								}
								Skip(Token.GENERIC_GT);
								accept_generic = false;
								break;
							case Token.OPEN_BRACKET:
								accept_generic = false;
								if (mTokens[mIndex + 1].token != Token.CLOSE_BRACKET)
									return expr;
								mIndex += 2;
								expr = new ArrayName(expr);
								break;
							case Token.STAR:
								accept_generic = false;
								++mIndex;
								expr = new PointerName(expr);
								break;
							default:
								return expr;
						}
					}
				default:
					Error(mTokens[mIndex].loc, "期望一个标识符");
					return null;
			}
		}
		private NameExpression ParseNameExpression()
		{
			var k = mTokens[mIndex];
			if (!Expect(Token.IDENTIFIER))
				return null;

			NameExpression expr = new NameExpression(null, k.value, k.loc);
			while (true)
			{
				switch (mTokens[mIndex].token)
				{
					case Token.DOT:
						++mIndex;
						k = mTokens[mIndex];
						if (Expect(Token.IDENTIFIER))
							expr = new NameExpression(expr, k.value, k.loc);
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
						expr = new SimpleAssign(expr, ParseExpression());
						break;
					case Token.OP_MULT_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(Binary.Operator.Multiply, expr, ParseExpression());
						break;
					case Token.OP_DIV_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(Binary.Operator.Division, expr, ParseExpression());
						break;
					case Token.OP_MOD_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(Binary.Operator.Modulus, expr, ParseExpression());
						break;
					case Token.OP_ADD_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(Binary.Operator.Addition, expr, ParseExpression());
						break;
					case Token.OP_SUB_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(Binary.Operator.Subtraction, expr, ParseExpression());
						break;
					case Token.OP_SHIFT_LEFT_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(Binary.Operator.LeftShift, expr, ParseExpression());
						break;
					case Token.OP_SHIFT_RIGHT_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(Binary.Operator.RightShift, expr, ParseExpression());
						break;
					case Token.OP_AND_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(Binary.Operator.BitwiseAnd, expr, ParseExpression());
						break;
					case Token.OP_XOR_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(Binary.Operator.ExclusiveOr, expr, ParseExpression());
						break;
					case Token.OP_OR_ASSIGN:
						++mIndex;
						expr = new CompoundAssign(Binary.Operator.BitwiseOr, expr, ParseExpression());
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

					expr = new Conditional(expr, e1, e2, expr.Location);
				}
			}
			return expr;
		}

		private Expression ParseValueExpression()
		{
			//if (mTokens[mIndex].token == Token.OPEN_BRACE)
			//{
			//	++mIndex;
			//	ValueListExpression expr = new ValueListExpression();
			//	while (mTokens[mIndex].token != Token.CLOSE_BRACE)
			//	{
			//		expr.Add(ParseConstantExpression());
			//		if (mTokens[mIndex].token != Token.COMMA)
			//			break;
			//		++mIndex;
			//	}
			//	Expect(Token.CLOSE_BRACE);
			//	return expr;
			//}
			if (mTokens[mIndex].token == Token.DEFAULT)
			{
				var loc = mTokens[mIndex].loc;
				++mIndex;
				return new Default(loc);
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
					expr = new Binary(Binary.Operator.LogicalOr, expr, ParseLogicalAndExpression());
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
					expr = new Binary(Binary.Operator.LogicalAnd, expr, ParseOrExpression());
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
					expr = new Binary(Binary.Operator.BitwiseOr, expr, ParseXorExpression());
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
					expr = new Binary(Binary.Operator.ExclusiveOr, expr, ParseAndExpression());
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
					expr = new Binary(Binary.Operator.BitwiseAnd, expr, ParseEqualityExpression());
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
						expr = new Binary(Binary.Operator.Equality, expr, ParseRelationalExpression());
						goto _loop;
					case Token.OP_NE:
						++mIndex;
						expr = new Binary(Binary.Operator.Inequality, expr, ParseRelationalExpression());
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
						expr = new Binary(Binary.Operator.LessThan, expr, ParseShiftExpression());
						goto _loop;
					case Token.OP_GT:
						++mIndex;
						expr = new Binary(Binary.Operator.GreaterThan, expr, ParseShiftExpression());
						goto _loop;
					case Token.OP_LE:
						++mIndex;
						expr = new Binary(Binary.Operator.LessThanOrEqual, expr, ParseShiftExpression());
						goto _loop;
					case Token.OP_GE:
						++mIndex;
						expr = new Binary(Binary.Operator.GreaterThanOrEqual, expr, ParseShiftExpression());
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
						expr = new Binary(Binary.Operator.LeftShift, expr, ParseAdditiveExpression());
						goto _loop;
					case Token.OP_SHIFT_RIGHT:
						++mIndex;
						expr = new Binary(Binary.Operator.RightShift, expr, ParseAdditiveExpression());
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
						expr = new Binary(Binary.Operator.Addition, expr, ParseMultiplicativeExpression());
						goto _loop;
					case Token.MINUS:
						++mIndex;
						expr = new Binary(Binary.Operator.Subtraction, expr, ParseMultiplicativeExpression());
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
						expr = new Binary(Binary.Operator.Multiply, expr, ParseCastExpression());
						goto _loop;
					case Token.DIV:
						++mIndex;
						expr = new Binary(Binary.Operator.Division, expr, ParseCastExpression());
						goto _loop;
					case Token.PERCENT:
						++mIndex;
						expr = new Binary(Binary.Operator.Modulus, expr, ParseCastExpression());
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
				var type = ParseTypeExpression();
				if (type != null && mTokens[mIndex].token == Token.CLOSE_PARENS)
				{
					Expression val = ParseCastExpression();
					if (val != null)
					{
						AppendErrors(PopError());
						return new TypeCast(val, type);
					}
				}
				PopError();
				mIndex = index;
			}
			return ParseUnaryExpression();
		}
		private Expression ParseUnaryExpression()
		{
			Location loc;
			switch (mTokens[mIndex].token)
			{
				case Token.OP_INC:
					loc = mTokens[mIndex].loc;
					++mIndex;
					return new UnaryMutator(UnaryMutator.Mode.PreIncrement, ParseUnaryExpression(), loc);
				case Token.OP_DEC:
					loc = mTokens[mIndex].loc;
					++mIndex;
					return new UnaryMutator(UnaryMutator.Mode.PreDecrement, ParseUnaryExpression(), loc);
				case Token.MINUS:
					loc = mTokens[mIndex].loc;
					++mIndex;
					return new Unary(Unary.Operator.UnaryNegation, ParseCastExpression(), loc);
				case Token.TILDE:
					loc = mTokens[mIndex].loc;
					++mIndex;
					return new Unary(Unary.Operator.UnaryNot, ParseCastExpression(), loc);
				case Token.BANG:
					loc = mTokens[mIndex].loc;
					++mIndex;
					return new Unary(Unary.Operator.LogicalNot, ParseCastExpression(), loc);
			}
			return ParsePostfixExpression();
		}
		private Expression ParsePostfixExpression()
		{
			Location loc;
			var expr = ParsePrimaryExpression();
			if (expr != null)
			{
				Arguments va;
				_loop:
				switch (mTokens[mIndex].token)
				{
					case Token.OPEN_BRACKET:
						++mIndex;
						va = new Arguments(2);
						do
						{
							va.Add(new Argument(ParseExpression()));
							if (mTokens[mIndex].token != Token.COMMA)
								break;
							++mIndex;
						} while (mTokens[mIndex].token != Token.CLOSE_BRACKET);
						Expect(Token.CLOSE_BRACKET);
						expr = new ElementAccess(expr, va, expr.Location);
						goto _loop;
					case Token.OPEN_PARENS:
						++mIndex;
						va = new Arguments(2);
						while (mTokens[mIndex].token != Token.CLOSE_BRACKET)
						{
							va.Add(new Argument(ParseExpression()));
							if (mTokens[mIndex].token != Token.COMMA)
								break;
							++mIndex;
						}
						Expect(Token.CLOSE_BRACKET);
						expr = new Invocation(expr, va);
						goto _loop;
					case Token.GENERIC_LT:
						loc = mTokens[mIndex].loc;
						++mIndex;
						TypeArguments ta = new TypeArguments();
						while (mTokens[mIndex].token != Token.GENERIC_GT)
						{
							ta.Add(ParseTypeExpression());
							if (mTokens[mIndex].token != Token.COMMA)
								break;
							++mIndex;
						}
						Expect(Token.CLOSE_BRACKET);
						expr = new GenericAccess(expr, ta, loc);
						goto _loop;
					case Token.DOT:
						++mIndex;
						var name = mTokens[mIndex];
						if (Expect(Token.IDENTIFIER))
						{
							expr = new MemberAccess(expr, name.value, name.loc);
							goto _loop;
						}
						break;
					case Token.OP_INC:
						++mIndex;
						expr = new UnaryMutator(UnaryMutator.Mode.PostIncrement, expr, expr.Location);
						goto _loop;
					case Token.OP_DEC:
						++mIndex;
						expr = new UnaryMutator(UnaryMutator.Mode.PostDecrement, expr, expr.Location);
						goto _loop;
				}
			}
			return expr;
		}
		private Expression ParsePrimaryExpression()
		{
			Expression type;
			TokenValue tk;
			switch (mTokens[mIndex].token)
			{
				case Token.IDENTIFIER:
					return ParseTypeExpression();
				case Token.STRING_LITERAL:
					tk = mTokens[mIndex++];
					return new StringLiteral(tk.value, tk.loc);
				case Token.NUMBER_LITERAL:
					tk = mTokens[mIndex++];
					return new IntLiteral(int.Parse(tk.value), tk.loc);
				case Token.TRUE:
					return new BoolLiteral(true, mTokens[mIndex++].loc);
				case Token.FALSE:
					return new BoolLiteral(false, mTokens[mIndex++].loc);
				case Token.NULL:
					return new NullConstant(BuildinTypeSpec.Object, mTokens[mIndex++].loc);
				case Token.THIS:
					return new This(mTokens[mIndex++].loc);
				case Token.BASE:
					return new BaseThis(mTokens[mIndex++].loc);
				case Token.OPEN_PARENS:
					++mIndex;
					var expr = ParseExpression();
					Expect(Token.CLOSE_PARENS);
					return expr;
				case Token.NEW:
					tk = mTokens[mIndex++];
					++mIndex;
					type = ParseTypeExpression();
					if (type == null)
						break;
				//	bool is_array = false;
					Arguments args = null;
				//	ValueListExpression value = null;
					if (mTokens[mIndex].token == Token.OPEN_PARENS)
					{
						++mIndex;
						args = new Arguments(2);
						while (mTokens[mIndex].token != Token.CLOSE_PARENS)
						{
							args.Add(new Argument(ParseExpression()));
							if (mTokens[mIndex].token != Token.COMMA)
								break;
							++mIndex;
						}
						Expect(Token.CLOSE_PARENS);
					}
					//else if (mTokens[mIndex].token == Token.OPEN_BRACKET)
					//{
					//	is_array = true;
					//	++mIndex;
					//	args = new ValueListExpression();
					//	do
					//	{
					//		args.Add(ParseExpression());
					//		if (mTokens[mIndex].token != Token.COMMA)
					//			break;
					//		++mIndex;
					//	} while (mTokens[mIndex].token != Token.CLOSE_BRACKET);
					//	Expect(Token.CLOSE_BRACKET);
					//}
					//if (mTokens[mIndex].token == Token.OPEN_BRACE)
					//{
					//	++mIndex;
					//	value = new ValueListExpression();
					//	while (mTokens[mIndex].token != Token.CLOSE_BRACE)
					//	{
					//		value.Add(ParseExpression());
					//		if (mTokens[mIndex].token != Token.COMMA)
					//			break;
					//		++mIndex;
					//	}
					//	Expect(Token.CLOSE_BRACE);
					//}
					return new New(type, args, tk.loc);
				//case Token.TYPEOF:
				//	++mIndex;
				//	if (Expect(Token.OPEN_PARENS))
				//	{
				//		int index = mIndex;
				//		PushError();
				//		type = ParseTypeExpression();
				//		if (type == null || mTokens[mIndex].token != Token.CLOSE_PARENS)
				//		{
				//			PopError();
				//			mIndex = index;
				//			type = ParseExpression();
				//		}
				//		else
				//			AppendErrors(PopError());
				//		Expect(Token.CLOSE_PARENS);
				//		return new TypeOf(type);
				//	}
				//	return null;
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

		private ParameterAttribute ParseParamAttribute(bool have_this)
		{
			ParameterAttribute attr = ParameterAttribute.NONE;
			while (true)
			{
				ParameterAttribute add = ParameterAttribute.NONE;
				switch (mTokens[mIndex].token)
				{
					case Token.IN:
						add = ParameterAttribute.IN;
						break;
					case Token.OUT:
						add = ParameterAttribute.OUT;
						break;
					case Token.REF:
						add = ParameterAttribute.REF;
						break;
					case Token.PARAMS:
						add = ParameterAttribute.PARAMS;
						break;
					case Token.THIS:
						if (!have_this)
							return attr;
						add = ParameterAttribute.THIS;
						break;
					default:
						return attr;
				}

				if ((attr & add) != ParameterAttribute.NONE)
					Error(mTokens[mIndex].loc, "重复声明属性 " + mTokens[mIndex].ToString());
				++mIndex;
				attr |= add;
			}
		}

		private AstNamespace ParseNamespaceStatement(AstNamespace parent)
		{
			++mIndex;
			var name = ParseNameExpression();
			if (name == null)
				return null;

			if (!Expect(Token.OPEN_BRACE))
				return null;

			AstNamespace result = name.CreateNamespace(parent);

			while (mTokens[mIndex].token != Token.CLOSE_BRACE && mTokens[mIndex].token != Token.EOF)
			{
				int next = mIndex + 1;
				ParseDeclareStatement(result);
				if (mIndex < next)
				{
					NotExpect();
					mIndex = next;
				}
			}

			Expect(Token.CLOSE_BRACE);
			return result;
		}

		private AstTypeDefine ParseTypeStatement(CommonAttribute attr, AstContainer parent, AstKind kind)
		{
			++mIndex;

			var loc = mTokens[mIndex].loc;
			var name = ParseMemberName();
			if (name == null)
				return null;

			AstTypeDefine result = new AstTypeDefine(parent, name, kind, loc);

			//if (mTokens[mIndex].token == Token.COLON)
			//{
			//	result.Bases = new List<FullNamedExpression>();
			//	do
			//	{
			//		++mIndex;
			//		var bs = ParseTypeExpression();
			//		if (bs != null)
			//			result.Bases.Add(bs);
			//	} while (mTokens[mIndex].token == Token.COMMA);
			//}

			if (!Expect(Token.OPEN_BRACE))
				return result;

			if (kind == AstKind.Enum)
			{
				SimpleName typename = new SimpleName(name.Name, name.Location);
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

					AstField field = new AstField(result, typename, new AstName(id.value, id.loc), id.loc);
					field.Value = val;
					result.Add(field);

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
				ParseDeclareStatement(result);
				if (mIndex < next)
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
