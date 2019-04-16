using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class PEAssembly
	{
		/// <summary>
		/// All assemblies this assembly references.
		/// </summary>
		/// <remarks>
		/// A concatenation of assemblies referenced by each module in the order they are listed in <see cref="_modules"/>.
		/// </remarks>
		public readonly AssemblyIdentity[] AssemblyReferences;

		/// <summary>
		/// The number of assemblies referenced by each module in <see cref="_modules"/>.
		/// </summary>
		public readonly int[] ModuleReferenceCounts;

		private readonly PEModule[] _modules;

		/// <summary>
		/// Assembly identity read from Assembly table, or null if the table is empty.
		/// </summary>
		private readonly AssemblyIdentity _identity;

		/// <summary>
		/// Using <see cref="ThreeState"/> for atomicity.
		/// </summary>
		private ThreeState _lazyContainsNoPiaLocalTypes;

		private ThreeState _lazyDeclaresTheObjectClass;

		/// <summary>
		/// We need to store reference to the assembly metadata to keep the metadata alive while 
		/// symbols have reference to PEAssembly.
		/// </summary>
		private readonly AssemblyMetadata _owner;

		//Maps from simple name to list of public keys. If an IVT attribute specifies no public
		//key, the list contains one element with an empty value
		private Dictionary<string, List<byte[]>> _lazyInternalsVisibleToMap;

		/// <exception cref="BadImageFormatException"/>
		public PEAssembly(AssemblyMetadata owner, PEModule[] modules)
		{
			Debug.Assert(modules != null);
			Debug.Assert(modules.Length > 0);

			_identity = modules[0].ReadAssemblyIdentityOrThrow();

			var refs = new List<AssemblyIdentity>();
			int[] refCounts = new int[modules.Length];

			for (int i = 0; i < modules.Length; i++)
			{
				AssemblyIdentity[] refsForModule = modules[i].ReferencedAssemblies;
				refCounts[i] = refsForModule.Length;
				refs.AddRange(refsForModule);
			}

			_modules = modules;
			this.AssemblyReferences = refs.ToArray();
			this.ModuleReferenceCounts = refCounts;
			_owner = owner;
		}

		//public EntityHandle Handle
		//{
		//	get
		//	{
		//		return EntityHandle.AssemblyDefinition;
		//	}
		//}

		public PEModule ManifestModule
		{
			get { return Modules[0]; }
		}

		public PEModule[] Modules
		{
			get
			{
				return _modules;
			}
		}

		public AssemblyIdentity Identity
		{
			get
			{
				return _identity;
			}
		}

		public bool ContainsNoPiaLocalTypes()
		{
			if (_lazyContainsNoPiaLocalTypes == ThreeState.Unknown)
			{
				foreach (PEModule module in Modules)
				{
					if (module.ContainsNoPiaLocalTypes())
					{
						_lazyContainsNoPiaLocalTypes = ThreeState.True;
						return true;
					}
				}

				_lazyContainsNoPiaLocalTypes = ThreeState.False;
			}

			return _lazyContainsNoPiaLocalTypes == ThreeState.True;
		}
		/*
		private Dictionary<string, List<ImmutableArray<byte>>> BuildInternalsVisibleToMap()
		{
			var ivtMap = new Dictionary<string, List<ImmutableArray<byte>>>(StringComparer.OrdinalIgnoreCase);
			foreach (string attrVal in Modules[0].GetInternalsVisibleToAttributeValues(Handle))
			{
				AssemblyIdentity identity;
				if (AssemblyIdentity.TryParseDisplayName(attrVal, out identity))
				{
					List<ImmutableArray<byte>> keys;
					if (ivtMap.TryGetValue(identity.Name, out keys))
						keys.Add(identity.PublicKey);
					else
					{
						keys = new List<ImmutableArray<byte>>();
						keys.Add(identity.PublicKey);
						ivtMap[identity.Name] = keys;
					}
				}
				else
				{
					// Dev10 C# reports WRN_InvalidAssemblyName and Dev10 VB reports ERR_FriendAssemblyNameInvalid but
					// we have no way to do that from here.  Since the absence of these diagnostics does not impact the
					// user experience enough to justify the work required to produce them, we will simply omit them
					// (DevDiv #15099, #14348).
				}
			}

			return ivtMap;
		}

		public IEnumerable<ImmutableArray<byte>> GetInternalsVisibleToPublicKeys(string simpleName)
		{
			if (_lazyInternalsVisibleToMap == null)
				Interlocked.CompareExchange(ref _lazyInternalsVisibleToMap, BuildInternalsVisibleToMap(), null);

			List<ImmutableArray<byte>> result;

			_lazyInternalsVisibleToMap.TryGetValue(simpleName, out result);

			return result ?? SpecializedCollections.EmptyEnumerable<ImmutableArray<byte>>();
		}
		*/
		public bool DeclaresTheObjectClass
		{
			get
			{
				if (_lazyDeclaresTheObjectClass == ThreeState.Unknown)
				{
					throw new Exception();
					//var value = _modules[0].MetadataReader.DeclaresTheObjectClass();
					//_lazyDeclaresTheObjectClass = value.ToThreeState();
				}

				return _lazyDeclaresTheObjectClass == ThreeState.True;
			}
		}

		public AssemblyMetadata GetNonDisposableMetadata() => _owner.Copy();
	}
}
