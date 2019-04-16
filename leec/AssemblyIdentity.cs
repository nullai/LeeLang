﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace leec
{
	public sealed class AssemblyIdentity : IEquatable<AssemblyIdentity>
	{
		// not null, not empty:
		private readonly string _name;

		// no version: 0.0.0.0
		private readonly Version _version;

		// Invariant culture is represented as empty string.
		// The culture might not exist on the system.
		private readonly string _cultureName;

		// weak name:   empty
		// strong name: full public key or empty (only token is available)
		private readonly byte[] _publicKey;

		// weak name: empty
		// strong name: null (to be initialized), or 8 bytes (initialized)
		private byte[] _lazyPublicKeyToken;

		private readonly bool _isRetargetable;

		// cached display name
		private string _lazyDisplayName;

		// cached hash code
		private int _lazyHashCode;

		public const int PublicKeyTokenSize = 8;

		private AssemblyIdentity(AssemblyIdentity other, Version version)
		{
			Debug.Assert((object)other != null);
			Debug.Assert((object)version != null);

			_name = other._name;
			_cultureName = other._cultureName;
			_publicKey = other._publicKey;
			_lazyPublicKeyToken = other._lazyPublicKeyToken;
			_isRetargetable = other._isRetargetable;

			_version = version;
			_lazyDisplayName = null;
			_lazyHashCode = 0;
		}
		public AssemblyIdentity WithVersion(Version version) => (version == _version) ? this : new AssemblyIdentity(this, version);

		/// <summary>
		/// Constructs an <see cref="AssemblyIdentity"/> from its constituent parts.
		/// </summary>
		/// <param name="name">The simple name of the assembly.</param>
		/// <param name="version">The version of the assembly.</param>
		/// <param name="cultureName">
		/// The name of the culture to associate with the assembly. 
		/// Specify null, <see cref="string.Empty"/>, or "neutral" (any casing) to represent <see cref="System.Globalization.CultureInfo.InvariantCulture"/>.
		/// The name can be an arbitrary string that doesn't contain NUL character, the legality of the culture name is not validated.
		/// </param>
		/// <param name="publicKeyOrToken">The public key or public key token of the assembly.</param>
		/// <param name="hasPublicKey">Indicates whether <paramref name="publicKeyOrToken"/> represents a public key.</param>
		/// <param name="isRetargetable">Indicates whether the assembly is retargetable.</param>
		/// <exception cref="ArgumentException">If <paramref name="name"/> is null, empty or contains a NUL character.</exception>
		/// <exception cref="ArgumentException">If <paramref name="cultureName"/> contains a NUL character.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="contentType"/> is not a value of the <see cref="AssemblyContentType"/> enumeration.</exception>
		/// <exception cref="ArgumentException"><paramref name="version"/> contains values that are not greater than or equal to zero and less than or equal to ushort.MaxValue.</exception>
		/// <exception cref="ArgumentException"><paramref name="hasPublicKey"/> is true and <paramref name="publicKeyOrToken"/> is not set.</exception>
		/// <exception cref="ArgumentException"><paramref name="hasPublicKey"/> is false and <paramref name="publicKeyOrToken"/> 
		/// contains a value that is not the size of a public key token, 8 bytes.</exception>
		public AssemblyIdentity(
			string name,
			Version version = null,
			string cultureName = null,
			byte[] publicKeyOrToken = null,
			bool isRetargetable = false)
		{
			if (!IsValidName(name))
			{
				throw new ArgumentException(string.Format("@InvalidAssemblyName", name), nameof(name));
			}

			if (!IsValidCultureName(cultureName))
			{
				throw new ArgumentException(string.Format("@InvalidCultureName", cultureName), nameof(cultureName));
			}

			// Version allows more values then can be encoded in metadata:
			if (!IsValid(version))
			{
				throw new ArgumentOutOfRangeException(nameof(version));
			}

			if (publicKeyOrToken != null)
			{
				//if (!MetadataHelpers.IsValidPublicKey(publicKeyOrToken))
				//{
				//	throw new ArgumentException(CodeAnalysisResources.InvalidPublicKey, nameof(publicKeyOrToken));
				//}
			}

			if (isRetargetable)
			{
				throw new ArgumentException("@WinRTIdentityCantBeRetargetable", nameof(isRetargetable));
			}

			_name = name;
			_version = version ?? NullVersion;
			_cultureName = NormalizeCultureName(cultureName);
			_isRetargetable = isRetargetable;
			InitializeKey(publicKeyOrToken, out _publicKey, out _lazyPublicKeyToken);
		}

		// asserting constructor used by SourceAssemblySymbol:
		public AssemblyIdentity(
			string name,
			Version version,
			string cultureName,
			byte[] publicKeyOrToken)
		{
			Debug.Assert(name != null);
			Debug.Assert(IsValid(version));
			Debug.Assert(IsValidCultureName(cultureName));

			_name = name;
			_version = version ?? NullVersion;
			_cultureName = NormalizeCultureName(cultureName);
			_isRetargetable = false;
			InitializeKey(publicKeyOrToken, out _publicKey, out _lazyPublicKeyToken);
		}

		// constructor used by metadata reader:
		public AssemblyIdentity(
			bool noThrow,
			string name,
			Version version = null,
			string cultureName = null,
			byte[] publicKeyOrToken = null,
			bool isRetargetable = false)
		{
			Debug.Assert(name != null);
			Debug.Assert(noThrow);

			_name = name;
			_version = version ?? NullVersion;
			_cultureName = NormalizeCultureName(cultureName);
			_isRetargetable = false;
			InitializeKey(publicKeyOrToken, out _publicKey, out _lazyPublicKeyToken);
		}

		private static string NormalizeCultureName(string cultureName)
		{
			// Treat "neutral" culture as invariant culture name, although it is technically not a legal culture name.
			//
			// A few reasons:
			// 1) Invariant culture is displayed as "neutral" in the identity display name.
			//    Thus a) an identity with culture "neutral" wouldn't roundrip serialization to display name.
			//         b) an identity with culture "neutral" wouldn't compare equal to invariant culture identity, 
			//            yet their display names are the same which is confusing.
			//
			// 2) The implementation of AssemblyName.CultureName on Mono incorrectly returns "neutral" for invariant culture identities.

			return cultureName == null || cultureName == InvariantCultureDisplay ? string.Empty : cultureName;
		}

		private static void InitializeKey(byte[] publicKeyOrToken,
			out byte[] publicKey, out byte[] publicKeyToken)
		{
			if (publicKeyOrToken != null)
			{
				publicKey = publicKeyOrToken;
				publicKeyToken = null;
			}
			else
			{
				publicKey = new byte[0];
				publicKeyToken = publicKeyOrToken ?? new byte[0];
			}
		}

		public static bool IsValidCultureName(string name)
		{
			// The native compiler doesn't enforce that the culture be anything in particular. 
			// AssemblyIdentity should preserve user input even if it is of dubious utility.

			// Note: If these checks change, the error messages emitted by the compilers when
			// this case is detected will also need to change. They currently directly
			// name the presence of the NUL character as the reason that the culture name is invalid.
			return name == null || name.IndexOf('\0') < 0;
		}

		private static bool IsValidName(string name)
		{
			return !string.IsNullOrEmpty(name) && name.IndexOf('\0') < 0;
		}

		public readonly static Version NullVersion = new Version(0, 0, 0, 0);

		private static bool IsValid(Version value)
		{
			return value == null
				|| value.Major >= 0
				&& value.Minor >= 0
				&& value.Build >= 0
				&& value.Revision >= 0
				&& value.Major <= ushort.MaxValue
				&& value.Minor <= ushort.MaxValue
				&& value.Build <= ushort.MaxValue
				&& value.Revision <= ushort.MaxValue;
		}
		/// <summary>
		/// The simple name of the assembly.
		/// </summary>
		public string Name { get { return _name; } }

		/// <summary>
		/// The version of the assembly.
		/// </summary>
		public Version Version { get { return _version; } }

		/// <summary>
		/// The culture name of the assembly, or empty if the culture is neutral.
		/// </summary>
		public string CultureName { get { return _cultureName; } }

		/// <summary>
		/// The AssemblyNameFlags.
		/// </summary>
		public AssemblyNameFlags Flags
		{
			get
			{
				return (_isRetargetable ? AssemblyNameFlags.Retargetable : AssemblyNameFlags.None) |
					   (HasPublicKey ? AssemblyNameFlags.PublicKey : AssemblyNameFlags.None);
			}
		}

		/// <summary>
		/// True if the assembly identity includes full public key.
		/// </summary>
		public bool HasPublicKey
		{
			get { return _publicKey.Length > 0; }
		}

		/// <summary>
		/// Full public key or empty.
		/// </summary>
		public byte[] PublicKey
		{
			get { return _publicKey; }
		}

		/// <summary>
		/// Low 8 bytes of SHA1 hash of the public key, or empty.
		/// </summary>
		public byte[] PublicKeyToken
		{
			get
			{
				if (_lazyPublicKeyToken == null)
				{
					_lazyPublicKeyToken = CalculatePublicKeyToken(_publicKey);
				}

				return _lazyPublicKeyToken;
			}
		}

		/// <summary>
		/// Gets the value which specifies if the assembly is retargetable. 
		/// </summary>
		public bool IsRetargetable
		{
			get { return _isRetargetable; }
		}

		public static bool IsFullName(AssemblyIdentityParts parts)
		{
			const AssemblyIdentityParts nvc = AssemblyIdentityParts.Name | AssemblyIdentityParts.Version | AssemblyIdentityParts.Culture;
			return (parts & nvc) == nvc && (parts & AssemblyIdentityParts.PublicKeyOrToken) != 0;
		}

		#region Equals, GetHashCode 

		/// <summary>
		/// Determines whether two <see cref="AssemblyIdentity"/> instances are equal.
		/// </summary>
		/// <param name="left">The operand appearing on the left side of the operator.</param>
		/// <param name="right">The operand appearing on the right side of the operator.</param>
		public static bool operator ==(AssemblyIdentity left, AssemblyIdentity right)
		{
			return EqualityComparer<AssemblyIdentity>.Default.Equals(left, right);
		}

		/// <summary>
		/// Determines whether two <see cref="AssemblyIdentity"/> instances are not equal.
		/// </summary>
		/// <param name="left">The operand appearing on the left side of the operator.</param>
		/// <param name="right">The operand appearing on the right side of the operator.</param>
		public static bool operator !=(AssemblyIdentity left, AssemblyIdentity right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Determines whether the specified instance is equal to the current instance.
		/// </summary>
		/// <param name="obj">The object to be compared with the current instance.</param>
		public bool Equals(AssemblyIdentity obj)
		{
			return !ReferenceEquals(obj, null)
				&& (_lazyHashCode == 0 || obj._lazyHashCode == 0 || _lazyHashCode == obj._lazyHashCode)
				&& MemberwiseEqual(this, obj) == true;
		}

		/// <summary>
		/// Determines whether the specified instance is equal to the current instance.
		/// </summary>
		/// <param name="obj">The object to be compared with the current instance.</param>
		public override bool Equals(object obj)
		{
			return Equals(obj as AssemblyIdentity);
		}

		/// <summary>
		/// Returns the hash code for the current instance.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			if (_lazyHashCode == 0)
			{
				// Do not include PK/PKT in the hash - collisions on PK/PKT are rare (assembly identities differ only in PKT/PK)
				// and we can't calculate hash of PKT if only PK is available
				_lazyHashCode =
					Hash.Combine(_name.GetHashCode(),
					Hash.Combine(_version.GetHashCode(), GetHashCodeIgnoringNameAndVersion()));
			}

			return _lazyHashCode;
		}

		public int GetHashCodeIgnoringNameAndVersion()
		{
			return Hash.Combine(_isRetargetable, _cultureName.GetHashCode());
		}

		// public for testing
		public static byte[] CalculatePublicKeyToken(byte[] publicKey)
		{
			var hash = Hash.ComputeHash(publicKey);

			// PublicKeyToken is the low 64 bits of the SHA-1 hash of the public key.
			int l = hash.Length - 1;
			var result = new List<byte>(PublicKeyTokenSize);
			for (int i = 0; i < PublicKeyTokenSize; i++)
			{
				result.Add(hash[l - i]);
			}

			return result.ToArray();
		}

		/// <summary>
		/// Returns true (false) if specified assembly identities are (not) equal 
		/// regardless of unification, retargeting or other assembly binding policies. 
		/// Returns null if these policies must be consulted to determine name equivalence.
		/// </summary>
		public static bool? MemberwiseEqual(AssemblyIdentity x, AssemblyIdentity y)
		{
			if (ReferenceEquals(x, y))
			{
				return true;
			}

			if (x._name != y._name)
			{
				return false;
			}

			if (x._version.Equals(y._version) && EqualIgnoringNameAndVersion(x, y))
			{
				return true;
			}

			return null;
		}

		public static bool EqualIgnoringNameAndVersion(AssemblyIdentity x, AssemblyIdentity y)
		{
			return
				x.IsRetargetable == y.IsRetargetable &&
				x.CultureName == y.CultureName &&
				KeysEqual(x, y);
		}

		public static bool KeysEqual(AssemblyIdentity x, AssemblyIdentity y)
		{
			var xToken = x._lazyPublicKeyToken;
			var yToken = y._lazyPublicKeyToken;

			// weak names or both strong names with initialized PKT - compare tokens:
			if (xToken != null && yToken != null)
			{
				return xToken.SequenceEqual(yToken);
			}

			// both are strong names with uninitialized PKT - compare full keys:
			if (xToken == null && yToken == null)
			{
				return x._publicKey.SequenceEqual(y._publicKey);
			}

			// one of the strong names doesn't have PK, other doesn't have PTK initialized.
			if (xToken == null)
			{
				return x.PublicKeyToken.SequenceEqual(yToken);
			}
			else
			{
				return xToken.SequenceEqual(y.PublicKeyToken);
			}
		}

		#endregion

		#region AssemblyName conversions 

		/// <summary>
		/// Retrieves assembly definition identity from given runtime assembly.
		/// </summary>
		/// <param name="assembly">The runtime assembly.</param>
		/// <returns>Assembly definition identity.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is null.</exception>
		public static AssemblyIdentity FromAssemblyDefinition(Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			return FromAssemblyDefinition(assembly.GetName());
		}

		// public for testing
		public static AssemblyIdentity FromAssemblyDefinition(AssemblyName name)
		{
			// AssemblyDef always has full key or no key:
			var publicKeyBytes = name.GetPublicKey();
			byte[] publicKey = publicKeyBytes;

			return new AssemblyIdentity(
				name.Name,
				name.Version,
				name.CultureInfo.Name,
				publicKey,
				(name.Flags & AssemblyNameFlags.Retargetable) != 0);
		}

		public static AssemblyIdentity FromAssemblyReference(AssemblyName name)
		{
			// AssemblyRef either has PKT or no key:
			return new AssemblyIdentity(
				name.Name,
				name.Version,
				name.CultureInfo.Name,
				null,
				isRetargetable: (name.Flags & AssemblyNameFlags.Retargetable) != 0);
		}

		#endregion

		public const string InvariantCultureDisplay = "neutral";

		/// <summary>
		/// Returns the display name of the assembly identity.
		/// </summary>
		/// <param name="fullKey">True if the full public key should be included in the name. Otherwise public key token is used.</param>
		/// <returns>The display name.</returns>
		/// <remarks>
		/// Characters ',', '=', '"', '\'', '\' occurring in the simple name are escaped by backslash in the display name.
		/// Any character '\t' is replaced by two characters '\' and 't',
		/// Any character '\n' is replaced by two characters '\' and 'n',
		/// Any character '\r' is replaced by two characters '\' and 'r',
		/// The assembly name in the display name is enclosed in double quotes if it starts or ends with 
		/// a whitespace character (' ', '\t', '\r', '\n').
		/// </remarks>
		public string GetDisplayName(bool fullKey = false)
		{
			if (fullKey)
			{
				return BuildDisplayName(fullKey: true);
			}

			if (_lazyDisplayName == null)
			{
				_lazyDisplayName = BuildDisplayName(fullKey: false);
			}

			return _lazyDisplayName;
		}

		/// <summary>
		/// Returns the display name of the current instance.
		/// </summary>
		public override string ToString()
		{
			return GetDisplayName(fullKey: false);
		}

		public static string PublicKeyToString(byte[] key)
		{
			if (key == null || key.Length == 0)
			{
				return "";
			}

			PooledStringBuilder sb = PooledStringBuilder.GetInstance();
			StringBuilder builder = sb.Builder;
			AppendKey(sb, key);
			return sb.ToStringAndFree();
		}

		private string BuildDisplayName(bool fullKey)
		{
			PooledStringBuilder pooledBuilder = PooledStringBuilder.GetInstance();
			var sb = pooledBuilder.Builder;
			EscapeName(sb, Name);

			sb.Append(", Version=");
			sb.Append(_version.Major);
			sb.Append(".");
			sb.Append(_version.Minor);
			sb.Append(".");
			sb.Append(_version.Build);
			sb.Append(".");
			sb.Append(_version.Revision);

			sb.Append(", Culture=");
			if (_cultureName.Length == 0)
			{
				sb.Append(InvariantCultureDisplay);
			}
			else
			{
				EscapeName(sb, _cultureName);
			}

			if (fullKey && HasPublicKey)
			{
				sb.Append(", PublicKey=");
				AppendKey(sb, _publicKey);
			}
			else
			{
				sb.Append(", PublicKeyToken=");
				if (PublicKeyToken.Length > 0)
				{
					AppendKey(sb, PublicKeyToken);
				}
				else
				{
					sb.Append("null");
				}
			}

			if (IsRetargetable)
			{
				sb.Append(", Retargetable=Yes");
			}
			string result = sb.ToString();
			pooledBuilder.Free();
			return result;
		}

		private static void AppendKey(StringBuilder sb, byte[] key)
		{
			foreach (byte b in key)
			{
				sb.Append(b.ToString("x2"));
			}
		}

		private string GetDebuggerDisplay()
		{
			return GetDisplayName(fullKey: true);
		}

		public static bool TryParseDisplayName(string displayName, out AssemblyIdentity identity)
		{
			if (displayName == null)
			{
				throw new ArgumentNullException(nameof(displayName));
			}

			AssemblyIdentityParts parts;
			return TryParseDisplayName(displayName, out identity, out parts);
		}

		/// <summary>
		/// Parses display name filling defaults for any basic properties that are missing.
		/// </summary>
		/// <param name="displayName">Display name.</param>
		/// <param name="identity">A full assembly identity.</param>
		/// <param name="parts">
		/// Parts of the assembly identity that were specified in the display name, 
		/// or 0 if the parsing failed.
		/// </param>
		/// <returns>True if display name parsed correctly.</returns>
		/// <remarks>
		/// The simple name has to be non-empty.
		/// A partially specified version might be missing build and/or revision number. The default value for these is 65535.
		/// The default culture is neutral (<see cref="CultureName"/> is <see cref="String.Empty"/>.
		/// If neither public key nor token is specified the identity is considered weak.
		/// </remarks>
		/// <exception cref="ArgumentNullException"><paramref name="displayName"/> is null.</exception>
		public static bool TryParseDisplayName(string displayName, out AssemblyIdentity identity, out AssemblyIdentityParts parts)
		{
			// see ndp\clr\src\Binder\TextualIdentityParser.cpp, ndp\clr\src\Binder\StringLexer.cpp

			identity = null;
			parts = 0;

			if (displayName == null)
			{
				throw new ArgumentNullException(nameof(displayName));
			}

			if (displayName.IndexOf('\0') >= 0)
			{
				return false;
			}

			int position = 0;
			string simpleName;
			if (!TryParseNameToken(displayName, ref position, out simpleName))
			{
				return false;
			}

			var parsedParts = AssemblyIdentityParts.Name;
			var seen = AssemblyIdentityParts.Name;

			Version version = null;
			string culture = null;
			bool isRetargetable = false;
			byte[] publicKey = null;
			byte[] publicKeyToken = null;

			while (position < displayName.Length)
			{
				// Parse ',' name '=' value
				if (displayName[position] != ',')
				{
					return false;
				}

				position++;

				string propertyName;
				if (!TryParseNameToken(displayName, ref position, out propertyName))
				{
					return false;
				}

				if (position >= displayName.Length || displayName[position] != '=')
				{
					return false;
				}

				position++;

				string propertyValue;
				if (!TryParseNameToken(displayName, ref position, out propertyValue))
				{
					return false;
				}

				// Process property
				if (string.Equals(propertyName, "Version", StringComparison.OrdinalIgnoreCase))
				{
					if ((seen & AssemblyIdentityParts.Version) != 0)
					{
						return false;
					}

					seen |= AssemblyIdentityParts.Version;

					if (propertyValue == "*")
					{
						continue;
					}

					ulong versionLong;
					AssemblyIdentityParts versionParts;
					if (!TryParseVersion(propertyValue, out versionLong, out versionParts))
					{
						return false;
					}

					version = ToVersion(versionLong);
					parsedParts |= versionParts;
				}
				else if (string.Equals(propertyName, "Culture", StringComparison.OrdinalIgnoreCase) ||
						 string.Equals(propertyName, "Language", StringComparison.OrdinalIgnoreCase))
				{
					if ((seen & AssemblyIdentityParts.Culture) != 0)
					{
						return false;
					}

					seen |= AssemblyIdentityParts.Culture;

					if (propertyValue == "*")
					{
						continue;
					}

					culture = string.Equals(propertyValue, InvariantCultureDisplay, StringComparison.OrdinalIgnoreCase) ? null : propertyValue;
					parsedParts |= AssemblyIdentityParts.Culture;
				}
				else if (string.Equals(propertyName, "PublicKey", StringComparison.OrdinalIgnoreCase))
				{
					if ((seen & AssemblyIdentityParts.PublicKey) != 0)
					{
						return false;
					}

					seen |= AssemblyIdentityParts.PublicKey;

					if (propertyValue == "*")
					{
						continue;
					}

					byte[] value;
					if (!TryParsePublicKey(propertyValue, out value))
					{
						return false;
					}

					// NOTE: Fusion would also set the public key token (as derived from the public key) here.
					//       We may need to do this as well for error cases, as Fusion would fail to parse the
					//       assembly name if public key token calculation failed.

					publicKey = value;
					parsedParts |= AssemblyIdentityParts.PublicKey;
				}
				else if (string.Equals(propertyName, "PublicKeyToken", StringComparison.OrdinalIgnoreCase))
				{
					if ((seen & AssemblyIdentityParts.PublicKeyToken) != 0)
					{
						return false;
					}

					seen |= AssemblyIdentityParts.PublicKeyToken;

					if (propertyValue == "*")
					{
						continue;
					}

					byte[] value;
					if (!TryParsePublicKeyToken(propertyValue, out value))
					{
						return false;
					}

					publicKeyToken = value;
					parsedParts |= AssemblyIdentityParts.PublicKeyToken;
				}
				else if (string.Equals(propertyName, "Retargetable", StringComparison.OrdinalIgnoreCase))
				{
					if ((seen & AssemblyIdentityParts.Retargetability) != 0)
					{
						return false;
					}

					seen |= AssemblyIdentityParts.Retargetability;

					if (propertyValue == "*")
					{
						continue;
					}

					if (string.Equals(propertyValue, "Yes", StringComparison.OrdinalIgnoreCase))
					{
						isRetargetable = true;
					}
					else if (string.Equals(propertyValue, "No", StringComparison.OrdinalIgnoreCase))
					{
						isRetargetable = false;
					}
					else
					{
						return false;
					}

					parsedParts |= AssemblyIdentityParts.Retargetability;
				}
				else if (string.Equals(propertyName, "ContentType", StringComparison.OrdinalIgnoreCase))
				{
					if ((seen & AssemblyIdentityParts.ContentType) != 0)
					{
						return false;
					}

					seen |= AssemblyIdentityParts.ContentType;

					if (propertyValue == "*")
					{
						continue;
					}

					parsedParts |= AssemblyIdentityParts.ContentType;
				}
				else
				{
					parsedParts |= AssemblyIdentityParts.Unknown;
				}
			}

			// incompatible values:
			if (isRetargetable)
			{
				return false;
			}

			bool hasPublicKey = publicKey != null;
			bool hasPublicKeyToken = publicKeyToken != null;

			identity = new AssemblyIdentity(simpleName, version, culture, hasPublicKey ? publicKey : publicKeyToken, isRetargetable);

			if (hasPublicKey && hasPublicKeyToken && !identity.PublicKeyToken.SequenceEqual(publicKeyToken))
			{
				identity = null;
				return false;
			}

			parts = parsedParts;
			return true;
		}

		private static bool TryParseNameToken(string displayName, ref int position, out string value)
		{
			Debug.Assert(displayName.IndexOf('\0') == -1);

			int i = position;

			// skip leading whitespace:
			while (true)
			{
				if (i == displayName.Length)
				{
					value = null;
					return false;
				}
				else if (!IsWhiteSpace(displayName[i]))
				{
					break;
				}

				i++;
			}

			char quote;
			if (IsQuote(displayName[i]))
			{
				quote = displayName[i++];
			}
			else
			{
				quote = '\0';
			}

			int valueStart = i;
			int valueEnd = displayName.Length;
			bool containsEscapes = false;

			while (true)
			{
				if (i >= displayName.Length)
				{
					i = displayName.Length;
					break;
				}

				char c = displayName[i];
				if (c == '\\')
				{
					containsEscapes = true;
					i += 2;
					continue;
				}

				if (quote == '\0')
				{
					if (IsNameTokenTerminator(c))
					{
						break;
					}
					else if (IsQuote(c))
					{
						value = null;
						return false;
					}
				}
				else if (c == quote)
				{
					valueEnd = i;
					i++;
					break;
				}

				i++;
			}

			if (quote == '\0')
			{
				int j = i - 1;
				while (j >= valueStart && IsWhiteSpace(displayName[j]))
				{
					j--;
				}

				valueEnd = j + 1;
			}
			else
			{
				// skip any whitespace following the quote and check for the terminator
				while (i < displayName.Length)
				{
					char c = displayName[i];
					if (!IsWhiteSpace(c))
					{
						if (!IsNameTokenTerminator(c))
						{
							value = null;
							return false;
						}
						break;
					}

					i++;
				}
			}

			Debug.Assert(i == displayName.Length || IsNameTokenTerminator(displayName[i]));
			position = i;

			// empty
			if (valueEnd == valueStart)
			{
				value = null;
				return false;
			}

			if (!containsEscapes)
			{
				value = displayName.Substring(valueStart, valueEnd - valueStart);
				return true;
			}
			else
			{
				return TryUnescape(displayName, valueStart, valueEnd, out value);
			}
		}

		private static bool IsNameTokenTerminator(char c)
		{
			return c == '=' || c == ',';
		}

		private static bool IsQuote(char c)
		{
			return c == '"' || c == '\'';
		}

		public static Version ToVersion(ulong version)
		{
			return new Version(
				unchecked((ushort)(version >> 48)),
				unchecked((ushort)(version >> 32)),
				unchecked((ushort)(version >> 16)),
				unchecked((ushort)version));
		}

		// public for testing
		// Parses version format: 
		//   [version-part]{[.][version-part], 3}
		// Where version part is
		//   [*]|[0-9]*
		// The number of dots in the version determines the present parts, i.e.
		//   "1..2" parses as "1.0.2.0" with Major, Minor and Build parts.
		//   "1.*" parses as "1.0.0.0" with Major and Minor parts.
		public static bool TryParseVersion(string str, out ulong result, out AssemblyIdentityParts parts)
		{
			Debug.Assert(str.Length > 0);
			Debug.Assert(str.IndexOf('\0') < 0);

			const int MaxVersionParts = 4;
			const int BitsPerVersionPart = 16;

			parts = 0;
			result = 0;
			int partOffset = BitsPerVersionPart * (MaxVersionParts - 1);
			int partIndex = 0;
			int partValue = 0;
			bool partHasValue = false;
			bool partHasWildcard = false;

			int i = 0;
			while (true)
			{
				char c = (i < str.Length) ? str[i++] : '\0';

				if (c == '.' || c == 0)
				{
					if (partIndex == MaxVersionParts || partHasValue && partHasWildcard)
					{
						return false;
					}

					result |= ((ulong)partValue) << partOffset;

					if (partHasValue || partHasWildcard)
					{
						parts |= (AssemblyIdentityParts)((int)AssemblyIdentityParts.VersionMajor << partIndex);
					}

					if (c == 0)
					{
						return true;
					}

					// next part:
					partValue = 0;
					partOffset -= BitsPerVersionPart;
					partIndex++;
					partHasWildcard = partHasValue = false;
				}
				else if (c >= '0' && c <= '9')
				{
					partHasValue = true;
					partValue = partValue * 10 + c - '0';
					if (partValue > ushort.MaxValue)
					{
						return false;
					}
				}
				else if (c == '*')
				{
					partHasWildcard = true;
				}
				else
				{
					return false;
				}
			}
		}

		private static bool TryParsePublicKey(string value, out byte[] key)
		{
			if (!TryParseHexBytes(value, out key))
			{
				key = default(byte[]);
				return false;
			}

			return true;
		}

		private const int PublicKeyTokenBytes = 8;

		private static bool TryParsePublicKeyToken(string value, out byte[] token)
		{
			if (string.Equals(value, "null", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(value, "neutral", StringComparison.OrdinalIgnoreCase))
			{
				token = new byte[0];
				return true;
			}

			byte[] result;
			if (value.Length != (PublicKeyTokenBytes * 2) || !TryParseHexBytes(value, out result))
			{
				token = default(byte[]);
				return false;
			}

			token = result;
			return true;
		}

		private static bool TryParseHexBytes(string value, out byte[] result)
		{
			if (value.Length == 0 || (value.Length % 2) != 0)
			{
				result = default(byte[]);
				return false;
			}

			var length = value.Length / 2;
			var bytes = new List<byte>(length);
			for (int i = 0; i < length; i++)
			{
				int hi = HexValue(value[i * 2]);
				int lo = HexValue(value[i * 2 + 1]);

				if (hi < 0 || lo < 0)
				{
					result = default(byte[]);
					return false;
				}

				bytes.Add((byte)((hi << 4) | lo));
			}

			result = bytes.ToArray();
			return true;
		}

		public static int HexValue(char c)
		{
			if (c >= '0' && c <= '9')
			{
				return c - '0';
			}

			if (c >= 'a' && c <= 'f')
			{
				return c - 'a' + 10;
			}

			if (c >= 'A' && c <= 'F')
			{
				return c - 'A' + 10;
			}

			return -1;
		}

		private static bool IsWhiteSpace(char c)
		{
			return c == ' ' || c == '\t' || c == '\r' || c == '\n';
		}

		private static void EscapeName(StringBuilder result, string name)
		{
			bool quoted = false;
			if (IsWhiteSpace(name[0]) || IsWhiteSpace(name[name.Length - 1]))
			{
				result.Append('"');
				quoted = true;
			}

			for (int i = 0; i < name.Length; i++)
			{
				char c = name[i];
				switch (c)
				{
					case ',':
					case '=':
					case '\\':
					case '"':
					case '\'':
						result.Append('\\');
						result.Append(c);
						break;

					case '\t':
						result.Append("\\t");
						break;

					case '\r':
						result.Append("\\r");
						break;

					case '\n':
						result.Append("\\n");
						break;

					default:
						result.Append(c);
						break;
				}
			}

			if (quoted)
			{
				result.Append('"');
			}
		}

		private static bool TryUnescape(string str, int start, int end, out string value)
		{
			var sb = PooledStringBuilder.GetInstance();

			int i = start;
			while (i < end)
			{
				char c = str[i++];
				if (c == '\\')
				{
					if (!Unescape(sb.Builder, str, ref i))
					{
						value = null;
						return false;
					}
				}
				else
				{
					sb.Builder.Append(c);
				}
			}

			value = sb.ToStringAndFree();
			return true;
		}

		private static bool Unescape(StringBuilder sb, string str, ref int i)
		{
			if (i == str.Length)
			{
				return false;
			}

			char c = str[i++];
			switch (c)
			{
				case ',':
				case '=':
				case '\\':
				case '/':
				case '"':
				case '\'':
					sb.Append(c);
					return true;

				case 't':
					sb.Append("\t");
					return true;

				case 'n':
					sb.Append("\n");
					return true;

				case 'r':
					sb.Append("\r");
					return true;

				case 'u':
					int semicolon = str.IndexOf(';', i);
					if (semicolon == -1)
					{
						return false;
					}

					try
					{
						int codepoint = Convert.ToInt32(str.Substring(i, semicolon - i), 16);

						// \0 is not valid in an assembly name
						if (codepoint == 0)
						{
							return false;
						}

						sb.Append(char.ConvertFromUtf32(codepoint));
					}
					catch
					{
						return false;
					}

					i = semicolon + 1;
					return true;

				default:
					return false;
			}
		}
	}
}
