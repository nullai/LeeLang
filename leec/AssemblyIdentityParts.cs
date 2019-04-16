using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	[Flags]
	public enum AssemblyIdentityParts
	{
		Name = 1,
		Version = VersionMajor | VersionMinor | VersionBuild | VersionRevision,

		// version parts are assumed to be in order:
		VersionMajor = 1 << 1,
		VersionMinor = 1 << 2,
		VersionBuild = 1 << 3,
		VersionRevision = 1 << 4,

		Culture = 1 << 5,
		PublicKey = 1 << 6,
		PublicKeyToken = 1 << 7,
		PublicKeyOrToken = PublicKey | PublicKeyToken,
		Retargetability = 1 << 8,
		ContentType = 1 << 9,

		Unknown = 1 << 10
	}
}
