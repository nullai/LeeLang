using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public class LexerCache
	{
		private static readonly ObjectPool<CachingIdentityFactory<string, SyntaxKind>> s_keywordKindPool = CachingIdentityFactory<string, SyntaxKind>.CreatePool(512, (key) =>
							{
								var kind = SyntaxFacts.GetKeywordKind(key);
								if (kind == SyntaxKind.None)
								{
									kind = SyntaxFacts.GetContextualKeywordKind(key);
								}

								return kind;
							});

		private readonly TextKeyedCache<SyntaxTrivia> _triviaMap;
		private readonly TextKeyedCache<SyntaxToken> _tokenMap;
		private readonly CachingIdentityFactory<string, SyntaxKind> _keywordKindMap;
		public const int MaxKeywordLength = 10;

		public LexerCache()
		{
			_triviaMap = TextKeyedCache<SyntaxTrivia>.GetInstance();
			_tokenMap = TextKeyedCache<SyntaxToken>.GetInstance();
			_keywordKindMap = s_keywordKindPool.Allocate();
		}

		public void Free()
		{
			_keywordKindMap.Free();
			_triviaMap.Free();
			_tokenMap.Free();
		}

		public bool TryGetKeywordKind(string key, out SyntaxKind kind)
		{
			if (key.Length > MaxKeywordLength)
			{
				kind = SyntaxKind.None;
				return false;
			}

			kind = _keywordKindMap.GetOrMakeValue(key);
			return kind != SyntaxKind.None;
		}

		public SyntaxTrivia LookupTrivia(
			char[] textBuffer,
			int keyStart,
			int keyLength,
			int hashCode,
			Func<SyntaxTrivia> createTriviaFunction)
		{
			var value = _triviaMap.FindItem(textBuffer, keyStart, keyLength, hashCode);

			if (value == null)
			{
				value = createTriviaFunction();
				_triviaMap.AddItem(textBuffer, keyStart, keyLength, hashCode, value);
			}

			return value;
		}

		public SyntaxToken LookupToken(
			char[] textBuffer,
			int keyStart,
			int keyLength,
			int hashCode,
			Func<SyntaxToken> createTokenFunction)
		{
			var value = _tokenMap.FindItem(textBuffer, keyStart, keyLength, hashCode);

			if (value == null)
			{
				value = createTokenFunction();
				_tokenMap.AddItem(textBuffer, keyStart, keyLength, hashCode, value);
			}

			return value;
		}
	}
}
