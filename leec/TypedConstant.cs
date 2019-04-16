using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public enum TypedConstantKind
	{
		Error = 0, // error should be the default so that default(TypedConstant) is internally consistent
		Primitive = 1,
		Enum = 2,
		Type = 3,
		Array = 4
	}

	public struct TypedConstant : IEquatable<TypedConstant>
	{
		private readonly TypedConstantKind _kind;
		private readonly ITypeSymbol _type;
		private readonly object _value;

		public TypedConstant(ITypeSymbol type, TypedConstantKind kind, object value)
		{
			Debug.Assert(kind == TypedConstantKind.Array || !(value is TypedConstant[]));
			_kind = kind;
			_type = type;
			_value = value;
		}

		public TypedConstant(ITypeSymbol type, TypedConstant[] array)
			: this(type, TypedConstantKind.Array, (object)array)
		{
		}

		/// <summary>
		/// The kind of the constant.
		/// </summary>
		public TypedConstantKind Kind
		{
			get { return _kind; }
		}

		/// <summary>
		/// Returns the <see cref="ITypeSymbol"/> of the constant, 
		/// or null if the type can't be determined (error).
		/// </summary>
		public ITypeSymbol Type
		{
			get { return _type; }
		}

		/// <summary>
		/// True if the constant represents a null reference.
		/// </summary>
		public bool IsNull
		{
			get
			{
				return _value == null;
			}
		}

		/// <summary>
		/// The value for a non-array constant.
		/// </summary>
		public object Value
		{
			get
			{
				if (Kind == TypedConstantKind.Array)
				{
					throw new InvalidOperationException("TypedConstant is an array. Use Values property.");
				}

				return _value;
			}
		}

		/// <summary>
		/// The value for a <see cref="TypedConstant"/> array. 
		/// </summary>
		public TypedConstant[] Values
		{
			get
			{
				if (Kind != TypedConstantKind.Array)
				{
					throw new InvalidOperationException("TypedConstant is not an array. Use Value property.");
				}

				if (this.IsNull)
				{
					return default(TypedConstant[]);
				}

				return (TypedConstant[])_value;
			}
		}

		public T DecodeValue<T>(SpecialType specialType)
		{
			if (_kind == TypedConstantKind.Error)
			{
				return default(T);
			}

			if (_type.SpecialType == specialType || (_type.TypeKind == TypeKind.Enum && specialType == SpecialType.System_Enum))
			{
				return (T)_value;
			}

			// the actual argument type doesn't match the type of the parameter - an error has already been reported by the binder
			return default(T);
		}

		/// <remarks>
		/// TypedConstant isn't computing its own kind from the type symbol because it doesn't
		/// have a way to recognize the well-known type System.Type.
		/// </remarks>
		public static TypedConstantKind GetTypedConstantKind(ITypeSymbol type, Compilation compilation)
		{
			Debug.Assert(type != null);

			switch (type.SpecialType)
			{
				case SpecialType.System_Boolean:
				case SpecialType.System_SByte:
				case SpecialType.System_Int16:
				case SpecialType.System_Int32:
				case SpecialType.System_Int64:
				case SpecialType.System_Byte:
				case SpecialType.System_UInt16:
				case SpecialType.System_UInt32:
				case SpecialType.System_UInt64:
				case SpecialType.System_Single:
				case SpecialType.System_Double:
				case SpecialType.System_Char:
				case SpecialType.System_String:
				case SpecialType.System_Object:
					return TypedConstantKind.Primitive;
				default:
					switch (type.TypeKind)
					{
						case TypeKind.Array:
							return TypedConstantKind.Array;
						case TypeKind.Enum:
							return TypedConstantKind.Enum;
						case TypeKind.Error:
							return TypedConstantKind.Error;
					}

					if (compilation != null &&
						compilation.IsSystemTypeReference(type))
					{
						return TypedConstantKind.Type;
					}

					return TypedConstantKind.Error;
			}
		}

		public override bool Equals(object obj)
		{
			return obj is TypedConstant && Equals((TypedConstant)obj);
		}

		public bool Equals(TypedConstant other)
		{
			return _kind == other._kind
				&& object.Equals(_value, other._value)
				&& object.Equals(_type, other._type);
		}

		public override int GetHashCode()
		{
			return Hash.Combine(_value,
				   Hash.Combine(_type, (int)this.Kind));
		}
	}
}
