using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace leec
{
	public class StrongNameFileSystem
	{
		public readonly static StrongNameFileSystem Instance = new StrongNameFileSystem();
		public readonly string _tempPath;

		public StrongNameFileSystem(string tempPath = null)
		{
			_tempPath = tempPath;
		}

		public virtual FileStream CreateFileStream(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			return new FileStream(filePath, fileMode, fileAccess, fileShare);
		}

		public virtual byte[] ReadAllBytes(string fullPath)
		{
			Debug.Assert(PathUtilities.IsAbsolute(fullPath));
			return File.ReadAllBytes(fullPath);
		}

		public virtual bool FileExists(string fullPath)
		{
			Debug.Assert(fullPath == null || PathUtilities.IsAbsolute(fullPath));
			return File.Exists(fullPath);
		}

		public virtual string GetTempPath() => _tempPath ?? Path.GetTempPath();
	}
}
