using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class FileSpec : NamespaceSpec
	{
		public ModuleSpec module;
		public string path;
		public bool resolving = false;

		public FileSpec(string name, ModuleSpec module)
			: base(name, null)
		{
			this.module = module;
		}

		public override void ResolveMember(string name, List<MemberSpec> result, VerifyMember verify)
		{
			if (resolving)
				return;
			resolving = true;
			base.ResolveMember(name, result, verify);

			module.ResolveMember(name, result, verify);
			resolving = false;
		}
	}
}
