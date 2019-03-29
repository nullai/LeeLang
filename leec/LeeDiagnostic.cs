using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class LeeDiagnostic : DiagnosticWithInfo
	{
		public LeeDiagnostic(DiagnosticInfo info, Location location, bool isSuppressed = false)
			: base(info, location, isSuppressed)
		{
		}

		public override string ToString()
		{
			return LeeDiagnosticFormatter.Instance.Format(this);
		}

		public override Diagnostic WithLocation(Location location)
		{
			if (location == null)
			{
				throw new ArgumentNullException(nameof(location));
			}

			if (location != this.Location)
			{
				return new LeeDiagnostic(this.Info, location, this.IsSuppressed);
			}

			return this;
		}

		public override Diagnostic WithSeverity(DiagnosticSeverity severity)
		{
			if (this.Severity != severity)
			{
				return new LeeDiagnostic(this.Info.GetInstanceWithSeverity(severity), this.Location, this.IsSuppressed);
			}

			return this;
		}

		public override Diagnostic WithIsSuppressed(bool isSuppressed)
		{
			if (this.IsSuppressed != isSuppressed)
			{
				return new LeeDiagnostic(this.Info, this.Location, isSuppressed);
			}

			return this;
		}
	}

	public class LeeDiagnosticFormatter : DiagnosticFormatter
	{
		public LeeDiagnosticFormatter()
		{
		}

		public new static LeeDiagnosticFormatter Instance { get; } = new LeeDiagnosticFormatter();
	}
}
