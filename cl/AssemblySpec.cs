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

		public AssemblySpec(string name)
		{
			ModuleSpec mod = new ModuleSpec(name, this);
			modules.Add(mod);
		}
		public List<MemberSpec> ResolveType(string name)
		{
			List<MemberSpec> r = null;
			for (int i = 0; i < modules.Count; i++)
			{
				var m = modules[i];
				var v = m.ResolveName(name);
				if (v != null)
				{
					if (r != null)
						r.AddRange(v);
					else
						r = v;
				}
			}
			return r;
		}
	}
}
