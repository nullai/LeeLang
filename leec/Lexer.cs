using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace leec
{
	[Flags]
	public enum LexerMode
	{
		Syntax = 0x0001,
		DebuggerSyntax = 0x0002,
		Directive = 0x0004,
		MaskLexMode = 0xFFFF,

		None = 0
	}

	public class Lexer : AbstractLexer
	{
		private const int TriviaListInitialCapacity = 8;

		private readonly ParseOptions _options;
		private LexerMode _mode;
		private readonly StringBuilder _builder;
		private char[] _identBuffer;
		private int _identLen;
		private DirectiveStack _directives;
        private readonly LexerCache _cache;
		private readonly bool _allowPreprocessorDirectives;
		private readonly bool _interpolationFollowedByColon;
		private int _badTokenCount; // cumulative count of bad tokens produced

		public struct TokenInfo
		{
			// scanned values
			public SyntaxKind Kind;
			public SyntaxKind ContextualKind;
			public string Text;
			public SpecialType ValueKind;
			public bool HasIdentifierEscapeSequence;
			public string StringValue;
			public char CharValue;
			public int IntValue;
			public uint UintValue;
			public long LongValue;
			public ulong UlongValue;
			public float FloatValue;
			public double DoubleValue;
			public decimal DecimalValue;
			public bool IsVerbatim;
		}

		public Lexer(SourceText text, ParseOptions options, bool allowPreprocessorDirectives = true, bool interpolationFollowedByColon = false)
			: base(text)
		{
            _options = options;
			_builder = new StringBuilder();
			_identBuffer = new char[32];
            _cache = new LexerCache();
			_allowPreprocessorDirectives = allowPreprocessorDirectives;
			_interpolationFollowedByColon = interpolationFollowedByColon;
		}

		public override void Dispose()
		{
			_cache.Free();

			base.Dispose();
		}

		public ParseOptions Options
		{
			get { return _options; }
		}
		public DirectiveStack Directives => _directives;
		public bool InterpolationFollowedByColon => _interpolationFollowedByColon;

		public void Reset(int position, DirectiveStack directives)
		{
			this.TextWindow.Reset(position);
			_directives = directives;
		}
		private static LexerMode ModeOf(LexerMode mode)
		{
			return mode & LexerMode.MaskLexMode;
		}

		private bool ModeIs(LexerMode mode)
		{
			return ModeOf(_mode) == mode;
		}

		public SyntaxToken Lex(ref LexerMode mode)
		{
			var result = Lex(mode);
			mode = _mode;
			return result;
		}

		public SyntaxToken Lex(LexerMode mode)
		{
			_mode = mode;
			switch (_mode)
			{
				case LexerMode.Syntax:
				case LexerMode.DebuggerSyntax:
					return this.LexSyntaxToken();
				case LexerMode.Directive:
					return this.LexDirectiveToken();
			}

			throw ExceptionUtilities.UnexpectedValue(ModeOf(_mode));
		}

		private SyntaxListBuilder _leadingTriviaCache = new SyntaxListBuilder(10);
		private SyntaxListBuilder _trailingTriviaCache = new SyntaxListBuilder(10);

		private static int GetFullWidth(SyntaxListBuilder builder)
		{
			int width = 0;

			if (builder != null)
			{
				for (int i = 0; i < builder.Count; i++)
				{
					width += builder[i].FullWidth;
				}
			}

			return width;
		}

		private SyntaxToken LexSyntaxToken()
		{
			_leadingTriviaCache.Clear();
			this.LexSyntaxTrivia(afterFirstToken: TextWindow.Position > 0, isTrailing: false, triviaList: ref _leadingTriviaCache);
			var leading = _leadingTriviaCache;

			var tokenInfo = default(TokenInfo);

			this.Start();
			this.ScanSyntaxToken(ref tokenInfo);
			var errors = this.GetErrors(GetFullWidth(leading));

			_trailingTriviaCache.Clear();
			this.LexSyntaxTrivia(afterFirstToken: true, isTrailing: true, triviaList: ref _trailingTriviaCache);
			var trailing = _trailingTriviaCache;

			return Create(ref tokenInfo, leading, trailing, errors);
		}

		public SyntaxTriviaList LexSyntaxLeadingTrivia()
		{
			_leadingTriviaCache.Clear();
			this.LexSyntaxTrivia(afterFirstToken: TextWindow.Position > 0, isTrailing: false, triviaList: ref _leadingTriviaCache);
			return new SyntaxTriviaList(default(SyntaxToken),
				_leadingTriviaCache.ToListNode(), position: 0, index: 0);
		}

		public SyntaxTriviaList LexSyntaxTrailingTrivia()
		{
			_trailingTriviaCache.Clear();
			this.LexSyntaxTrivia(afterFirstToken: true, isTrailing: true, triviaList: ref _trailingTriviaCache);
			return new SyntaxTriviaList(default(SyntaxToken),
				_trailingTriviaCache.ToListNode(), position: 0, index: 0);
		}

		private SyntaxToken Create(ref TokenInfo info, SyntaxListBuilder leading, SyntaxListBuilder trailing, SyntaxDiagnosticInfo[] errors)
		{
			Debug.Assert(info.Kind != SyntaxKind.IdentifierToken || info.StringValue != null);

			var leadingNode = leading?.ToListNode();
			var trailingNode = trailing?.ToListNode();

			SyntaxToken token;
			switch (info.Kind)
			{
				case SyntaxKind.IdentifierToken:
					token = SyntaxFactory.Identifier(info.ContextualKind, leadingNode, info.Text, info.StringValue, trailingNode);
					break;
				case SyntaxKind.NumericLiteralToken:
					switch (info.ValueKind)
					{
						case SpecialType.System_Int32:
							token = SyntaxFactory.Literal(leadingNode, info.Text, info.IntValue, trailingNode);
							break;
						case SpecialType.System_UInt32:
							token = SyntaxFactory.Literal(leadingNode, info.Text, info.UintValue, trailingNode);
							break;
						case SpecialType.System_Int64:
							token = SyntaxFactory.Literal(leadingNode, info.Text, info.LongValue, trailingNode);
							break;
						case SpecialType.System_UInt64:
							token = SyntaxFactory.Literal(leadingNode, info.Text, info.UlongValue, trailingNode);
							break;
						case SpecialType.System_Single:
							token = SyntaxFactory.Literal(leadingNode, info.Text, info.FloatValue, trailingNode);
							break;
						case SpecialType.System_Double:
							token = SyntaxFactory.Literal(leadingNode, info.Text, info.DoubleValue, trailingNode);
							break;
						case SpecialType.System_Decimal:
							token = SyntaxFactory.Literal(leadingNode, info.Text, info.DecimalValue, trailingNode);
							break;
						default:
							throw ExceptionUtilities.UnexpectedValue(info.ValueKind);
					}

					break;
				case SyntaxKind.StringLiteralToken:
					token = SyntaxFactory.Literal(leadingNode, info.Text, info.Kind, info.StringValue, trailingNode);
					break;
				case SyntaxKind.CharacterLiteralToken:
					token = SyntaxFactory.Literal(leadingNode, info.Text, info.CharValue, trailingNode);
					break;
				case SyntaxKind.EndOfDocumentationCommentToken:
				case SyntaxKind.EndOfFileToken:
					token = SyntaxFactory.Token(leadingNode, info.Kind, trailingNode);
					break;
				case SyntaxKind.None:
					token = SyntaxFactory.BadToken(leadingNode, info.Text, trailingNode);
					break;

				default:
					Debug.Assert(SyntaxFacts.IsPunctuationOrKeyword(info.Kind));
					token = SyntaxFactory.Token(leadingNode, info.Kind, trailingNode);
					break;
			}

			if (errors != null)
			{
				token = token.WithDiagnosticsGreen(errors);
			}

			return token;
		}

		private void ScanSyntaxToken(ref TokenInfo info)
		{
			// Initialize for new token scan
			info.Kind = SyntaxKind.None;
			info.ContextualKind = SyntaxKind.None;
			info.Text = null;
			char character;
			char surrogateCharacter = SlidingTextWindow.InvalidCharacter;
			bool isEscaped = false;
			int startingPosition = TextWindow.Position;

			// Start scanning the token
			character = TextWindow.PeekChar();
			switch (character)
			{
				case '\"':
				case '\'':
					this.ScanStringLiteral(ref info);
					break;

				case '/':
					TextWindow.AdvanceChar();
					if (TextWindow.PeekChar() == '=')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.SlashEqualsToken;
					}
					else
					{
						info.Kind = SyntaxKind.SlashToken;
					}

					break;

				case '.':
					if (!this.ScanNumericLiteral(ref info))
					{
						TextWindow.AdvanceChar();
						if (TextWindow.PeekChar() == '.')
						{
							TextWindow.AdvanceChar();
							if (TextWindow.PeekChar() == '.')
							{
								// Triple-dot: explicitly reject this, to allow triple-dot
								// to be added to the language without a breaking change.
								// (without this, 0...2 would parse as (0)..(.2), i.e. a range from 0 to 0.2)
								this.AddError(ErrorCode.ERR_TripleDotNotAllowed);
							}

							info.Kind = SyntaxKind.DotDotToken;
						}
						else
						{
							info.Kind = SyntaxKind.DotToken;
						}
					}

					break;

				case ',':
					TextWindow.AdvanceChar();
					info.Kind = SyntaxKind.CommaToken;
					break;

				case ':':
					TextWindow.AdvanceChar();
					if (TextWindow.PeekChar() == ':')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.ColonColonToken;
					}
					else
					{
						info.Kind = SyntaxKind.ColonToken;
					}

					break;

				case ';':
					TextWindow.AdvanceChar();
					info.Kind = SyntaxKind.SemicolonToken;
					break;

				case '~':
					TextWindow.AdvanceChar();
					info.Kind = SyntaxKind.TildeToken;
					break;

				case '!':
					TextWindow.AdvanceChar();
					if (TextWindow.PeekChar() == '=')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.ExclamationEqualsToken;
					}
					else
					{
						info.Kind = SyntaxKind.ExclamationToken;
					}

					break;

				case '=':
					TextWindow.AdvanceChar();
					if ((character = TextWindow.PeekChar()) == '=')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.EqualsEqualsToken;
					}
					else if (character == '>')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.EqualsGreaterThanToken;
					}
					else
					{
						info.Kind = SyntaxKind.EqualsToken;
					}

					break;

				case '*':
					TextWindow.AdvanceChar();
					if (TextWindow.PeekChar() == '=')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.AsteriskEqualsToken;
					}
					else
					{
						info.Kind = SyntaxKind.AsteriskToken;
					}

					break;

				case '(':
					TextWindow.AdvanceChar();
					info.Kind = SyntaxKind.OpenParenToken;
					break;

				case ')':
					TextWindow.AdvanceChar();
					info.Kind = SyntaxKind.CloseParenToken;
					break;

				case '{':
					TextWindow.AdvanceChar();
					info.Kind = SyntaxKind.OpenBraceToken;
					break;

				case '}':
					TextWindow.AdvanceChar();
					info.Kind = SyntaxKind.CloseBraceToken;
					break;

				case '[':
					TextWindow.AdvanceChar();
					info.Kind = SyntaxKind.OpenBracketToken;
					break;

				case ']':
					TextWindow.AdvanceChar();
					info.Kind = SyntaxKind.CloseBracketToken;
					break;

				case '?':
					TextWindow.AdvanceChar();
					if (TextWindow.PeekChar() == '?')
					{
						TextWindow.AdvanceChar();

						if (TextWindow.PeekChar() == '=')
						{
							TextWindow.AdvanceChar();
							info.Kind = SyntaxKind.QuestionQuestionEqualsToken;
						}
						else
						{
							info.Kind = SyntaxKind.QuestionQuestionToken;
						}
					}
					else
					{
						info.Kind = SyntaxKind.QuestionToken;
					}

					break;

				case '+':
					TextWindow.AdvanceChar();
					if ((character = TextWindow.PeekChar()) == '=')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.PlusEqualsToken;
					}
					else if (character == '+')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.PlusPlusToken;
					}
					else
					{
						info.Kind = SyntaxKind.PlusToken;
					}

					break;

				case '-':
					TextWindow.AdvanceChar();
					if ((character = TextWindow.PeekChar()) == '=')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.MinusEqualsToken;
					}
					else if (character == '-')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.MinusMinusToken;
					}
					else if (character == '>')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.MinusGreaterThanToken;
					}
					else
					{
						info.Kind = SyntaxKind.MinusToken;
					}

					break;

				case '%':
					TextWindow.AdvanceChar();
					if (TextWindow.PeekChar() == '=')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.PercentEqualsToken;
					}
					else
					{
						info.Kind = SyntaxKind.PercentToken;
					}

					break;

				case '&':
					TextWindow.AdvanceChar();
					if ((character = TextWindow.PeekChar()) == '=')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.AmpersandEqualsToken;
					}
					else if (TextWindow.PeekChar() == '&')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.AmpersandAmpersandToken;
					}
					else
					{
						info.Kind = SyntaxKind.AmpersandToken;
					}

					break;

				case '^':
					TextWindow.AdvanceChar();
					if (TextWindow.PeekChar() == '=')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.CaretEqualsToken;
					}
					else
					{
						info.Kind = SyntaxKind.CaretToken;
					}

					break;

				case '|':
					TextWindow.AdvanceChar();
					if (TextWindow.PeekChar() == '=')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.BarEqualsToken;
					}
					else if (TextWindow.PeekChar() == '|')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.BarBarToken;
					}
					else
					{
						info.Kind = SyntaxKind.BarToken;
					}

					break;

				case '<':
					TextWindow.AdvanceChar();
					if (TextWindow.PeekChar() == '=')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.LessThanEqualsToken;
					}
					else if (TextWindow.PeekChar() == '<')
					{
						TextWindow.AdvanceChar();
						if (TextWindow.PeekChar() == '=')
						{
							TextWindow.AdvanceChar();
							info.Kind = SyntaxKind.LessThanLessThanEqualsToken;
						}
						else
						{
							info.Kind = SyntaxKind.LessThanLessThanToken;
						}
					}
					else
					{
						info.Kind = SyntaxKind.LessThanToken;
					}

					break;

				case '>':
					TextWindow.AdvanceChar();
					if (TextWindow.PeekChar() == '=')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.GreaterThanEqualsToken;
					}
					else
					{
						info.Kind = SyntaxKind.GreaterThanToken;
					}

					break;

				case '@':
					if (TextWindow.PeekChar(1) == '"')
					{
						this.ScanVerbatimStringLiteral(ref info);
					}
					else if (!this.ScanIdentifierOrKeyword(ref info))
					{
						TextWindow.AdvanceChar();
						info.Text = TextWindow.GetText(intern: true);
						this.AddError(ErrorCode.ERR_ExpectedVerbatimLiteral);
					}

					break;
				// All the 'common' identifier characters are represented directly in
				// these switch cases for optimal perf.  Calling IsIdentifierChar() functions is relatively
				// expensive.
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
					this.ScanIdentifierOrKeyword(ref info);
					break;

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
					this.ScanNumericLiteral(ref info);
					break;

				case '\\':
					{
						// Could be unicode escape. Try that.
						character = TextWindow.PeekCharOrUnicodeEscape(out surrogateCharacter);

						isEscaped = true;
						if (SyntaxFacts.IsIdentifierStartCharacter(character))
						{
							goto case 'a';
						}

						goto default;
					}

				case SlidingTextWindow.InvalidCharacter:
					if (!TextWindow.IsReallyAtEnd())
					{
						goto default;
					}

					if (_directives.HasUnfinishedIf())
					{
						this.AddError(ErrorCode.ERR_EndifDirectiveExpected);
					}

					if (_directives.HasUnfinishedRegion())
					{
						this.AddError(ErrorCode.ERR_EndRegionDirectiveExpected);
					}

					info.Kind = SyntaxKind.EndOfFileToken;
					break;

				default:
					if (SyntaxFacts.IsIdentifierStartCharacter(character))
					{
						goto case 'a';
					}

					if (isEscaped)
					{
						SyntaxDiagnosticInfo error;
						TextWindow.NextCharOrUnicodeEscape(out surrogateCharacter, out error);
						AddError(error);
					}
					else
					{
						TextWindow.AdvanceChar();
					}

					if (_badTokenCount++ > 200)
					{
						// If we get too many characters that we cannot make sense of, absorb the rest of the input.
						int end = TextWindow.Text.Length;
						int width = end - startingPosition;
						info.Text = TextWindow.Text.ToString(new TextSpan(startingPosition, width));
						TextWindow.Reset(end);
					}
					else
					{
						info.Text = TextWindow.GetText(intern: true);
					}

					this.AddError(ErrorCode.ERR_UnexpectedCharacter, info.Text);
					break;
			}
		}

		private bool ScanInteger()
		{
			int start = TextWindow.Position;
			char ch;
			while ((ch = TextWindow.PeekChar()) >= '0' && ch <= '9')
			{
				TextWindow.AdvanceChar();
			}

			return start < TextWindow.Position;
		}

		private void ScanNumericLiteralSingleInteger(ref bool underscoreInWrongPlace, ref bool usedUnderscore, ref bool firstCharWasUnderscore, bool isHex, bool isBinary)
		{
			if (TextWindow.PeekChar() == '_')
			{
				if (isHex || isBinary)
				{
					firstCharWasUnderscore = true;
				}
				else
				{
					underscoreInWrongPlace = true;
				}
			}

			bool lastCharWasUnderscore = false;
			while (true)
			{
				char ch = TextWindow.PeekChar();
				if (ch == '_')
				{
					usedUnderscore = true;
					lastCharWasUnderscore = true;
				}
				else if (!(isHex ? SyntaxFacts.IsHexDigit(ch) :
						   isBinary ? SyntaxFacts.IsBinaryDigit(ch) :
						   SyntaxFacts.IsDecDigit(ch)))
				{
					break;
				}
				else
				{
					_builder.Append(ch);
					lastCharWasUnderscore = false;
				}
				TextWindow.AdvanceChar();
			}

			if (lastCharWasUnderscore)
			{
				underscoreInWrongPlace = true;
			}
		}

		private bool ScanNumericLiteral(ref TokenInfo info)
		{
			int start = TextWindow.Position;
			char ch;
			bool isHex = false;
			bool isBinary = false;
			bool hasDecimal = false;
			bool hasExponent = false;
			info.Text = null;
			info.ValueKind = SpecialType.None;
			_builder.Clear();
			bool hasUSuffix = false;
			bool hasLSuffix = false;
			bool underscoreInWrongPlace = false;
			bool usedUnderscore = false;
			bool firstCharWasUnderscore = false;

			ch = TextWindow.PeekChar();
			if (ch == '0')
			{
				ch = TextWindow.PeekChar(1);
				if (ch == 'x' || ch == 'X')
				{
					TextWindow.AdvanceChar(2);
					isHex = true;
				}
				else if (ch == 'b' || ch == 'B')
				{
					TextWindow.AdvanceChar(2);
					isBinary = true;
				}
			}

			if (isHex || isBinary)
			{
				// It's OK if it has no digits after the '0x' -- we'll catch it in ScanNumericLiteral
				// and give a proper error then.
				ScanNumericLiteralSingleInteger(ref underscoreInWrongPlace, ref usedUnderscore, ref firstCharWasUnderscore, isHex, isBinary);

				if ((ch = TextWindow.PeekChar()) == 'L' || ch == 'l')
				{
					if (ch == 'l')
					{
						this.AddError(TextWindow.Position, 1, ErrorCode.WRN_LowercaseEllSuffix);
					}

					TextWindow.AdvanceChar();
					hasLSuffix = true;
					if ((ch = TextWindow.PeekChar()) == 'u' || ch == 'U')
					{
						TextWindow.AdvanceChar();
						hasUSuffix = true;
					}
				}
				else if ((ch = TextWindow.PeekChar()) == 'u' || ch == 'U')
				{
					TextWindow.AdvanceChar();
					hasUSuffix = true;
					if ((ch = TextWindow.PeekChar()) == 'L' || ch == 'l')
					{
						TextWindow.AdvanceChar();
						hasLSuffix = true;
					}
				}
			}
			else
			{
				ScanNumericLiteralSingleInteger(ref underscoreInWrongPlace, ref usedUnderscore, ref firstCharWasUnderscore, isHex: false, isBinary: false);

				if (this.ModeIs(LexerMode.DebuggerSyntax) && TextWindow.PeekChar() == '#')
				{
					// Previously, in DebuggerSyntax mode, "123#" was a valid identifier.
					TextWindow.AdvanceChar();
					info.StringValue = info.Text = TextWindow.GetText(intern: true);
					info.Kind = SyntaxKind.IdentifierToken;
					this.AddError(MakeError(ErrorCode.ERR_LegacyObjectIdSyntax));
					return true;
				}

				if ((ch = TextWindow.PeekChar()) == '.')
				{
					var ch2 = TextWindow.PeekChar(1);
					if (ch2 >= '0' && ch2 <= '9')
					{
						hasDecimal = true;
						_builder.Append(ch);
						TextWindow.AdvanceChar();

						ScanNumericLiteralSingleInteger(ref underscoreInWrongPlace, ref usedUnderscore, ref firstCharWasUnderscore, isHex: false, isBinary: false);
					}
					else if (_builder.Length == 0)
					{
						// we only have the dot so far.. (no preceding number or following number)
						TextWindow.Reset(start);
						return false;
					}
				}

				if ((ch = TextWindow.PeekChar()) == 'E' || ch == 'e')
				{
					_builder.Append(ch);
					TextWindow.AdvanceChar();
					hasExponent = true;
					if ((ch = TextWindow.PeekChar()) == '-' || ch == '+')
					{
						_builder.Append(ch);
						TextWindow.AdvanceChar();
					}

					if (!(((ch = TextWindow.PeekChar()) >= '0' && ch <= '9') || ch == '_'))
					{
						// use this for now (CS0595), cant use CS0594 as we dont know 'type'
						this.AddError(MakeError(ErrorCode.ERR_InvalidReal));
						// add dummy exponent, so parser does not blow up
						_builder.Append('0');
					}
					else
					{
						ScanNumericLiteralSingleInteger(ref underscoreInWrongPlace, ref usedUnderscore, ref firstCharWasUnderscore, isHex: false, isBinary: false);
					}
				}

				if (hasExponent || hasDecimal)
				{
					if ((ch = TextWindow.PeekChar()) == 'f' || ch == 'F')
					{
						TextWindow.AdvanceChar();
						info.ValueKind = SpecialType.System_Single;
					}
					else if (ch == 'D' || ch == 'd')
					{
						TextWindow.AdvanceChar();
						info.ValueKind = SpecialType.System_Double;
					}
					else if (ch == 'm' || ch == 'M')
					{
						TextWindow.AdvanceChar();
						info.ValueKind = SpecialType.System_Decimal;
					}
					else
					{
						info.ValueKind = SpecialType.System_Double;
					}
				}
				else if ((ch = TextWindow.PeekChar()) == 'f' || ch == 'F')
				{
					TextWindow.AdvanceChar();
					info.ValueKind = SpecialType.System_Single;
				}
				else if (ch == 'D' || ch == 'd')
				{
					TextWindow.AdvanceChar();
					info.ValueKind = SpecialType.System_Double;
				}
				else if (ch == 'm' || ch == 'M')
				{
					TextWindow.AdvanceChar();
					info.ValueKind = SpecialType.System_Decimal;
				}
				else if (ch == 'L' || ch == 'l')
				{
					if (ch == 'l')
					{
						this.AddError(TextWindow.Position, 1, ErrorCode.WRN_LowercaseEllSuffix);
					}

					TextWindow.AdvanceChar();
					hasLSuffix = true;
					if ((ch = TextWindow.PeekChar()) == 'u' || ch == 'U')
					{
						TextWindow.AdvanceChar();
						hasUSuffix = true;
					}
				}
				else if (ch == 'u' || ch == 'U')
				{
					hasUSuffix = true;
					TextWindow.AdvanceChar();
					if ((ch = TextWindow.PeekChar()) == 'L' || ch == 'l')
					{
						TextWindow.AdvanceChar();
						hasLSuffix = true;
					}
				}
			}

			if (underscoreInWrongPlace)
			{
				this.AddError(MakeError(start, TextWindow.Position - start, ErrorCode.ERR_InvalidNumber));
			}

			info.Kind = SyntaxKind.NumericLiteralToken;
			info.Text = TextWindow.GetText(true);
			Debug.Assert(info.Text != null);
			var valueText = TextWindow.Intern(_builder);
			ulong val;
			switch (info.ValueKind)
			{
				case SpecialType.System_Single:
					info.FloatValue = this.GetValueSingle(valueText);
					break;
				case SpecialType.System_Double:
					info.DoubleValue = this.GetValueDouble(valueText);
					break;
				case SpecialType.System_Decimal:
					info.DecimalValue = this.GetValueDecimal(valueText, start, TextWindow.Position);
					break;
				default:
					if (string.IsNullOrEmpty(valueText))
					{
						if (!underscoreInWrongPlace)
						{
							this.AddError(MakeError(ErrorCode.ERR_InvalidNumber));
						}
						val = 0; //safe default
					}
					else
					{
						val = this.GetValueUInt64(valueText, isHex, isBinary);
					}

					// 2.4.4.2 Integer literals
					// ...
					// The type of an integer literal is determined as follows:

					// * If the literal has no suffix, it has the first of these types in which its value can be represented: int, uint, long, ulong.
					if (!hasUSuffix && !hasLSuffix)
					{
						if (val <= Int32.MaxValue)
						{
							info.ValueKind = SpecialType.System_Int32;
							info.IntValue = (int)val;
						}
						else if (val <= UInt32.MaxValue)
						{
							info.ValueKind = SpecialType.System_UInt32;
							info.UintValue = (uint)val;

							// TODO: See below, it may be desirable to mark this token
							// as special for folding if its value is 2147483648.
						}
						else if (val <= Int64.MaxValue)
						{
							info.ValueKind = SpecialType.System_Int64;
							info.LongValue = (long)val;
						}
						else
						{
							info.ValueKind = SpecialType.System_UInt64;
							info.UlongValue = val;

							// TODO: See below, it may be desirable to mark this token
							// as special for folding if its value is 9223372036854775808
						}
					}
					else if (hasUSuffix && !hasLSuffix)
					{
						// * If the literal is suffixed by U or u, it has the first of these types in which its value can be represented: uint, ulong.
						if (val <= UInt32.MaxValue)
						{
							info.ValueKind = SpecialType.System_UInt32;
							info.UintValue = (uint)val;
						}
						else
						{
							info.ValueKind = SpecialType.System_UInt64;
							info.UlongValue = val;
						}
					}

					// * If the literal is suffixed by L or l, it has the first of these types in which its value can be represented: long, ulong.
					else if (!hasUSuffix & hasLSuffix)
					{
						if (val <= Int64.MaxValue)
						{
							info.ValueKind = SpecialType.System_Int64;
							info.LongValue = (long)val;
						}
						else
						{
							info.ValueKind = SpecialType.System_UInt64;
							info.UlongValue = val;

							// TODO: See below, it may be desirable to mark this token
							// as special for folding if its value is 9223372036854775808
						}
					}

					// * If the literal is suffixed by UL, Ul, uL, ul, LU, Lu, lU, or lu, it is of type ulong.
					else
					{
						Debug.Assert(hasUSuffix && hasLSuffix);
						info.ValueKind = SpecialType.System_UInt64;
						info.UlongValue = val;
					}

					break;

					// Note, the following portion of the spec is not implemented here. It is implemented
					// in the unary minus analysis.

					// * When a decimal-integer-literal with the value 2147483648 (231) and no integer-type-suffix appears
					//   as the token immediately following a unary minus operator token (§7.7.2), the result is a constant
					//   of type int with the value −2147483648 (−231). In all other situations, such a decimal-integer-
					//   literal is of type uint.
					// * When a decimal-integer-literal with the value 9223372036854775808 (263) and no integer-type-suffix
					//   or the integer-type-suffix L or l appears as the token immediately following a unary minus operator
					//   token (§7.7.2), the result is a constant of type long with the value −9223372036854775808 (−263).
					//   In all other situations, such a decimal-integer-literal is of type ulong.
			}

			return true;
		}

		private static bool TryParseBinaryUInt64(string text, out ulong value)
		{
			value = 0;
			foreach (char c in text)
			{
				// if uppermost bit is set, then the next bitshift will overflow
				if ((value & 0x8000000000000000) != 0)
				{
					return false;
				}
				// We shouldn't ever get a string that's nonbinary (see ScanNumericLiteral),
				// so don't explicitly check for it (there's a debug assert in SyntaxFacts)
				var bit = (ulong)SyntaxFacts.BinaryValue(c);
				value = (value << 1) | bit;
			}
			return true;
		}

		private int GetValueInt32(string text, bool isHex)
		{
			int result;
			if (!Int32.TryParse(text, isHex ? NumberStyles.AllowHexSpecifier : NumberStyles.None, CultureInfo.InvariantCulture, out result))
			{
				//we've already lexed the literal, so the error must be from overflow
				this.AddError(MakeError(ErrorCode.ERR_IntOverflow));
			}

			return result;
		}

		private ulong GetValueUInt64(string text, bool isHex, bool isBinary)
		{
			ulong result;
			if (isBinary)
			{
				if (!TryParseBinaryUInt64(text, out result))
				{
					this.AddError(MakeError(ErrorCode.ERR_IntOverflow));
				}
			}
			else if (!UInt64.TryParse(text, isHex ? NumberStyles.AllowHexSpecifier : NumberStyles.None, CultureInfo.InvariantCulture, out result))
			{
				//we've already lexed the literal, so the error must be from overflow
				this.AddError(MakeError(ErrorCode.ERR_IntOverflow));
			}

			return result;
		}

		private double GetValueDouble(string text)
		{
			double result;
			if (!double.TryParse(text, out result))
			{
				//we've already lexed the literal, so the error must be from overflow
				this.AddError(MakeError(ErrorCode.ERR_FloatOverflow, "double"));
			}

			return result;
		}

		private float GetValueSingle(string text)
		{
			float result;
			if (!float.TryParse(text, out result))
			{
				//we've already lexed the literal, so the error must be from overflow
				this.AddError(MakeError(ErrorCode.ERR_FloatOverflow, "float"));
			}

			return result;
		}

		private decimal GetValueDecimal(string text, int start, int end)
		{
			decimal result;
			if (!decimal.TryParse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out result))
			{
				//we've already lexed the literal, so the error must be from overflow
				this.AddError(this.MakeError(start, end - start, ErrorCode.ERR_FloatOverflow, "decimal"));
			}

			return result;
		}

		private void ResetIdentBuffer()
		{
			_identLen = 0;
		}

		private void AddIdentChar(char ch)
		{
			if (_identLen >= _identBuffer.Length)
			{
				this.GrowIdentBuffer();
			}

			_identBuffer[_identLen++] = ch;
		}

		private void GrowIdentBuffer()
		{
			var tmp = new char[_identBuffer.Length * 2];
			Array.Copy(_identBuffer, tmp, _identBuffer.Length);
			_identBuffer = tmp;
		}

		private bool ScanIdentifier(ref TokenInfo info)
		{
			int start = TextWindow.Position;
			this.ResetIdentBuffer();

			info.IsVerbatim = TextWindow.PeekChar() == '@';
			if (info.IsVerbatim)
			{
				TextWindow.AdvanceChar();
			}

			bool isObjectAddress = false;

			while (true)
			{
				char surrogateCharacter = SlidingTextWindow.InvalidCharacter;
				bool isEscaped = false;
				char ch = TextWindow.PeekChar();
				top:
				switch (ch)
				{
					case '\\':
						if (!isEscaped && TextWindow.IsUnicodeEscape())
						{
							// ^^^^^^^ otherwise \u005Cu1234 looks just like \u1234! (i.e. escape within escape)
							info.HasIdentifierEscapeSequence = true;
							isEscaped = true;
							ch = TextWindow.PeekUnicodeEscape(out surrogateCharacter);
							goto top;
						}

						goto default;
					case '$':
						if (!this.ModeIs(LexerMode.DebuggerSyntax) || _identLen > 0)
						{
							goto LoopExit;
						}

						break;
					case SlidingTextWindow.InvalidCharacter:
						if (!TextWindow.IsReallyAtEnd())
						{
							goto default;
						}

						goto LoopExit;
					case '_':
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
						{
							// Again, these are the 'common' identifier characters...
							break;
						}

					case '0':
						{
							if (_identLen == 0)
							{
								// Debugger syntax allows @0x[hexdigit]+ for object address identifiers.
								if (info.IsVerbatim &&
									this.ModeIs(LexerMode.DebuggerSyntax) &&
									(char.ToLower(TextWindow.PeekChar(1)) == 'x'))
								{
									isObjectAddress = true;
								}
								else
								{
									goto LoopExit;
								}
							}

							// Again, these are the 'common' identifier characters...
							break;
						}
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
						{
							if (_identLen == 0)
							{
								goto LoopExit;
							}

							// Again, these are the 'common' identifier characters...
							break;
						}

					case ' ':
					case '\t':
					case '.':
					case ';':
					case '(':
					case ')':
					case ',':
						// ...and these are the 'common' stop characters.
						goto LoopExit;
					case '<':
						if (_identLen == 0 && this.ModeIs(LexerMode.DebuggerSyntax) && TextWindow.PeekChar(1) == '>')
						{
							// In DebuggerSyntax mode, identifiers are allowed to begin with <>.
							TextWindow.AdvanceChar(2);
							this.AddIdentChar('<');
							this.AddIdentChar('>');
							continue;
						}

						goto LoopExit;
					default:
						{
							// This is the 'expensive' call
							if (_identLen == 0 && ch > 127 && SyntaxFacts.IsIdentifierStartCharacter(ch))
							{
								break;
							}
							else if (_identLen > 0 && ch > 127 && SyntaxFacts.IsIdentifierPartCharacter(ch))
							{
								break;
							}
							else
							{
								// Not a valid identifier character, so bail.
								goto LoopExit;
							}
						}
				}

				if (isEscaped)
				{
					SyntaxDiagnosticInfo error;
					TextWindow.NextCharOrUnicodeEscape(out surrogateCharacter, out error);
					AddError(error);
				}
				else
				{
					TextWindow.AdvanceChar();
				}

				this.AddIdentChar(ch);
				if (surrogateCharacter != SlidingTextWindow.InvalidCharacter)
				{
					this.AddIdentChar(surrogateCharacter);
				}
			}

			LoopExit:
			var width = TextWindow.Width; // exact size of input characters
			if (_identLen > 0)
			{
				info.Text = TextWindow.GetInternedText();

				// id buffer is identical to width in input
				if (_identLen == width)
				{
					info.StringValue = info.Text;
				}
				else
				{
					info.StringValue = TextWindow.Intern(_identBuffer, 0, _identLen);
				}

				if (isObjectAddress)
				{
					// @0x[hexdigit]+
					const int objectAddressOffset = 2;
					Debug.Assert(string.Equals(info.Text.Substring(0, objectAddressOffset + 1), "@0x", StringComparison.OrdinalIgnoreCase));
					var valueText = TextWindow.Intern(_identBuffer, objectAddressOffset, _identLen - objectAddressOffset);
					// Verify valid hex value.
					if ((valueText.Length == 0) || !valueText.All(IsValidHexDigit))
					{
						goto Fail;
					}
					// Parse hex value to check for overflow.
					this.GetValueUInt64(valueText, isHex: true, isBinary: false);
				}

				return true;
			}

			Fail:
			info.Text = null;
			info.StringValue = null;
			TextWindow.Reset(start);
			return false;
		}

		private static bool IsValidHexDigit(char c)
		{
			if ((c >= '0') && (c <= '9'))
			{
				return true;
			}
			c = char.ToLower(c);
			return (c >= 'a') && (c <= 'f');
		}

		private bool ScanIdentifierOrKeyword(ref TokenInfo info)
		{
			info.ContextualKind = SyntaxKind.None;

			if (this.ScanIdentifier(ref info))
			{
				// check to see if it is an actual keyword
				if (!info.IsVerbatim && !info.HasIdentifierEscapeSequence)
				{
					if (this.ModeIs(LexerMode.Directive))
					{
						SyntaxKind keywordKind = SyntaxFacts.GetPreprocessorKeywordKind(info.Text);
						if (SyntaxFacts.IsPreprocessorContextualKeyword(keywordKind))
						{
							// Let the parser decide which instances are actually keywords.
							info.Kind = SyntaxKind.IdentifierToken;
							info.ContextualKind = keywordKind;
						}
						else
						{
							info.Kind = keywordKind;
						}
					}
					else
					{
						if (!_cache.TryGetKeywordKind(info.Text, out info.Kind))
						{
							info.ContextualKind = info.Kind = SyntaxKind.IdentifierToken;
						}
						else if (SyntaxFacts.IsContextualKeyword(info.Kind))
						{
							info.ContextualKind = info.Kind;
							info.Kind = SyntaxKind.IdentifierToken;
						}
					}

					if (info.Kind == SyntaxKind.None)
					{
						info.Kind = SyntaxKind.IdentifierToken;
					}
				}
				else
				{
					info.ContextualKind = info.Kind = SyntaxKind.IdentifierToken;
				}

				return true;
			}
			else
			{
				info.Kind = SyntaxKind.None;
				return false;
			}
		}

		private void LexSyntaxTrivia(bool afterFirstToken, bool isTrailing, ref SyntaxListBuilder triviaList)
		{
			bool onlyWhitespaceOnLine = !isTrailing;

			while (true)
			{
				this.Start();
				char ch = TextWindow.PeekChar();
				if (ch == ' ')
				{
					this.AddTrivia(this.ScanWhitespace(), ref triviaList);
					continue;
				}
				else if (ch > 127)
				{
					if (SyntaxFacts.IsWhitespace(ch))
					{
						ch = ' ';
					}
					else if (SyntaxFacts.IsNewLine(ch))
					{
						ch = '\n';
					}
				}

				switch (ch)
				{
					case ' ':
					case '\t':       // Horizontal tab
					case '\v':       // Vertical Tab
					case '\f':       // Form-feed
					case '\u001A':
						this.AddTrivia(this.ScanWhitespace(), ref triviaList);
						break;
					case '/':
						if ((ch = TextWindow.PeekChar(1)) == '/')
						{
							// normal single line comment
							this.ScanToEndOfLine();
							var text = TextWindow.GetText(false);
							this.AddTrivia(SyntaxFactory.Comment(text), ref triviaList);
							onlyWhitespaceOnLine = false;
							break;
						}
						else if (ch == '*')
						{
							bool isTerminated;
							this.ScanMultiLineComment(out isTerminated);
							if (!isTerminated)
							{
								// The comment didn't end.  Report an error at the start point.
								this.AddError(ErrorCode.ERR_OpenEndedComment);
							}

							var text = TextWindow.GetText(false);
							this.AddTrivia(SyntaxFactory.Comment(text), ref triviaList);
							onlyWhitespaceOnLine = false;
							break;
						}

						// not trivia
						return;
					case '\r':
					case '\n':
						this.AddTrivia(this.ScanEndOfLine(), ref triviaList);
						if (isTrailing)
						{
							return;
						}

						onlyWhitespaceOnLine = true;
						break;
					case '#':
						if (_allowPreprocessorDirectives)
						{
							this.LexDirectiveAndExcludedTrivia(afterFirstToken, isTrailing || !onlyWhitespaceOnLine, ref triviaList);
							break;
						}
						else
						{
							return;
						}

					case '=':
					case '<':
						return;

					default:
						return;
				}
			}
		}
		private void AddTrivia(LeeSyntaxNode trivia, ref SyntaxListBuilder list)
		{
			if (this.HasErrors)
			{
				trivia = trivia.WithDiagnosticsGreen(this.GetErrors(leadingTriviaWidth: 0));
			}

			if (list == null)
			{
				list = new SyntaxListBuilder(TriviaListInitialCapacity);
			}

			list.Add(trivia);
		}

		private bool ScanMultiLineComment(out bool isTerminated)
		{
			if (TextWindow.PeekChar() == '/' && TextWindow.PeekChar(1) == '*')
			{
				TextWindow.AdvanceChar(2);

				char ch;
				while (true)
				{
					if ((ch = TextWindow.PeekChar()) == SlidingTextWindow.InvalidCharacter && TextWindow.IsReallyAtEnd())
					{
						isTerminated = false;
						break;
					}
					else if (ch == '*' && TextWindow.PeekChar(1) == '/')
					{
						TextWindow.AdvanceChar(2);
						isTerminated = true;
						break;
					}
					else
					{
						TextWindow.AdvanceChar();
					}
				}

				return true;
			}
			else
			{
				isTerminated = false;
				return false;
			}
		}

		private void ScanToEndOfLine()
		{
			char ch;
			while (!SyntaxFacts.IsNewLine(ch = TextWindow.PeekChar()) &&
				(ch != SlidingTextWindow.InvalidCharacter || !TextWindow.IsReallyAtEnd()))
			{
				TextWindow.AdvanceChar();
			}
		}

		private LeeSyntaxNode ScanEndOfLine()
		{
			char ch;
			switch (ch = TextWindow.PeekChar())
			{
				case '\r':
					TextWindow.AdvanceChar();
					if (TextWindow.PeekChar() == '\n')
					{
						TextWindow.AdvanceChar();
						return SyntaxFactory.CarriageReturnLineFeed;
					}

					return SyntaxFactory.CarriageReturn;
				case '\n':
					TextWindow.AdvanceChar();
					return SyntaxFactory.LineFeed;
				default:
					if (SyntaxFacts.IsNewLine(ch))
					{
						TextWindow.AdvanceChar();
						return SyntaxFactory.EndOfLine(ch.ToString());
					}

					return null;
			}
		}

		private SyntaxTrivia ScanWhitespace()
		{
			bool onlySpaces = true;

			top:
			char ch = TextWindow.PeekChar();

			switch (ch)
			{
				case '\t':       // Horizontal tab
				case '\v':       // Vertical Tab
				case '\f':       // Form-feed
				case '\u001A':
					onlySpaces = false;
					goto case ' ';

				case ' ':
					TextWindow.AdvanceChar();
					goto top;

				case '\r':      // Carriage Return
				case '\n':      // Line-feed
					break;

				default:
					if (ch > 127 && SyntaxFacts.IsWhitespace(ch))
					{
						goto case '\t';
					}

					break;
			}

			if (TextWindow.Width == 1 && onlySpaces)
			{
				return SyntaxFactory.Space;
			}
			else
			{
				return SyntaxFactory.Whitespace(TextWindow.GetText(intern: true));
			}
		}

		private void LexDirectiveAndExcludedTrivia(
			bool afterFirstToken,
			bool afterNonWhitespaceOnLine,
			ref SyntaxListBuilder triviaList)
		{
			var directive = this.LexSingleDirective(true, true, afterFirstToken, afterNonWhitespaceOnLine, ref triviaList);

			// also lex excluded stuff            
			var branching = directive as BranchingDirectiveTriviaSyntax;
			if (branching != null && !branching.BranchTaken)
			{
				this.LexExcludedDirectivesAndTrivia(true, ref triviaList);
			}
		}

		private void LexExcludedDirectivesAndTrivia(bool endIsActive, ref SyntaxListBuilder triviaList)
		{
			while (true)
			{
				bool hasFollowingDirective;
				var text = this.LexDisabledText(out hasFollowingDirective);
				if (text != null)
				{
					this.AddTrivia(text, ref triviaList);
				}

				if (!hasFollowingDirective)
				{
					break;
				}

				var directive = this.LexSingleDirective(false, endIsActive, false, false, ref triviaList);
				var branching = directive as BranchingDirectiveTriviaSyntax;
				if (directive.Kind == SyntaxKind.EndIfDirectiveTrivia || (branching != null && branching.BranchTaken))
				{
					break;
				}
				else if (directive.Kind == SyntaxKind.IfDirectiveTrivia)
				{
					this.LexExcludedDirectivesAndTrivia(false, ref triviaList);
				}
			}
		}

		private LeeSyntaxNode LexSingleDirective(
			bool isActive,
			bool endIsActive,
			bool afterFirstToken,
			bool afterNonWhitespaceOnLine,
			ref SyntaxListBuilder triviaList)
		{
			if (SyntaxFacts.IsWhitespace(TextWindow.PeekChar()))
			{
				this.Start();
				this.AddTrivia(this.ScanWhitespace(), ref triviaList);
			}

			LeeSyntaxNode directive;
			var saveMode = _mode;

			using (var dp = new DirectiveParser(this, _directives))
			{
				directive = dp.ParseDirective(isActive, endIsActive, afterFirstToken, afterNonWhitespaceOnLine);
			}

			this.AddTrivia(directive, ref triviaList);
			_directives = directive.ApplyDirectives(_directives);
			_mode = saveMode;
			return directive;
		}

		private LeeSyntaxNode LexDisabledText(out bool followedByDirective)
		{
			this.Start();

			int lastLineStart = TextWindow.Position;
			int lines = 0;
			bool allWhitespace = true;

			while (true)
			{
				char ch = TextWindow.PeekChar();
				switch (ch)
				{
					case SlidingTextWindow.InvalidCharacter:
						if (!TextWindow.IsReallyAtEnd())
						{
							goto default;
						}

						followedByDirective = false;
						return TextWindow.Width > 0 ? SyntaxFactory.DisabledText(TextWindow.GetText(false)) : null;
					case '#':
						if (!_allowPreprocessorDirectives) goto default;
						followedByDirective = true;
						if (lastLineStart < TextWindow.Position && !allWhitespace)
						{
							goto default;
						}

						TextWindow.Reset(lastLineStart);  // reset so directive parser can consume the starting whitespace on this line
						return TextWindow.Width > 0 ? SyntaxFactory.DisabledText(TextWindow.GetText(false)) : null;
					case '\r':
					case '\n':
						this.ScanEndOfLine();
						lastLineStart = TextWindow.Position;
						allWhitespace = true;
						lines++;
						break;
					default:
						if (SyntaxFacts.IsNewLine(ch))
						{
							goto case '\n';
						}

						allWhitespace = allWhitespace && SyntaxFacts.IsWhitespace(ch);
						TextWindow.AdvanceChar();
						break;
				}
			}
		}

		private SyntaxToken LexDirectiveToken()
		{
			this.Start();
			TokenInfo info = default(TokenInfo);
			this.ScanDirectiveToken(ref info);
			var errors = this.GetErrors(leadingTriviaWidth: 0);
			var trailing = this.LexDirectiveTrailingTrivia(info.Kind == SyntaxKind.EndOfDirectiveToken);
			return Create(ref info, null, trailing, errors);
		}

		private bool ScanDirectiveToken(ref TokenInfo info)
		{
			char character;
			char surrogateCharacter;
			bool isEscaped = false;

			switch (character = TextWindow.PeekChar())
			{
				case SlidingTextWindow.InvalidCharacter:
					if (!TextWindow.IsReallyAtEnd())
					{
						goto default;
					}
					// don't consume end characters here
					info.Kind = SyntaxKind.EndOfDirectiveToken;
					break;

				case '\r':
				case '\n':
					// don't consume end characters here
					info.Kind = SyntaxKind.EndOfDirectiveToken;
					break;

				case '#':
					TextWindow.AdvanceChar();
					info.Kind = SyntaxKind.HashToken;
					break;

				case '(':
					TextWindow.AdvanceChar();
					info.Kind = SyntaxKind.OpenParenToken;
					break;

				case ')':
					TextWindow.AdvanceChar();
					info.Kind = SyntaxKind.CloseParenToken;
					break;

				case ',':
					TextWindow.AdvanceChar();
					info.Kind = SyntaxKind.CommaToken;
					break;

				case '!':
					TextWindow.AdvanceChar();
					if (TextWindow.PeekChar() == '=')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.ExclamationEqualsToken;
					}
					else
					{
						info.Kind = SyntaxKind.ExclamationToken;
					}

					break;

				case '=':
					TextWindow.AdvanceChar();
					if (TextWindow.PeekChar() == '=')
					{
						TextWindow.AdvanceChar();
						info.Kind = SyntaxKind.EqualsEqualsToken;
					}
					else
					{
						info.Kind = SyntaxKind.EqualsToken;
					}

					break;

				case '&':
					if (TextWindow.PeekChar(1) == '&')
					{
						TextWindow.AdvanceChar(2);
						info.Kind = SyntaxKind.AmpersandAmpersandToken;
						break;
					}

					goto default;

				case '|':
					if (TextWindow.PeekChar(1) == '|')
					{
						TextWindow.AdvanceChar(2);
						info.Kind = SyntaxKind.BarBarToken;
						break;
					}

					goto default;

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
					this.ScanInteger();
					info.Kind = SyntaxKind.NumericLiteralToken;
					info.Text = TextWindow.GetText(true);
					info.ValueKind = SpecialType.System_Int32;
					info.IntValue = this.GetValueInt32(info.Text, false);
					break;

				case '\"':
					this.ScanStringLiteral(ref info, false);
					break;

				case '\\':
					{
						// Could be unicode escape. Try that.
						character = TextWindow.PeekCharOrUnicodeEscape(out surrogateCharacter);
						isEscaped = true;
						if (SyntaxFacts.IsIdentifierStartCharacter(character))
						{
							this.ScanIdentifierOrKeyword(ref info);
							break;
						}

						goto default;
					}

				default:
					if (!isEscaped && SyntaxFacts.IsNewLine(character))
					{
						goto case '\n';
					}

					if (SyntaxFacts.IsIdentifierStartCharacter(character))
					{
						this.ScanIdentifierOrKeyword(ref info);
					}
					else
					{
						// unknown single character
						if (isEscaped)
						{
							SyntaxDiagnosticInfo error;
							TextWindow.NextCharOrUnicodeEscape(out surrogateCharacter, out error);
							AddError(error);
						}
						else
						{
							TextWindow.AdvanceChar();
						}

						info.Kind = SyntaxKind.None;
						info.Text = TextWindow.GetText(true);
					}

					break;
			}

			Debug.Assert(info.Kind != SyntaxKind.None || info.Text != null);
			return info.Kind != SyntaxKind.None;
		}

		private SyntaxListBuilder LexDirectiveTrailingTrivia(bool includeEndOfLine)
		{
			SyntaxListBuilder trivia = null;

			LeeSyntaxNode tr;
			while (true)
			{
				var pos = TextWindow.Position;
				tr = this.LexDirectiveTrivia();
				if (tr == null)
				{
					break;
				}
				else if (tr.Kind == SyntaxKind.EndOfLineTrivia)
				{
					if (includeEndOfLine)
					{
						AddTrivia(tr, ref trivia);
					}
					else
					{
						// don't consume end of line...
						TextWindow.Reset(pos);
					}

					break;
				}
				else
				{
					AddTrivia(tr, ref trivia);
				}
			}

			return trivia;
		}

		private LeeSyntaxNode LexDirectiveTrivia()
		{
			LeeSyntaxNode trivia = null;

			this.Start();
			char ch = TextWindow.PeekChar();
			switch (ch)
			{
				case '/':
					if (TextWindow.PeekChar(1) == '/')
					{
						// normal single line comment
						this.ScanToEndOfLine();
						var text = TextWindow.GetText(false);
						trivia = SyntaxFactory.Comment(text);
					}

					break;
				case '\r':
				case '\n':
					trivia = this.ScanEndOfLine();
					break;
				case ' ':
				case '\t':       // Horizontal tab
				case '\v':       // Vertical Tab
				case '\f':       // Form-feed
					trivia = this.ScanWhitespace();
					break;

				default:
					if (SyntaxFacts.IsWhitespace(ch))
					{
						goto case ' ';
					}

					if (SyntaxFacts.IsNewLine(ch))
					{
						goto case '\n';
					}

					break;
			}

			return trivia;
		}

		private bool AdvanceIfMatches(char ch)
		{
			char peekCh = TextWindow.PeekChar();
			if ((peekCh == ch) ||
				(peekCh == '{' && ch == '<') ||
				(peekCh == '}' && ch == '>'))
			{
				TextWindow.AdvanceChar();
				return true;
			}

			if (peekCh == '&')
			{
				int pos = TextWindow.Position;
				TextWindow.Reset(pos);
			}

			return false;
		}

		private void AddCrefError(ErrorCode code, params object[] args)
		{
			this.AddCrefError(MakeError(code, args));
		}

		private void AddCrefError(DiagnosticInfo info)
		{
			if (info != null)
			{
				this.AddError(ErrorCode.WRN_ErrorOverride, info, info.Code);
			}
		}

		private void ScanStringLiteral(ref TokenInfo info, bool allowEscapes = true)
		{
			var quoteCharacter = TextWindow.PeekChar();
			if (quoteCharacter == '\'' || quoteCharacter == '"')
			{
				TextWindow.AdvanceChar();
				_builder.Length = 0;
				while (true)
				{
					char ch = TextWindow.PeekChar();
					if (ch == '\\' && allowEscapes)
					{
						// normal string & char constants can have escapes
						char c2;
						ch = this.ScanEscapeSequence(out c2);
						_builder.Append(ch);
						if (c2 != SlidingTextWindow.InvalidCharacter)
						{
							_builder.Append(c2);
						}
					}
					else if (ch == quoteCharacter)
					{
						TextWindow.AdvanceChar();
						break;
					}
					else if (SyntaxFacts.IsNewLine(ch) ||
							(ch == SlidingTextWindow.InvalidCharacter && TextWindow.IsReallyAtEnd()))
					{
						//String and character literals can contain any Unicode character. They are not limited
						//to valid UTF-16 characters. So if we get the SlidingTextWindow's sentinel value,
						//double check that it was not real user-code contents. This will be rare.
						Debug.Assert(TextWindow.Width > 0);
						this.AddError(ErrorCode.ERR_NewlineInConst);
						break;
					}
					else
					{
						TextWindow.AdvanceChar();
						_builder.Append(ch);
					}
				}

				info.Text = TextWindow.GetText(true);
				if (quoteCharacter == '\'')
				{
					info.Kind = SyntaxKind.CharacterLiteralToken;
					if (_builder.Length != 1)
					{
						this.AddError((_builder.Length != 0) ? ErrorCode.ERR_TooManyCharsInConst : ErrorCode.ERR_EmptyCharConst);
					}

					if (_builder.Length > 0)
					{
						info.StringValue = TextWindow.Intern(_builder);
						info.CharValue = info.StringValue[0];
					}
					else
					{
						info.StringValue = string.Empty;
						info.CharValue = SlidingTextWindow.InvalidCharacter;
					}
				}
				else
				{
					info.Kind = SyntaxKind.StringLiteralToken;
					if (_builder.Length > 0)
					{
						info.StringValue = TextWindow.Intern(_builder);
					}
					else
					{
						info.StringValue = string.Empty;
					}
				}
			}
			else
			{
				info.Kind = SyntaxKind.None;
				info.Text = null;
			}
		}

		private char ScanEscapeSequence(out char surrogateCharacter)
		{
			var start = TextWindow.Position;
			surrogateCharacter = SlidingTextWindow.InvalidCharacter;
			char ch = TextWindow.NextChar();
			Debug.Assert(ch == '\\');

			ch = TextWindow.NextChar();
			switch (ch)
			{
				// escaped characters that translate to themselves
				case '\'':
				case '"':
				case '\\':
					break;
				// translate escapes as per C# spec 2.4.4.4
				case '0':
					ch = '\u0000';
					break;
				case 'a':
					ch = '\u0007';
					break;
				case 'b':
					ch = '\u0008';
					break;
				case 'f':
					ch = '\u000c';
					break;
				case 'n':
					ch = '\u000a';
					break;
				case 'r':
					ch = '\u000d';
					break;
				case 't':
					ch = '\u0009';
					break;
				case 'v':
					ch = '\u000b';
					break;
				case 'x':
				case 'u':
				case 'U':
					TextWindow.Reset(start);
					SyntaxDiagnosticInfo error;
					ch = TextWindow.NextUnicodeEscape(surrogateCharacter: out surrogateCharacter, info: out error);
					AddError(error);
					break;
				default:
					this.AddError(start, TextWindow.Position - start, ErrorCode.ERR_IllegalEscape);
					break;
			}

			return ch;
		}

		private void ScanVerbatimStringLiteral(ref TokenInfo info, bool allowNewlines = true)
		{
			_builder.Length = 0;

			if (TextWindow.PeekChar() == '@' && TextWindow.PeekChar(1) == '"')
			{
				TextWindow.AdvanceChar(2);
				bool done = false;
				char ch;
				_builder.Length = 0;
				while (!done)
				{
					switch (ch = TextWindow.PeekChar())
					{
						case '"':
							TextWindow.AdvanceChar();
							if (TextWindow.PeekChar() == '"')
							{
								// Doubled quote -- skip & put the single quote in the string
								TextWindow.AdvanceChar();
								_builder.Append(ch);
							}
							else
							{
								done = true;
							}

							break;

						case SlidingTextWindow.InvalidCharacter:
							if (!TextWindow.IsReallyAtEnd())
							{
								goto default;
							}

							// Reached the end of the source without finding the end-quote.  Give
							// an error back at the starting point.
							this.AddError(ErrorCode.ERR_UnterminatedStringLit);
							done = true;
							break;

						default:
							if (!allowNewlines && SyntaxFacts.IsNewLine(ch))
							{
								this.AddError(ErrorCode.ERR_UnterminatedStringLit);
								done = true;
								break;
							}

							TextWindow.AdvanceChar();
							_builder.Append(ch);
							break;
					}
				}

				info.Kind = SyntaxKind.StringLiteralToken;
				info.Text = TextWindow.GetText(false);
				info.StringValue = _builder.ToString();
			}
			else
			{
				info.Kind = SyntaxKind.None;
				info.Text = null;
				info.StringValue = null;
			}
		}
	}
}
