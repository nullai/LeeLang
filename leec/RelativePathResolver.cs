using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace leec
{
	public class RelativePathResolver : IEquatable<RelativePathResolver>
	{
		public string[] SearchPaths { get; }
		public string BaseDirectory { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="RelativePathResolver"/> class.
		/// </summary>
		/// <param name="searchPaths">An ordered set of fully qualified 
		/// paths which are searched when resolving assembly names.</param>
		/// <param name="baseDirectory">Directory used when resolving relative paths.</param>
		public RelativePathResolver(string[] searchPaths, string baseDirectory)
		{
			Debug.Assert(searchPaths.All(PathUtilities.IsAbsolute));
			Debug.Assert(baseDirectory == null || PathUtilities.GetPathKind(baseDirectory) == PathKind.Absolute);

			SearchPaths = searchPaths;
			BaseDirectory = baseDirectory;
		}

		public string ResolvePath(string reference, string baseFilePath)
		{
			string resolvedPath = FileUtilities.ResolveRelativePath(reference, baseFilePath, BaseDirectory, SearchPaths, FileExists);
			if (resolvedPath == null)
			{
				return null;
			}

			return FileUtilities.TryNormalizeAbsolutePath(resolvedPath);
		}

		protected virtual bool FileExists(string fullPath)
		{
			Debug.Assert(fullPath != null);
			Debug.Assert(PathUtilities.IsAbsolute(fullPath));
			return File.Exists(fullPath);
		}

		public bool Equals(RelativePathResolver other) =>
			BaseDirectory == other.BaseDirectory && SearchPaths.SequenceEqual(other.SearchPaths);

		public override int GetHashCode() =>
			Hash.Combine(BaseDirectory, Hash.CombineValues(SearchPaths));

		public override bool Equals(object obj) => Equals(obj as RelativePathResolver);
	}
}
