using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public struct AttributeUsageInfo : IEquatable<AttributeUsageInfo>
	{
		[Flags()]
		private enum PackedAttributeUsage
		{
			None = 0,
			Assembly = AttributeTargets.Assembly,
			Module = AttributeTargets.Module,
			Class = AttributeTargets.Class,
			Struct = AttributeTargets.Struct,
			Enum = AttributeTargets.Enum,
			Constructor = AttributeTargets.Constructor,
			Method = AttributeTargets.Method,
			Property = AttributeTargets.Property,
			Field = AttributeTargets.Field,
			Event = AttributeTargets.Event,
			Interface = AttributeTargets.Interface,
			Parameter = AttributeTargets.Parameter,
			Delegate = AttributeTargets.Delegate,
			ReturnValue = AttributeTargets.ReturnValue,
			GenericParameter = AttributeTargets.GenericParameter,
			All = AttributeTargets.All,

			// NOTE: VB allows AttributeUsageAttribute with no valid target, i.e. <AttributeUsageAttribute(0)>, and doesn't generate any diagnostics.
			// We use PackedAttributeUsage.Initialized field to differentiate between uninitialized AttributeUsageInfo and initialized AttributeUsageInfo with no valid target.
			Initialized = GenericParameter << 1,

			AllowMultiple = Initialized << 1,
			Inherited = AllowMultiple << 1
		}

		private readonly PackedAttributeUsage _flags;

		/// <summary>
		/// Default attribute usage for attribute types:
		/// (a) Valid targets: AttributeTargets.All
		/// (b) AllowMultiple: false
		/// (c) Inherited: true
		/// </summary>
		static public readonly AttributeUsageInfo Default = new AttributeUsageInfo(validTargets: AttributeTargets.All, allowMultiple: false, inherited: true);

		static public readonly AttributeUsageInfo Null = default(AttributeUsageInfo);

		public AttributeUsageInfo(AttributeTargets validTargets, bool allowMultiple, bool inherited)
		{
			// NOTE: VB allows AttributeUsageAttribute with no valid target, i.e. <AttributeUsageAttribute(0)>, and doesn't generate any diagnostics.
			// We use PackedAttributeUsage.Initialized field to differentiate between uninitialized AttributeUsageInfo and initialized AttributeUsageInfo with no valid targets.
			_flags = (PackedAttributeUsage)validTargets | PackedAttributeUsage.Initialized;

			if (allowMultiple)
			{
				_flags |= PackedAttributeUsage.AllowMultiple;
			}

			if (inherited)
			{
				_flags |= PackedAttributeUsage.Inherited;
			}
		}

		public bool IsNull
		{
			get
			{
				return (_flags & PackedAttributeUsage.Initialized) == 0;
			}
		}


		public AttributeTargets ValidTargets
		{
			get
			{
				return (AttributeTargets)(_flags & PackedAttributeUsage.All);
			}
		}

		public bool AllowMultiple
		{
			get
			{
				return (_flags & PackedAttributeUsage.AllowMultiple) != 0;
			}
		}

		public bool Inherited
		{
			get
			{
				return (_flags & PackedAttributeUsage.Inherited) != 0;
			}
		}

		public static bool operator ==(AttributeUsageInfo left, AttributeUsageInfo right)
		{
			return left._flags == right._flags;
		}

		public static bool operator !=(AttributeUsageInfo left, AttributeUsageInfo right)
		{
			return left._flags != right._flags;
		}

		public override bool Equals(object obj)
		{
			if (obj is AttributeUsageInfo)
			{
				return this.Equals((AttributeUsageInfo)obj);
			}

			return false;
		}

		public bool Equals(AttributeUsageInfo other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return _flags.GetHashCode();
		}

		public bool HasValidAttributeTargets
		{
			get
			{
				var value = (int)ValidTargets;
				return value != 0 && (value & (int)~AttributeTargets.All) == 0;
			}
		}

		public object GetValidTargetsErrorArgument()
		{
			var validTargetsInt = (int)ValidTargets;
			if (!HasValidAttributeTargets)
			{
				return string.Empty;
			}

			var builder = new List<string>();
			int flag = 0;
			while (validTargetsInt > 0)
			{
				if ((validTargetsInt & 1) != 0)
				{
					builder.Add(GetErrorDisplayNameResourceId((AttributeTargets)(1 << flag)));
				}

				validTargetsInt >>= 1;
				flag++;
			}

			return new ValidTargetsStringLocalizableErrorArgument(builder.ToArray());
		}

		private struct ValidTargetsStringLocalizableErrorArgument : IFormattable
		{
			private readonly string[] _targetResourceIds;

			public ValidTargetsStringLocalizableErrorArgument(string[] targetResourceIds)
			{
				Debug.Assert(targetResourceIds != null);
				_targetResourceIds = targetResourceIds;
			}

			public override string ToString()
			{
				return ToString(null, null);
			}

			public string ToString(string format, IFormatProvider formatProvider)
			{
				var builder = PooledStringBuilder.GetInstance();
				var culture = formatProvider as System.Globalization.CultureInfo;

				if (_targetResourceIds != null)
				{
					foreach (string id in _targetResourceIds)
					{
						if (builder.Builder.Length > 0)
						{
							builder.Builder.Append(", ");
						}

						throw new Exception("@!!");
						//builder.Builder.Append(CodeAnalysisResources.ResourceManager.GetString(id, culture));
					}
				}

				var message = builder.Builder.ToString();
				builder.Free();

				return message;
			}
		}

		private static string GetErrorDisplayNameResourceId(AttributeTargets target)
		{
			switch (target)
			{
				case AttributeTargets.Assembly: return "Assembly";
				case AttributeTargets.Class: return "Class1";
				case AttributeTargets.Constructor: return "Constructor";
				case AttributeTargets.Delegate: return "Delegate1";
				case AttributeTargets.Enum: return "Enum1";
				case AttributeTargets.Event: return "Event1";
				case AttributeTargets.Field: return "Field";
				case AttributeTargets.GenericParameter: return "TypeParameter";
				case AttributeTargets.Interface: return "Interface1";
				case AttributeTargets.Method: return "Method";
				case AttributeTargets.Module: return "Module";
				case AttributeTargets.Parameter: return "Parameter";
				case AttributeTargets.Property: return "Property";
				case AttributeTargets.ReturnValue: return "Return1";
				case AttributeTargets.Struct: return "Struct1";
				default:
					throw ExceptionUtilities.UnexpectedValue(target);
			}
		}
	}
}
