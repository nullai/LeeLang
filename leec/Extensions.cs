using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace leec
{
	public static class Extensions
	{
		public static Span<T> AsSpan<T>(this T[] value)
		{
			return new Span<T>(value);
		}

		public static Span<T> AsSpan<T>(this T[] value, int start)
		{
			return new Span<T>(value, start, value.Length - start);
		}

		public static Span<T> AsSpan<T>(this T[] value, int start, int length)
		{
			return new Span<T>(value, start, length);
		}

		public static Span<char> AsSpan(this string value)
		{
			return new Span<char>(value.ToCharArray());
		}

		public static TNode WithDiagnosticsGreen<TNode>(this TNode node, DiagnosticInfo[] diagnostics) where TNode : GreenNode
		{
			return (TNode)node.SetDiagnostics(diagnostics);
		}

		public static void AddOptional<T>(this List<T> builder, T item)
			where T : class
		{
			if (item != null)
			{
				builder.Add(item);
			}
		}

		public static int IndexOf<T>(this T[] array, T value, int start, int count) where T : IEquatable<T>
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Equals(value))
					return i;
			}
			return -1;
		}

		public static bool SequenceEqual<T>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> comparer)
		{
			Debug.Assert(comparer != null);

			if (first == second)
			{
				return true;
			}

			if (first == null || second == null)
			{
				return false;
			}

			using (var enumerator = first.GetEnumerator())
			using (var enumerator2 = second.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator2.MoveNext() || !comparer(enumerator.Current, enumerator2.Current))
					{
						return false;
					}
				}

				if (enumerator2.MoveNext())
				{
					return false;
				}
			}

			return true;
		}

		public static int BinarySearchUpperBound(this int[] array, int value)
		{
			int low = 0;
			int high = array.Length - 1;

			while (low <= high)
			{
				int middle = low + ((high - low) >> 1);
				if (array[middle] > value)
				{
					high = middle - 1;
				}
				else
				{
					low = middle + 1;
				}
			}

			return low;
		}

		public static int BinarySearch(this int[] array, int value)
		{
			var low = 0;
			var high = array.Length - 1;

			while (low <= high)
			{
				var middle = low + ((high - low) >> 1);
				var midValue = array[middle];

				if (midValue == value)
				{
					return middle;
				}
				else if (midValue > value)
				{
					high = middle - 1;
				}
				else
				{
					low = middle + 1;
				}
			}

			return ~low;
		}

		public static SyntaxToken ExtractAnonymousTypeMemberName(this ExpressionSyntax input)
		{
			while (true)
			{
				switch (input.Kind)
				{
					case SyntaxKind.IdentifierName:
						return ((IdentifierNameSyntax)input).Identifier;

					case SyntaxKind.SimpleMemberAccessExpression:
						input = ((MemberAccessExpressionSyntax)input).Name;
						continue;

					case SyntaxKind.ConditionalAccessExpression:
						input = ((ConditionalAccessExpressionSyntax)input).WhenNotNull;
						if (input.Kind == SyntaxKind.MemberBindingExpression)
						{
							return ((MemberBindingExpressionSyntax)input).Name.Identifier;
						}

						continue;

					default:
						return default(SyntaxToken);
				}
			}
		}

		public static bool HasBody(this IMethodDefinition methodDef)
		{
			// Method definition has body if it is a non-abstract, non-extern method.
			// Additionally, methods within COM types have no body.

			return !methodDef.IsAbstract && !methodDef.IsExternal &&
				(methodDef.ContainingTypeDefinition == null || !methodDef.ContainingTypeDefinition.IsComObject);
		}

		/// <summary>
		/// When emitting ref assemblies, some members will not be included.
		/// </summary>
		public static bool ShouldInclude(this ITypeDefinitionMember member, EmitContext context)
		{
			if (context.IncludePrivateMembers)
			{
				return true;
			}

			var method = member as IMethodDefinition;
			if (method != null && method.IsVirtual)
			{
				return true;
			}

			switch (member.Visibility)
			{
				case TypeMemberVisibility.Private:
					return context.IncludePrivateMembers;
				case TypeMemberVisibility.Assembly:
				case TypeMemberVisibility.FamilyAndAssembly:
					return context.IncludePrivateMembers || context.Module.SourceAssemblyOpt?.InternalsAreVisible == true;
			}
			return true;
		}

		public static bool ValueEquals(this uint[] array, uint[] other)
		{
			if (array == other)
			{
				return true;
			}

			if (array == null || other == null || array.Length != other.Length)
			{
				return false;
			}

			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != other[i])
				{
					return false;
				}
			}

			return true;
		}

		public static int IndexOf(this string[] array, string value)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == value)
					return i;
			}
			return -1;
		}

		public static int GetMaxCharCountOrThrowIfHuge(this Encoding encoding, Stream stream)
		{
			Debug.Assert(stream.CanSeek);
			long length = stream.Length;

			int maxCharCount;
			if (encoding.TryGetMaxCharCount(length, out maxCharCount))
			{
				return maxCharCount;
			}

#if WORKSPACE
            throw new IOException(WorkspacesResources.Stream_is_too_long);
#else
			throw new IOException("@StreamIsTooLong");
#endif
		}

		public static bool TryGetMaxCharCount(this Encoding encoding, long length, out int maxCharCount)
		{
			maxCharCount = 0;

			if (length <= int.MaxValue)
			{
				try
				{
					maxCharCount = encoding.GetMaxCharCount((int)length);
					return true;
				}
				catch (ArgumentOutOfRangeException)
				{
					// Encoding does not provide a way to predict that max byte count would not
					// fit in Int32 and we must therefore catch ArgumentOutOfRange to handle that
					// case.
				}
			}

			return false;
		}

		public static bool TryGetGuidAttributeValue(this AttributeData attrData, out string guidString)
		{
			if (attrData.CommonConstructorArguments.Length == 1)
			{
				object value = attrData.CommonConstructorArguments[0].Value;

				if (value == null || value is string)
				{
					guidString = (string)value;
					return true;
				}
			}

			guidString = null;
			return false;
		}
	}
}
