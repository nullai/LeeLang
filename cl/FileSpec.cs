using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeeLang
{
	public class FileSpec : NamespaceSpec
	{
		public ModuleSpec module;

		public FileSpec(string name, ModuleSpec module)
			: base(Path.GetFileName(name), null)
		{
			this.module = module;
			this.fullname = name;
		}
	}
}
