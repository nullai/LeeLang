using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DWORD = System.UInt32;

namespace leec
{
	public class Win32Resource : IWin32Resource
	{
		private readonly byte[] _data;
		private readonly DWORD _codePage;
		private readonly DWORD _languageId;
		private readonly int _id;
		private readonly string _name;
		private readonly int _typeId;
		private readonly string _typeName;

		public Win32Resource(
			byte[] data,
			DWORD codePage,
			DWORD languageId,
			int id,
			string name,
			int typeId,
			string typeName)
		{
			_data = data;
			_codePage = codePage;
			_languageId = languageId;
			_id = id;
			_name = name;
			_typeId = typeId;
			_typeName = typeName;
		}

		public string TypeName => _typeName;

		public int TypeId => _typeId;

		public string Name => _name;

		public int Id => _id;

		public DWORD LanguageId => _languageId;

		public DWORD CodePage => _codePage;

		public IEnumerable<byte> Data => _data;
	}
}
