using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class LeeDiagnosticInfo : DiagnosticInfoWithSymbols
	{
		public static readonly DiagnosticInfo EmptyErrorInfo = new LeeDiagnosticInfo(0);
		public static readonly DiagnosticInfo VoidDiagnosticInfo = new LeeDiagnosticInfo(ErrorCode.Void);

		private readonly Location[] _additionalLocations;

		public LeeDiagnosticInfo(ErrorCode code)
			: this(code, new object[0], new Symbol[0], new Location[0])
		{
		}

		public LeeDiagnosticInfo(ErrorCode code, params object[] args)
			: this(code, args, new Symbol[0], new Location[0])
		{
		}

		public LeeDiagnosticInfo(ErrorCode code, Symbol[] symbols, object[] args)
			: this(code, args, symbols, new Location[0])
		{
		}

		public LeeDiagnosticInfo(ErrorCode code, object[] args, Symbol[] symbols, Location[] additionalLocations)
			: base(code, args, symbols)
		{
			_additionalLocations = additionalLocations;
		}

		public override IList<Location> AdditionalLocations => _additionalLocations;

		public new ErrorCode Code => (ErrorCode)base.Code;

		public static bool IsEmpty(DiagnosticInfo info) => (object)info == EmptyErrorInfo;
	}
}
