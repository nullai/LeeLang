using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace leelang
{
	public class Grammar
	{
		public static TokenExpression ParserTokenTree(Tokenizer tokener)
		{
			List<TokenExpression> vals = new List<TokenExpression>();
			while (true)
			{
				var v = ParserAny(tokener);
				if (v == null)
					break;
				vals.Add(v);
			}
			return new TokenExpression(null, vals.ToArray());
		}

		public static TokenExpression ParserAny(Tokenizer tokener)
		{
			var token = tokener.Current;
			switch (token.token)
			{
				case Token.ABSTRACT:
				case Token.BOOL:
				case Token.BREAK:
				case Token.BYTE:
				case Token.CHAR:
					tokener.Next();
					return new TokenExpression(token, null);
				case Token.CLASS:
					break;
			}
			return null;
		}
	}
}
