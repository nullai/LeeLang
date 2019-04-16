using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public struct EmitContext
	{
		public readonly CommonPEModuleBuilder Module;
		public readonly GreenNode SyntaxNodeOpt;
		public readonly DiagnosticBag Diagnostics;
		private readonly Flags _flags;

		public bool IncludePrivateMembers => (_flags & Flags.IncludePrivateMembers) != 0;
		public bool MetadataOnly => (_flags & Flags.MetadataOnly) != 0;
		public bool IsRefAssembly => MetadataOnly && !IncludePrivateMembers;

		public EmitContext(CommonPEModuleBuilder module, GreenNode syntaxNodeOpt, DiagnosticBag diagnostics, bool metadataOnly, bool includePrivateMembers)
		{
			Debug.Assert(module != null);
			Debug.Assert(diagnostics != null);
			Debug.Assert(includePrivateMembers || metadataOnly);

			Module = module;
			SyntaxNodeOpt = syntaxNodeOpt;
			Diagnostics = diagnostics;

			Flags flags = Flags.None;
			if (metadataOnly)
			{
				flags |= Flags.MetadataOnly;
			}
			if (includePrivateMembers)
			{
				flags |= Flags.IncludePrivateMembers;
			}
			_flags = flags;
		}

		[Flags]
		private enum Flags
		{
			None = 0,
			MetadataOnly = 1,
			IncludePrivateMembers = 2,
		}
	}
}
