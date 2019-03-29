using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class NullableDirectiveMap
	{
		public struct MapItemData
		{
			public int Position;
			public bool? State;

			public MapItemData(int p, bool? s)
			{
				Position = p;
				State = s;
			}
		}

		private static readonly NullableDirectiveMap EmptyGenerated = new NullableDirectiveMap(new MapItemData[0], isGeneratedCode: true);

		private static readonly NullableDirectiveMap EmptyNonGenerated = new NullableDirectiveMap(new MapItemData[0], isGeneratedCode: false);


		private readonly MapItemData[] _directives;

		private readonly bool _isGeneratedCode;

		public static NullableDirectiveMap Create(SyntaxTree tree, bool isGeneratedCode)
		{
			var directives = GetDirectives(tree);

			var empty = isGeneratedCode ? EmptyGenerated : EmptyNonGenerated;
			return directives.Length == 0 ? empty : new NullableDirectiveMap(directives, isGeneratedCode);
		}

		private NullableDirectiveMap(MapItemData[] directives, bool isGeneratedCode)
		{
#if DEBUG
			for (int i = 1; i < directives.Length; i++)
			{
				Debug.Assert(directives[i - 1].Position < directives[i].Position);
			}
#endif
			_directives = directives;
			_isGeneratedCode = isGeneratedCode;
		}

		/// <summary>
		/// Returns true if the `#nullable` directive preceding the position is
		/// `enable` or `safeonly`, false if `disable`, and null if no preceding directive,
		/// or directive preceding the position is `restore`.
		/// </summary>
		public bool? GetDirectiveState(int position)
		{
			int index = -1;
			for (int i = 0; i < _directives.Length; i++)
			{
				if (_directives[i].Position == position && _directives[i].State == false)
				{
					index = i;
					break;
				}
			}
			bool? state = null;
			if (index >= 0)
			{
				Debug.Assert(_directives[index].Position <= position);
				Debug.Assert(index == _directives.Length - 1 || position < _directives[index + 1].Position);
				state = _directives[index].State;
			}

			if (state == null && _isGeneratedCode)
			{
				// Generated files have a default nullable context that is "disabled".
				state = false;
			}

			return state;
		}

		private static MapItemData[] GetDirectives(SyntaxTree tree)
		{
			throw new Exception();
			/*
			var builder = new List<MapItemData>();
			foreach (var d in tree.GetRoot().GetDirectives())
			{
				if (d.Kind() != SyntaxKind.NullableDirectiveTrivia)
				{
					continue;
				}
				var nn = (NullableDirectiveTriviaSyntax)d;
				if (nn.SettingToken.IsMissing || !nn.IsActive)
				{
					continue;
				}

				bool? state;
				switch (nn.SettingToken.Kind())
				{
					case SyntaxKind.EnableKeyword:
					case SyntaxKind.SafeOnlyKeyword:
						state = true;
						break;
					case SyntaxKind.RestoreKeyword:
						state = null;
						break;
					case SyntaxKind.DisableKeyword:
						state = false;
						break;
					default:
						throw ExceptionUtilities.UnexpectedValue(nn.SettingToken.Kind());
				}

				builder.Add((nn.Location.SourceSpan.End, state));
			}
			return builder.ToImmutableAndFree();*/
		}
	}
}
