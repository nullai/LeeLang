using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class ModuleSpec : NamespaceSpec
	{
		public AssemblySpec assembly;
		public string path;
		public bool resolving = false;

		public ModuleSpec(string name, AssemblySpec assembly)
			: base(name, null)
		{
			this.assembly = assembly;
		}

		public override void ResolveMember(string name, List<MemberSpec> result, VerifyMember verify)
		{
			if (resolving)
				return;
			resolving = true;
			base.ResolveMember(name, result, verify);

			assembly.ResolveMember(name, result, verify);
			resolving = false;
		}
	}
}
