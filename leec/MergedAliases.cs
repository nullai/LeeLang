using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class MergedAliases
	{
		public List<string> AliasesOpt;
		public List<string> RecursiveAliasesOpt;

		/// <summary>
		/// Adds aliases of a specified reference to the merged set of aliases.
		/// Consider the following special cases:
		/// 
		/// o {} + {} = {} 
		///   If neither reference has any aliases then the result has no aliases.
		/// 
		/// o {A} + {} = {A, global}
		///   {} + {A} = {A, global}
		///   
		///   If one and only one of the references has aliases we add the global alias since the 
		///   referenced declarations should now be accessible both via existing aliases 
		///   as well as unqualified.
		///   
		/// o {A, A} + {A, B, B} = {A, A, B, B}
		///   We preserve dups in each alias array, but avoid making more dups when merging.
		/// </summary>
		public void Merge(MetadataReference reference)
		{
			if (reference.Properties.HasRecursiveAliases)
			{
				if (RecursiveAliasesOpt == null)
				{
					RecursiveAliasesOpt = new List<string>();
					RecursiveAliasesOpt.AddRange(reference.Properties.Aliases);
					return;
				}
			}
			else
			{
				if (AliasesOpt == null)
				{
					AliasesOpt = new List<string>();
					AliasesOpt.AddRange(reference.Properties.Aliases);
					return;
				}
			}

			Merge(
				aliases: reference.Properties.HasRecursiveAliases ? RecursiveAliasesOpt : AliasesOpt,
				newAliases: reference.Properties.Aliases);
		}

		public static void Merge(List<string> aliases, string[] newAliases)
		{
			if (aliases.Count == 0 ^ newAliases.Length == 0)
			{
				AddNonIncluded(aliases, MetadataReferenceProperties.GlobalAlias);
			}

			AddNonIncluded(aliases, newAliases);
		}

		public static string[] Merge(string[] aliasesOpt, string[] newAliases)
		{
			if (aliasesOpt.Length == 0)
			{
				return newAliases;
			}

			var result = new List<string>(aliasesOpt.Length);
			result.AddRange(aliasesOpt);
			Merge(result, newAliases);
			return result.ToArray();
		}

		private static void AddNonIncluded(List<string> builder, string item)
		{
			if (!builder.Contains(item))
			{
				builder.Add(item);
			}
		}

		private static void AddNonIncluded(List<string> builder, string[] items)
		{
			int originalCount = builder.Count;

			foreach (var item in items)
			{
				if (builder.IndexOf(item, 0, originalCount) < 0)
				{
					builder.Add(item);
				}
			}
		}
	}
}
