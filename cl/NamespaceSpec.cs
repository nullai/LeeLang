using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class NamespaceSpec : MemberSpec
	{
		public string fullname;
		public Dictionary<string, List<MemberSpec>> members = new Dictionary<string, List<MemberSpec>>();

		public NamespaceSpec(string name, NamespaceSpec declare)
			: base(name, declare)
		{
			fullname = declare == null ? name : declare.fullname + "." + name;
		}

		public override void ResolveMember(string name, List<MemberSpec> result, VerifyMember verify)
		{
			List<MemberSpec> vals = GetMembers(name);
			if (vals == null)
			{
				if (Declaring != null)
					Declaring.ResolveMember(name, result, verify);
				return;
			}

			for (int i = 0; i < vals.Count; i++)
			{
				var item = vals[i];
				if (item.name == name && item.prefix == null)
				{
					if (verify == null || verify(item))
						result.Add(item);
					continue;
				}
			}

			if (Declaring != null)
				Declaring.ResolveMember(name, result, verify);
		}
		public virtual List<MemberSpec> GetMembers(string name)
		{
			List<MemberSpec> r;
			members.TryGetValue(name, out r);
			return r;
		}
	}
}
