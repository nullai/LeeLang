using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public class ArrayMethods
	{
		// There are four kinds of array pseudo-methods
		// They are specific to a given array type
		private enum ArrayMethodKind : byte
		{
			GET,
			SET,
			ADDRESS,
			CTOR,
		}

		/// <summary>
		/// Acquires an array constructor for a given array type
		/// </summary>
		public ArrayMethod GetArrayConstructor(IArrayTypeReference arrayType)
		{
			return GetArrayMethod(arrayType, ArrayMethodKind.CTOR);
		}

		/// <summary>
		/// Acquires an element getter method for a given array type
		/// </summary>
		public ArrayMethod GetArrayGet(IArrayTypeReference arrayType)
			=> GetArrayMethod(arrayType, ArrayMethodKind.GET);

		/// <summary>
		/// Acquires an element setter method for a given array type
		/// </summary>
		public ArrayMethod GetArraySet(IArrayTypeReference arrayType)
			=> GetArrayMethod(arrayType, ArrayMethodKind.SET);

		/// <summary>
		/// Acquires an element referencer method for a given array type
		/// </summary>
		public ArrayMethod GetArrayAddress(IArrayTypeReference arrayType)
			=> GetArrayMethod(arrayType, ArrayMethodKind.ADDRESS);

		/// <summary>
		/// Maps {array type, method kind} tuples to implementing pseudo-methods.
		/// </summary>
		private readonly Dictionary<KeyValuePair<byte, IArrayTypeReference>, ArrayMethod> _dict =
			new Dictionary<KeyValuePair<byte, IArrayTypeReference>, ArrayMethod>();

		/// <summary>
		/// lazily fetches or creates a new array method.
		/// </summary>
		private ArrayMethod GetArrayMethod(IArrayTypeReference arrayType, ArrayMethodKind id)
		{
			var key = new KeyValuePair<byte, IArrayTypeReference>((byte)id, arrayType);
			ArrayMethod result;

			var dict = _dict;
			if (!dict.TryGetValue(key, out result))
			{
				result = MakeArrayMethod(arrayType, id);
				if (dict.ContainsKey(key))
					result = dict[key];
				else
					dict.Add(key, result);
			}

			return result;
		}

		private static ArrayMethod MakeArrayMethod(IArrayTypeReference arrayType, ArrayMethodKind id)
		{
			switch (id)
			{
				case ArrayMethodKind.CTOR:
					return new ArrayConstructor(arrayType);

				case ArrayMethodKind.GET:
					return new ArrayGet(arrayType);

				case ArrayMethodKind.SET:
					return new ArraySet(arrayType);

				case ArrayMethodKind.ADDRESS:
					return new ArrayAddress(arrayType);
			}

			throw ExceptionUtilities.UnexpectedValue(id);
		}


		/// <summary>
		/// "newobj ArrayConstructor"  is equivalent of "newarr ElementType" 
		/// when working with multidimensional arrays
		/// </summary>
		private sealed class ArrayConstructor : ArrayMethod
		{
			public ArrayConstructor(IArrayTypeReference arrayType) : base(arrayType) { }

			public override string Name => ".ctor";

			public override ITypeReference GetType(EmitContext context)
				=> context.Module.GetPlatformType(PlatformType.SystemVoid, context);
		}

		/// <summary>
		/// "call ArrayGet"  is equivalent of "ldelem ElementType" 
		/// when working with multidimensional arrays
		/// </summary>
		private sealed class ArrayGet : ArrayMethod
		{
			public ArrayGet(IArrayTypeReference arrayType) : base(arrayType) { }

			public override string Name => "Get";

			public override ITypeReference GetType(EmitContext context)
				=> arrayType.GetElementType(context);
		}

		/// <summary>
		/// "call ArrayAddress"  is equivalent of "ldelema ElementType" 
		/// when working with multidimensional arrays
		/// </summary>
		private sealed class ArrayAddress : ArrayMethod
		{
			public ArrayAddress(IArrayTypeReference arrayType) : base(arrayType) { }

			public override bool ReturnValueIsByRef => true;

			public override ITypeReference GetType(EmitContext context)
				=> arrayType.GetElementType(context);

			public override string Name => "Address";
		}

		/// <summary>
		/// "call ArraySet"  is equivalent of "stelem ElementType" 
		/// when working with multidimensional arrays
		/// </summary>
		private sealed class ArraySet : ArrayMethod
		{
			public ArraySet(IArrayTypeReference arrayType) : base(arrayType) { }

			public override string Name => "Set";

			public override ITypeReference GetType(EmitContext context)
				=> context.Module.GetPlatformType(PlatformType.SystemVoid, context);

			protected override ArrayMethodParameterInfo[] MakeParameters()
			{
				int rank = (int)arrayType.Rank;
				var parameters = new List<ArrayMethodParameterInfo>(rank + 1);

				for (int i = 0; i < rank; i++)
				{
					parameters.Add(ArrayMethodParameterInfo.GetIndexParameter((ushort)i));
				}

				parameters.Add(new ArraySetValueParameterInfo((ushort)rank, arrayType));
				return parameters.ToArray();
			}
		}
	}

	/// <summary>
	/// Represents a parameter in an array pseudo-method.
	/// 
	/// NOTE: It appears that only number of indices is used for verification, 
	/// types just have to be Int32.
	/// Even though actual arguments can be native ints.
	/// </summary>
	public class ArrayMethodParameterInfo : IParameterTypeInformation
	{
		// position in the signature
		private readonly ushort _index;

		// cache common parameter instances 
		// (we can do this since the only data we have is the index)
		private static readonly ArrayMethodParameterInfo s_index0 = new ArrayMethodParameterInfo(0);
		private static readonly ArrayMethodParameterInfo s_index1 = new ArrayMethodParameterInfo(1);
		private static readonly ArrayMethodParameterInfo s_index2 = new ArrayMethodParameterInfo(2);
		private static readonly ArrayMethodParameterInfo s_index3 = new ArrayMethodParameterInfo(3);

		protected ArrayMethodParameterInfo(ushort index)
		{
			_index = index;
		}

		public static ArrayMethodParameterInfo GetIndexParameter(ushort index)
		{
			switch (index)
			{
				case 0: return s_index0;
				case 1: return s_index1;
				case 2: return s_index2;
				case 3: return s_index3;
			}

			return new ArrayMethodParameterInfo(index);
		}

		public ICustomModifier[] RefCustomModifiers
			=> new ICustomModifier[0];

		public ICustomModifier[] CustomModifiers
			=> new ICustomModifier[0];

		public bool IsByReference => false;

		public virtual ITypeReference GetType(EmitContext context)
			=> context.Module.GetPlatformType(PlatformType.SystemInt32, context);

		public ushort Index => _index;
	}

	/// <summary>
	/// Represents the "value" parameter of the Set pseudo-method.
	/// 
	/// NOTE: unlike index parameters, type of the value parameter must match 
	/// the actual element type.
	/// </summary>
	internal sealed class ArraySetValueParameterInfo : ArrayMethodParameterInfo
	{
		private readonly IArrayTypeReference _arrayType;

		internal ArraySetValueParameterInfo(ushort index, IArrayTypeReference arrayType)
			: base(index)
		{
			_arrayType = arrayType;
		}

		public override ITypeReference GetType(EmitContext context)
			=> _arrayType.GetElementType(context);
	}

	/// <summary>
	/// Base of all array methods. They have a lot in common.
	/// </summary>
	public abstract class ArrayMethod : IMethodReference
	{
		private readonly ArrayMethodParameterInfo[] _parameters;
		protected readonly IArrayTypeReference arrayType;

		protected ArrayMethod(IArrayTypeReference arrayType)
		{
			this.arrayType = arrayType;
			_parameters = MakeParameters();
		}

		public abstract string Name { get; }
		public abstract ITypeReference GetType(EmitContext context);

		// Address overrides this to "true"
		public virtual bool ReturnValueIsByRef => false;

		// Set overrides this to include "value" parameter.
		protected virtual ArrayMethodParameterInfo[] MakeParameters()
		{
			int rank = (int)arrayType.Rank;
			var parameters = new List<ArrayMethodParameterInfo>(rank);

			for (int i = 0; i < rank; i++)
			{
				parameters.Add(ArrayMethodParameterInfo.GetIndexParameter((ushort)i));
			}

			return parameters.ToArray();
		}

		public IParameterTypeInformation[] GetParameters(EmitContext context)
			=> StaticCast<IParameterTypeInformation>.From(_parameters);

		public bool AcceptsExtraArguments => false;

		public ushort GenericParameterCount => 0;

		public bool IsGeneric => false;

		public IMethodDefinition GetResolvedMethod(EmitContext context) => null;

		public IParameterTypeInformation[] ExtraParameters
			=> new IParameterTypeInformation[0];

		public IGenericMethodInstanceReference AsGenericMethodInstanceReference => null;

		public ISpecializedMethodReference AsSpecializedMethodReference => null;

		public CallingConvention CallingConvention => CallingConvention.HasThis;

		public ushort ParameterCount => (ushort)_parameters.Length;

		public ICustomModifier[] RefCustomModifiers
			=> new ICustomModifier[0];

		public ICustomModifier[] ReturnValueCustomModifiers
			=> new ICustomModifier[0];

		public ITypeReference GetContainingType(EmitContext context)
		{
			// We are not translating arrayType. 
			// It is an array type and it is never generic or contained in a generic.
			return this.arrayType;
		}

		public IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
			=> new ICustomAttribute[0];

		public void Dispatch(MetadataVisitor visitor)
			=> visitor.Visit(this);

		public IDefinition AsDefinition(EmitContext context)
			=> null;

		public override string ToString()
			=> arrayType.ToString() + "." + Name;
	}
}
