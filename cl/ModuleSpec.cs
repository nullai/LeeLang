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

		public ModuleSpec(string name, AssemblySpec assembly)
			: base(name, null)
		{
			this.assembly = assembly;
		}
		public override List<MemberSpec> ResolveName(string name)
		{
			if (this != assembly.CurrentModule)
			{
				return base.ResolveName(name);
			}
			foreach(List<MemberSpec> vals in members.Values)
			{
				for (int i = 0; i < vals.Count; i++)
				{
					var item = vals[i];
					if (item.name == name && item.prefix == null)
					{
						return new List<MemberSpec>() { item };
					}
				}
			}
			return null;
		}
	}
}
