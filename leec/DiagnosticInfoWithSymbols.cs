using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public class DiagnosticInfoWithSymbols : DiagnosticInfo
	{
		// not serialized:
		public readonly Symbol[] Symbols;

		public DiagnosticInfoWithSymbols(ErrorCode errorCode, object[] arguments, Symbol[] symbols)
			: base(leec.MessageProvider.Instance, (int)errorCode, arguments)
		{
			this.Symbols = symbols;
		}

		public DiagnosticInfoWithSymbols(bool isWarningAsError, ErrorCode errorCode, object[] arguments, Symbol[] symbols)
			: base(leec.MessageProvider.Instance, isWarningAsError, (int)errorCode, arguments)
		{
			this.Symbols = symbols;
		}
	}
}
