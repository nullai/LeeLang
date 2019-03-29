using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	}
}
