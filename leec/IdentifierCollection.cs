using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	internal class IdentifierCollection
	{
		// Maps an identifier to all spellings of that identifier in this module.  The value type is
		// typed as object so that it can store either an individual element (the common case), or a 
		// collection.
		//
		// Note: we use a case insensitive comparer so that we can quickly lookup if we know a name
		// regardless of its case.
		private readonly Dictionary<string, object> _map = new Dictionary<string, object>(
			StringComparer.OrdinalIgnoreCase);

		public IdentifierCollection()
		{
		}

		public IdentifierCollection(IEnumerable<string> identifiers)
		{
			this.AddIdentifiers(identifiers);
		}

		public void AddIdentifiers(IEnumerable<string> identifiers)
		{
			foreach (var identifier in identifiers)
			{
				AddIdentifier(identifier);
			}
		}

		public void AddIdentifier(string identifier)
		{
			Debug.Assert(identifier != null);

			object value;
			if (!_map.TryGetValue(identifier, out value))
			{
				AddInitialSpelling(identifier);
			}
			else
			{
				AddAdditionalSpelling(identifier, value);
			}
		}

		private void AddAdditionalSpelling(string identifier, object value)
		{
			// Had a mapping for it.  It will either map to a single 
			// spelling, or to a set of spellings.
			var strValue = value as string;
			if (strValue != null)
			{
				if (!string.Equals(identifier, strValue, StringComparison.Ordinal))
				{
					// We now have two spellings.  Create a collection for
					// that and map the name to it.
					_map[identifier] = new HashSet<string> { identifier, strValue };
				}
			}
			else
			{
				// We have multiple spellings already.
				var spellings = (HashSet<string>)value;

				// Note: the set will prevent duplicates.
				spellings.Add(identifier);
			}
		}

		private void AddInitialSpelling(string identifier)
		{
			// We didn't have any spellings for this word already.  Just
			// add the word as the single known spelling.
			_map.Add(identifier, identifier);
		}

		public bool ContainsIdentifier(string identifier, bool caseSensitive)
		{
			Debug.Assert(identifier != null);

			if (caseSensitive)
			{
				return CaseSensitiveContains(identifier);
			}
			else
			{
				return CaseInsensitiveContains(identifier);
			}
		}

		private bool CaseInsensitiveContains(string identifier)
		{
			// Simple case.  Just check if we've mapped this word to 
			// anything.  The map will take care of the case insensitive
			// lookup for us.
			return _map.ContainsKey(identifier);
		}

		private bool CaseSensitiveContains(string identifier)
		{
			object spellings;
			if (_map.TryGetValue(identifier, out spellings))
			{
				var spelling = spellings as string;
				if (spelling != null)
				{
					return string.Equals(identifier, spelling, StringComparison.Ordinal);
				}

				var set = (HashSet<string>)spellings;
				return set.Contains(identifier);
			}

			return false;
		}

		public ICollection<string> AsCaseSensitiveCollection()
		{
			return new CaseSensitiveCollection(this);
		}

		public ICollection<string> AsCaseInsensitiveCollection()
		{
			return new CaseInsensitiveCollection(this);
		}

		private abstract class CollectionBase : ICollection<string>
		{
			protected readonly IdentifierCollection IdentifierCollection;
			private int _count = -1;

			protected CollectionBase(IdentifierCollection identifierCollection)
			{
				this.IdentifierCollection = identifierCollection;
			}

			public abstract bool Contains(string item);

			public void CopyTo(string[] array, int arrayIndex)
			{
				using (var enumerator = this.GetEnumerator())
				{
					while (arrayIndex < array.Length && enumerator.MoveNext())
					{
						array[arrayIndex] = enumerator.Current;
						arrayIndex++;
					}
				}
			}

			public int Count
			{
				get
				{
					if (_count == -1)
					{
						_count = this.IdentifierCollection._map.Values.Sum(o => o is string ? 1 : ((ISet<string>)o).Count);
					}

					return _count;
				}
			}

			public bool IsReadOnly => true;

			public IEnumerator<string> GetEnumerator()
			{
				foreach (var obj in this.IdentifierCollection._map.Values)
				{
					var strs = obj as HashSet<string>;
					if (strs != null)
					{
						foreach (var s in strs)
						{
							yield return s;
						}
					}
					else
					{
						yield return (string)obj;
					}
				}
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			#region Unsupported  
			public void Add(string item)
			{
				throw new NotSupportedException();
			}

			public void Clear()
			{
				throw new NotSupportedException();
			}

			public bool Remove(string item)
			{
				throw new NotSupportedException();
			}
			#endregion
		}

		private sealed class CaseSensitiveCollection : CollectionBase
		{
			public CaseSensitiveCollection(IdentifierCollection identifierCollection) : base(identifierCollection)
			{
			}

			public override bool Contains(string item) => IdentifierCollection.CaseSensitiveContains(item);
		}

		private sealed class CaseInsensitiveCollection : CollectionBase
		{
			public CaseInsensitiveCollection(IdentifierCollection identifierCollection) : base(identifierCollection)
			{
			}

			public override bool Contains(string item) => IdentifierCollection.CaseInsensitiveContains(item);
		}
	}
}
