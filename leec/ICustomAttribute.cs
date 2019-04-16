using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public interface ICustomAttribute
	{
		/// <summary>
		/// Zero or more positional arguments for the attribute constructor.
		/// </summary>
		IMetadataExpression[] GetArguments(EmitContext context);

		/// <summary>
		/// A reference to the constructor that will be used to instantiate this custom attribute during execution (if the attribute is inspected via Reflection).
		/// </summary>
		IMethodReference Constructor(EmitContext context, bool reportDiagnostics);

		/// <summary>
		/// Zero or more named arguments that specify values for fields and properties of the attribute.
		/// </summary>
		IMetadataNamedArgument[] GetNamedArguments(EmitContext context);

		/// <summary>
		/// The number of positional arguments.
		/// </summary>
		int ArgumentCount
		{
			get;
		}

		/// <summary>
		/// The number of named arguments.
		/// </summary>
		ushort NamedArgumentCount
		{
			get;
		}

		/// <summary>
		/// The type of the attribute. For example System.AttributeUsageAttribute.
		/// </summary>
		ITypeReference GetType(EmitContext context);

		/// <summary>
		/// Whether attribute allows multiple.
		/// </summary>
		bool AllowMultiple { get; }
	}
}
