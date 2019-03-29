using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public class ParseOptions
	{
		private readonly Lazy<Diagnostic[]> _lazyErrors;
		public string[] PreprocessorSymbols;

		public ParseOptions(string[] preprocessorSymbols)
		{
			PreprocessorSymbols = preprocessorSymbols;
			_lazyErrors = new Lazy<Diagnostic[]>(() =>
			{
				var builder = new List<Diagnostic>();
				ValidateOptions(builder);
				return builder.ToArray();
			});
		}

		public void ValidateOptions(List<Diagnostic> builder)
		{
			if (PreprocessorSymbols != null)
			{
				foreach (var symbol in PreprocessorSymbols)
				{
					if (symbol == null)
					{
						builder.Add(Diagnostic.Create(MessageProvider.Instance, (int)ErrorCode.ERR_InvalidPreprocessingSymbol, "null"));
					}
					else if (!SyntaxFacts.IsValidIdentifier(symbol))
					{
						builder.Add(Diagnostic.Create(MessageProvider.Instance, (int)ErrorCode.ERR_InvalidPreprocessingSymbol, symbol));
					}
				}
			}
		}
	}
}
