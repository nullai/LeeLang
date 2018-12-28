using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class AssemblySpec
	{
		public List<ModuleSpec> modules = new List<ModuleSpec>();
		public ModuleSpec CurrentModule => modules[0];
		public bool resolving = false;

		public AssemblySpec(string name)
		{
			ModuleSpec mod = new ModuleSpec(name, this);
			modules.Add(mod);
		}

		public void ResolveMember(string name, List<MemberSpec> result, MemberSpec.VerifyMember verify)
		{
			if (resolving)
				return;
			resolving = true;
			for (int i = 0; i < modules.Count; i++)
			{
				var m = modules[i];
				m.ResolveMember(name, result, verify);
			}
			resolving = false;
		}
	}
}
