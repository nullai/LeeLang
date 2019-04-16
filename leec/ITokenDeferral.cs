using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	internal interface ITokenDeferral
	{
		uint GetFakeStringTokenForIL(string value);
		uint GetFakeSymbolTokenForIL(IReference value, GreenNode syntaxNode, DiagnosticBag diagnostics);
		//uint GetSourceDocumentIndexForIL(DebugSourceDocument document);

		IFieldReference GetFieldForData(byte[] data, GreenNode syntaxNode, DiagnosticBag diagnostics);
		IMethodReference GetInitArrayHelper();

		string GetStringFromToken(uint token);
		IReference GetReferenceFromToken(uint token);

		ArrayMethods ArrayMethods { get; }
	}
}
