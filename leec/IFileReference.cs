using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface IFileReference
	{
		/// <summary>
		/// True if the file has metadata.
		/// </summary>
		bool HasMetadata { get; }

		/// <summary>
		/// File name with extension.
		/// </summary>
		string FileName { get; }

		/// <summary>
		/// A hash of the file contents.
		/// </summary>
		byte[] GetHashValue();
	}
}
