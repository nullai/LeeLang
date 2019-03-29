using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace leec
{
	public abstract class SyntaxParser : IDisposable
	{
		protected struct ResetPoint
		{
			public readonly int ResetCount;
			public readonly LexerMode Mode;
			public readonly int Position;
			public readonly GreenNode PrevTokenTrailingTrivia;

			public ResetPoint(int resetCount, LexerMode mode, int position, GreenNode prevTokenTrailingTrivia)
			{
				this.ResetCount = resetCount;
				this.Mode = mode;
				this.Position = position;
				this.PrevTokenTrailingTrivia = prevTokenTrailingTrivia;
			}
		}

		protected readonly Lexer lexer;
		protected readonly CancellationToken cancellationToken;

		private LexerMode _mode;
		private SyntaxToken _currentToken;
		private SyntaxToken[] _lexedTokens;
		private GreenNode _prevTokenTrailingTrivia;
		private int _firstToken; // The position of _lexedTokens[0] (or _blendedTokens[0]).
		private int _tokenOffset; // The index of the current token within _lexedTokens or _blendedTokens.
		private int _tokenCount;
		private int _resetCount;
		private int _resetStart;

		protected SyntaxParser(
			Lexer lexer,
			LexerMode mode,
			bool preLexIfNotIncremental = false,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			this.lexer = lexer;
			_mode = mode;
			this.cancellationToken = cancellationToken;
			_lexedTokens = new SyntaxToken[32];

			// PreLex is not cancellable. 
			//      If we may cancel why would we aggressively lex ahead?
			//      Cancellations in a constructor make disposing complicated
			//
			// So, if we have a real cancellation token, do not do prelexing.
			if (preLexIfNotIncremental && !cancellationToken.CanBeCanceled)
			{
				this.PreLex();
			}
		}

		public void Dispose()
		{
		}

		protected void ReInitialize()
		{
			_firstToken = 0;
			_tokenOffset = 0;
			_tokenCount = 0;
			_resetCount = 0;
			_resetStart = 0;
			_currentToken = null;
			_prevTokenTrailingTrivia = null;
		}

		private void PreLex()
		{
			// NOTE: Do not cancel in this method. It is called from the constructor.
			var size = Math.Min(4096, Math.Max(32, this.lexer.TextWindow.Text.Length / 2));
			_lexedTokens = new SyntaxToken[size];
			var lexer = this.lexer;
			var mode = _mode;

			for (int i = 0; i < size; i++)
			{
				var token = lexer.Lex(mode);
				this.AddLexedToken(token);
				if (token.Kind == SyntaxKind.EndOfFileToken)
				{
					break;
				}
			}
		}

		protected ResetPoint GetResetPoint()
		{
			var pos = CurrentTokenPosition;
			if (_resetCount == 0)
			{
				_resetStart = pos; // low water mark
			}

			_resetCount++;
			return new ResetPoint(_resetCount, _mode, pos, _prevTokenTrailingTrivia);
		}

		protected void Reset(ref ResetPoint point)
		{
			_mode = point.Mode;
			var offset = point.Position - _firstToken;
			Debug.Assert(offset >= 0 && offset < _tokenCount);
			_tokenOffset = offset;
			_currentToken = default(SyntaxToken);
			_prevTokenTrailingTrivia = point.PrevTokenTrailingTrivia;
		}

		protected void Release(ref ResetPoint point)
		{
			Debug.Assert(_resetCount == point.ResetCount);
			_resetCount--;
			if (_resetCount == 0)
			{
				_resetStart = -1;
			}
		}

		public ParseOptions Options
		{
			get { return this.lexer.Options; }
		}

		protected LexerMode Mode
		{
			get
			{
				return _mode;
			}
		}

		protected GreenNode CurrentNode
		{
			get
			{
				return _currentToken;
			}
		}

		protected SyntaxKind CurrentNodeKind
		{
			get
			{
				var cn = this.CurrentNode;
				return cn != null ? cn.Kind : SyntaxKind.None;
			}
		}

		protected GreenNode EatNode()
		{
			// remember result
			var result = CurrentNode;
			_currentToken = null;
			return result;
		}

		protected SyntaxToken CurrentToken
		{
			get
			{
				return _currentToken ?? (_currentToken = this.FetchCurrentToken());
			}
		}

		private SyntaxToken FetchCurrentToken()
		{
			if (_tokenOffset >= _tokenCount)
			{
				this.AddNewToken();
			}

			return _lexedTokens[_tokenOffset];
		}

		private void AddNewToken()
		{
			this.AddLexedToken(this.lexer.Lex(_mode));
		}

		private void AddLexedToken(SyntaxToken token)
		{
			Debug.Assert(token != null);
			if (_tokenCount >= _lexedTokens.Length)
			{
				this.AddLexedTokenSlot();
			}

			_lexedTokens[_tokenCount] = token;
			_tokenCount++;
		}

		private void AddLexedTokenSlot()
		{
			// shift tokens to left if we are far to the right
			// don't shift if reset points have fixed locked tge starting point at the token in the window
			if (_tokenOffset > (_lexedTokens.Length >> 1)
				&& (_resetStart == -1 || _resetStart > _firstToken))
			{
				int shiftOffset = (_resetStart == -1) ? _tokenOffset : _resetStart - _firstToken;
				int shiftCount = _tokenCount - shiftOffset;
				Debug.Assert(shiftOffset > 0);
				if (shiftCount > 0)
				{
					Array.Copy(_lexedTokens, shiftOffset, _lexedTokens, 0, shiftCount);
				}

				_firstToken += shiftOffset;
				_tokenCount -= shiftOffset;
				_tokenOffset -= shiftOffset;
			}
			else
			{
				var tmp = new SyntaxToken[_lexedTokens.Length * 2];
				Array.Copy(_lexedTokens, tmp, _lexedTokens.Length);
				_lexedTokens = tmp;
			}
		}

		protected SyntaxToken PeekToken(int n)
		{
			Debug.Assert(n >= 0);
			while (_tokenOffset + n >= _tokenCount)
			{
				this.AddNewToken();
			}

			return _lexedTokens[_tokenOffset + n];
		}

		//this method is called very frequently
		//we should keep it simple so that it can be inlined.
		protected SyntaxToken EatToken()
		{
			var ct = this.CurrentToken;
			MoveToNextToken();
			return ct;
		}

		private void MoveToNextToken()
		{
			_prevTokenTrailingTrivia = _currentToken.GetTrailingTrivia();

			_currentToken = default(SyntaxToken);

			_tokenOffset++;
		}

		protected void ForceEndOfFile()
		{
			_currentToken = SyntaxFactory.Token(SyntaxKind.EndOfFileToken);
		}

		//this method is called very frequently
		//we should keep it simple so that it can be inlined.
		protected SyntaxToken EatToken(SyntaxKind kind)
		{
			Debug.Assert(SyntaxFacts.IsAnyToken(kind));

			var ct = this.CurrentToken;
			if (ct.Kind == kind)
			{
				MoveToNextToken();
				return ct;
			}

			//slow part of EatToken(SyntaxKind kind)
			return CreateMissingToken(kind, this.CurrentToken.Kind, reportError: true);
		}

		// Consume a token if it is the right kind. Otherwise skip a token and replace it with one of the correct kind.
		protected SyntaxToken EatTokenAsKind(SyntaxKind expected)
		{
			Debug.Assert(SyntaxFacts.IsAnyToken(expected));

			var ct = this.CurrentToken;
			if (ct.Kind == expected)
			{
				MoveToNextToken();
				return ct;
			}

			var replacement = CreateMissingToken(expected, this.CurrentToken.Kind, reportError: true);
			return AddTrailingSkippedSyntax(replacement, this.EatToken());
		}

		private SyntaxToken CreateMissingToken(SyntaxKind expected, SyntaxKind actual, bool reportError)
		{
			// should we eat the current ParseToken's leading trivia?
			var token = SyntaxFactory.MissingToken(expected);
			if (reportError)
			{
				token = WithAdditionalDiagnostics(token, this.GetExpectedTokenError(expected, actual));
			}

			return token;
		}

		private SyntaxToken CreateMissingToken(SyntaxKind expected, ErrorCode code, bool reportError)
		{
			// should we eat the current ParseToken's leading trivia?
			var token = SyntaxFactory.MissingToken(expected);
			if (reportError)
			{
				token = AddError(token, code);
			}

			return token;
		}

		protected SyntaxToken EatToken(SyntaxKind kind, bool reportError)
		{
			if (reportError)
			{
				return EatToken(kind);
			}

			Debug.Assert(SyntaxFacts.IsAnyToken(kind));
			if (this.CurrentToken.Kind != kind)
			{
				// should we eat the current ParseToken's leading trivia?
				return SyntaxFactory.MissingToken(kind);
			}
			else
			{
				return this.EatToken();
			}
		}

		protected SyntaxToken EatToken(SyntaxKind kind, ErrorCode code, bool reportError = true)
		{
			Debug.Assert(SyntaxFacts.IsAnyToken(kind));
			if (this.CurrentToken.Kind != kind)
			{
				return CreateMissingToken(kind, code, reportError);
			}
			else
			{
				return this.EatToken();
			}
		}

		protected SyntaxToken EatTokenWithPrejudice(SyntaxKind kind)
		{
			var token = this.CurrentToken;
			Debug.Assert(SyntaxFacts.IsAnyToken(kind));
			if (token.Kind != kind)
			{
				token = WithAdditionalDiagnostics(token, this.GetExpectedTokenError(kind, token.Kind));
			}

			this.MoveToNextToken();
			return token;
		}

		protected SyntaxToken EatTokenWithPrejudice(ErrorCode errorCode, params object[] args)
		{
			var token = this.EatToken();
			token = WithAdditionalDiagnostics(token, MakeError(token.GetLeadingTriviaWidth(), token.Width, errorCode, args));
			return token;
		}

		protected SyntaxToken EatContextualToken(SyntaxKind kind, ErrorCode code, bool reportError = true)
		{
			Debug.Assert(SyntaxFacts.IsAnyToken(kind));

			if (this.CurrentToken.ContextualKind != kind)
			{
				return CreateMissingToken(kind, code, reportError);
			}
			else
			{
				return ConvertToKeyword(this.EatToken());
			}
		}

		protected SyntaxToken EatContextualToken(SyntaxKind kind, bool reportError = true)
		{
			Debug.Assert(SyntaxFacts.IsAnyToken(kind));

			var contextualKind = this.CurrentToken.ContextualKind;
			if (contextualKind != kind)
			{
				return CreateMissingToken(kind, contextualKind, reportError);
			}
			else
			{
				return ConvertToKeyword(this.EatToken());
			}
		}

		protected virtual SyntaxDiagnosticInfo GetExpectedTokenError(SyntaxKind expected, SyntaxKind actual, int offset, int width)
		{
			var code = GetExpectedTokenErrorCode(expected, actual);
			if (code == ErrorCode.ERR_SyntaxError || code == ErrorCode.ERR_IdentifierExpectedKW)
			{
				return new SyntaxDiagnosticInfo(offset, width, code, SyntaxFacts.GetText(expected), SyntaxFacts.GetText(actual));
			}
			else
			{
				return new SyntaxDiagnosticInfo(offset, width, code);
			}
		}

		protected virtual SyntaxDiagnosticInfo GetExpectedTokenError(SyntaxKind expected, SyntaxKind actual)
		{
			int offset, width;
			this.GetDiagnosticSpanForMissingToken(out offset, out width);

			return this.GetExpectedTokenError(expected, actual, offset, width);
		}

		private static ErrorCode GetExpectedTokenErrorCode(SyntaxKind expected, SyntaxKind actual)
		{
			switch (expected)
			{
				case SyntaxKind.IdentifierToken:
					if (SyntaxFacts.IsReservedKeyword(actual))
					{
						return ErrorCode.ERR_IdentifierExpectedKW;   // A keyword -- use special message.
					}
					else
					{
						return ErrorCode.ERR_IdentifierExpected;
					}

				case SyntaxKind.SemicolonToken:
					return ErrorCode.ERR_SemicolonExpected;

				// case TokenKind::Colon:         iError = ERR_ColonExpected;          break;
				// case TokenKind::OpenParen:     iError = ERR_LparenExpected;         break;
				case SyntaxKind.CloseParenToken:
					return ErrorCode.ERR_CloseParenExpected;
				case SyntaxKind.OpenBraceToken:
					return ErrorCode.ERR_LbraceExpected;
				case SyntaxKind.CloseBraceToken:
					return ErrorCode.ERR_RbraceExpected;

				// case TokenKind::CloseSquare:   iError = ERR_CloseSquareExpected;    break;
				default:
					return ErrorCode.ERR_SyntaxError;
			}
		}

		protected void GetDiagnosticSpanForMissingToken(out int offset, out int width)
		{
			// If the previous token has a trailing EndOfLineTrivia,
			// the missing token diagnostic position is moved to the
			// end of line containing the previous token and
			// its width is set to zero.
			// Otherwise the diagnostic offset and width is set
			// to the corresponding values of the current token

			var trivia = _prevTokenTrailingTrivia;
			if (trivia != null)
			{
				SyntaxList<LeeSyntaxNode> triviaList = new SyntaxList<LeeSyntaxNode>(trivia);
				bool prevTokenHasEndOfLineTrivia = triviaList.Any((int)SyntaxKind.EndOfLineTrivia);
				if (prevTokenHasEndOfLineTrivia)
				{
					offset = -trivia.FullWidth;
					width = 0;
					return;
				}
			}

			SyntaxToken ct = this.CurrentToken;
			offset = ct.GetLeadingTriviaWidth();
			width = ct.Width;
		}

		protected virtual TNode WithAdditionalDiagnostics<TNode>(TNode node, params DiagnosticInfo[] diagnostics) where TNode : GreenNode
		{
			DiagnosticInfo[] existingDiags = node.GetDiagnostics();
			int existingLength = existingDiags.Length;
			if (existingLength == 0)
			{
				return node.WithDiagnosticsGreen(diagnostics);
			}
			else
			{
				DiagnosticInfo[] result = new DiagnosticInfo[existingDiags.Length + diagnostics.Length];
				existingDiags.CopyTo(result, 0);
				diagnostics.CopyTo(result, existingLength);
				return node.WithDiagnosticsGreen(result);
			}
		}

		protected TNode AddError<TNode>(TNode node, ErrorCode code) where TNode : GreenNode
		{
			return AddError(node, code, new object[0]);
		}

		protected TNode AddError<TNode>(TNode node, ErrorCode code, params object[] args) where TNode : GreenNode
		{
			if (!node.IsMissing)
			{
				return WithAdditionalDiagnostics(node, MakeError(node, code, args));
			}

			int offset, width;

			SyntaxToken token = node as SyntaxToken;
			if (token != null && token.ContainsSkippedText)
			{
				// This code exists to clean up an anti-pattern:
				//   1) an undesirable token is parsed,
				//   2) a desirable missing token is created and the parsed token is appended as skipped text,
				//   3) an error is attached to the missing token describing the problem.
				// If this occurs, then this.previousTokenTrailingTrivia is still populated with the trivia 
				// of the undesirable token (now skipped text).  Since the trivia no longer precedes the
				// node to which the error is to be attached, the computed offset will be incorrect.

				offset = token.GetLeadingTriviaWidth(); // Should always be zero, but at least we'll do something sensible if it's not.
				Debug.Assert(offset == 0, "Why are we producing a missing token that has both skipped text and leading trivia?");

				width = 0;
				bool seenSkipped = false;
				foreach (var trivia in token.TrailingTrivia)
				{
					if (trivia.Kind == SyntaxKind.SkippedTokensTrivia)
					{
						seenSkipped = true;
						width += trivia.Width;
					}
					else if (seenSkipped)
					{
						break;
					}
					else
					{
						offset += trivia.Width;
					}
				}
			}
			else
			{
				this.GetDiagnosticSpanForMissingToken(out offset, out width);
			}

			return WithAdditionalDiagnostics(node, MakeError(offset, width, code, args));
		}

		protected TNode AddError<TNode>(TNode node, int offset, int length, ErrorCode code, params object[] args) where TNode : LeeSyntaxNode
		{
			return WithAdditionalDiagnostics(node, MakeError(offset, length, code, args));
		}

		protected TNode AddError<TNode>(TNode node, LeeSyntaxNode location, ErrorCode code, params object[] args) where TNode : LeeSyntaxNode
		{
			// assumes non-terminals will at most appear once in sub-tree
			int offset;
			FindOffset(node, location, out offset);
			return WithAdditionalDiagnostics(node, MakeError(offset, location.Width, code, args));
		}

		protected TNode AddErrorToFirstToken<TNode>(TNode node, ErrorCode code) where TNode : LeeSyntaxNode
		{
			var firstToken = node.GetFirstToken();
			return WithAdditionalDiagnostics(node, MakeError(firstToken.GetLeadingTriviaWidth(), firstToken.Width, code));
		}

		protected TNode AddErrorToFirstToken<TNode>(TNode node, ErrorCode code, params object[] args) where TNode : LeeSyntaxNode
		{
			var firstToken = node.GetFirstToken();
			return WithAdditionalDiagnostics(node, MakeError(firstToken.GetLeadingTriviaWidth(), firstToken.Width, code, args));
		}

		protected TNode AddErrorToLastToken<TNode>(TNode node, ErrorCode code) where TNode : LeeSyntaxNode
		{
			int offset;
			int width;
			GetOffsetAndWidthForLastToken(node, out offset, out width);
			return WithAdditionalDiagnostics(node, MakeError(offset, width, code));
		}

		protected TNode AddErrorToLastToken<TNode>(TNode node, ErrorCode code, params object[] args) where TNode : LeeSyntaxNode
		{
			int offset;
			int width;
			GetOffsetAndWidthForLastToken(node, out offset, out width);
			return WithAdditionalDiagnostics(node, MakeError(offset, width, code, args));
		}

		private static void GetOffsetAndWidthForLastToken<TNode>(TNode node, out int offset, out int width) where TNode : LeeSyntaxNode
		{
			var lastToken = node.GetLastNonmissingToken();
			offset = node.FullWidth; //advance to end of entire node
			width = 0;
			if (lastToken != null) //will be null if all tokens are missing
			{
				offset -= lastToken.FullWidth; //rewind past last token
				offset += lastToken.GetLeadingTriviaWidth(); //advance past last token leading trivia - now at start of last token
				width += lastToken.Width;
			}
		}

		protected static SyntaxDiagnosticInfo MakeError(int offset, int width, ErrorCode code)
		{
			return new SyntaxDiagnosticInfo(offset, width, code);
		}

		protected static SyntaxDiagnosticInfo MakeError(int offset, int width, ErrorCode code, params object[] args)
		{
			return new SyntaxDiagnosticInfo(offset, width, code, args);
		}

		protected static SyntaxDiagnosticInfo MakeError(GreenNode node, ErrorCode code, params object[] args)
		{
			return new SyntaxDiagnosticInfo(node.GetLeadingTriviaWidth(), node.Width, code, args);
		}

		protected static SyntaxDiagnosticInfo MakeError(ErrorCode code, params object[] args)
		{
			return new SyntaxDiagnosticInfo(code, args);
		}

		protected TNode AddLeadingSkippedSyntax<TNode>(TNode node, GreenNode skippedSyntax) where TNode : LeeSyntaxNode
		{
			var oldToken = node as SyntaxToken ?? node.GetFirstToken();
			var newToken = AddSkippedSyntax(oldToken, skippedSyntax, trailing: false);
			return node;
		}

		protected void AddTrailingSkippedSyntax(SyntaxListBuilder list, GreenNode skippedSyntax)
		{
			list[list.Count - 1] = AddTrailingSkippedSyntax((LeeSyntaxNode)list[list.Count - 1], skippedSyntax);
		}

		protected TNode AddTrailingSkippedSyntax<TNode>(TNode node, GreenNode skippedSyntax) where TNode : LeeSyntaxNode
		{
			var token = node as SyntaxToken;
			if (token != null)
			{
				return (TNode)(object)AddSkippedSyntax(token, skippedSyntax, trailing: true);
			}
			else
			{
				var lastToken = node.GetLastToken();
				var newToken = AddSkippedSyntax(lastToken, skippedSyntax, trailing: true);
				return node;
			}
		}

		/// <summary>
		/// Converts skippedSyntax node into tokens and adds these as trivia on the target token.
		/// Also adds the first error (in depth-first preorder) found in the skipped syntax tree to the target token.
		/// </summary>
		public SyntaxToken AddSkippedSyntax(SyntaxToken target, GreenNode skippedSyntax, bool trailing)
		{
			return target;
		}

		/// <summary>
		/// This function searches for the given location node within the subtree rooted at root node. 
		/// If it finds it, the function computes the offset span of that child node within the root and returns true, 
		/// otherwise it returns false.
		/// </summary>
		/// <param name="root">Root node</param>
		/// <param name="location">Node to search in the subtree rooted at root node</param>
		/// <param name="offset">Offset of the location node within the subtree rooted at child</param>
		/// <returns></returns>
		private bool FindOffset(GreenNode root, LeeSyntaxNode location, out int offset)
		{
			int currentOffset = 0;
			offset = 0;
			if (root != null)
			{
				for (int i = 0, n = root.SlotCount; i < n; i++)
				{
					var child = root.GetSlot(i);
					if (child == null)
					{
						// ignore null slots
						continue;
					}

					// check if the child node is the location node
					if (child == location)
					{
						// Found the location node in the subtree
						// Initialize offset with the offset of the location node within its parent
						// and walk up the stack of recursive calls adding the offset of each node
						// within its parent
						offset = currentOffset;
						return true;
					}

					// search for the location node in the subtree rooted at child node
					if (this.FindOffset(child, location, out offset))
					{
						// Found the location node in child's subtree
						// Add the offset of child node within its parent to offset
						// and continue walking up the stack
						offset += child.GetLeadingTriviaWidth() + currentOffset;
						return true;
					}

					// We didn't find the location node in the subtree rooted at child
					// Move on to the next child
					currentOffset += child.FullWidth;
				}
			}

			// We didn't find the location node within the subtree rooted at root node
			return false;
		}

		protected static SyntaxToken ConvertToKeyword(SyntaxToken token)
		{
			if (token.Kind != token.ContextualKind)
			{
				var kw = token.IsMissing
						? SyntaxFactory.MissingToken(token.LeadingTrivia.Node, token.ContextualKind, token.TrailingTrivia.Node)
						: SyntaxFactory.Token(token.LeadingTrivia.Node, token.ContextualKind, token.TrailingTrivia.Node);
				var d = token.GetDiagnostics();
				if (d != null && d.Length > 0)
				{
					kw = kw.WithDiagnosticsGreen(d);
				}

				return kw;
			}

			return token;
		}

		public DirectiveStack Directives
		{
			get { return lexer.Directives; }
		}

		/// <summary>
		/// Whenever parsing in a <c>while (true)</c> loop and a bug could prevent the loop from making progress,
		/// this method can prevent the parsing from hanging.
		/// Use as:
		///     int tokenProgress = -1;
		///     while (IsMakingProgress(ref tokenProgress))
		/// It should be used as a guardrail, not as a crutch, so it asserts if no progress was made.
		/// </summary>
		protected bool IsMakingProgress(ref int lastTokenPosition)
		{
			var pos = CurrentTokenPosition;
			if (pos > lastTokenPosition)
			{
				lastTokenPosition = pos;
				return true;
			}

			Debug.Assert(false);
			return false;
		}

		private int CurrentTokenPosition => _firstToken + _tokenOffset;
	}
}
