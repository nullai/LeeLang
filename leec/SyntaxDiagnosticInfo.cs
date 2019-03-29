using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public class SyntaxDiagnosticInfo : DiagnosticInfo
	{
		public readonly int Offset;
		public readonly int Width;

		public SyntaxDiagnosticInfo(int offset, int width, ErrorCode code, params object[] args)
			: base(leec.MessageProvider.Instance, (int)code, args)
		{
			Debug.Assert(width >= 0);
			this.Offset = offset;
			this.Width = width;
		}

		public SyntaxDiagnosticInfo(int offset, int width, ErrorCode code)
			: this(offset, width, code, new object[0])
		{
		}

		public SyntaxDiagnosticInfo(ErrorCode code, params object[] args)
			: this(0, 0, code, args)
		{
		}

		public SyntaxDiagnosticInfo(ErrorCode code)
			: this(0, 0, code)
		{
		}

		public SyntaxDiagnosticInfo WithOffset(int offset)
		{
			return new SyntaxDiagnosticInfo(offset, this.Width, (ErrorCode)this.Code, this.Arguments);
		}
	}
}
