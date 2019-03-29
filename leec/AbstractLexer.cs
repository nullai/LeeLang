﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public class AbstractLexer : IDisposable
	{
		public readonly SlidingTextWindow TextWindow;
		private List<SyntaxDiagnosticInfo> _errors;

		protected AbstractLexer(SourceText text)
		{
			this.TextWindow = new SlidingTextWindow(text);
		}

		public virtual void Dispose()
		{
			this.TextWindow.Dispose();
		}

		protected void Start()
		{
			TextWindow.Start();
			_errors = null;
		}

		protected bool HasErrors
		{
			get { return _errors != null; }
		}

		protected SyntaxDiagnosticInfo[] GetErrors(int leadingTriviaWidth)
		{
			if (_errors != null)
			{
				if (leadingTriviaWidth > 0)
				{
					var array = new SyntaxDiagnosticInfo[_errors.Count];
					for (int i = 0; i < _errors.Count; i++)
					{
						// fixup error positioning to account for leading trivia
						array[i] = _errors[i].WithOffset(_errors[i].Offset + leadingTriviaWidth);
					}

					return array;
				}
				else
				{
					return _errors.ToArray();
				}
			}
			else
			{
				return null;
			}
		}

		protected void AddError(int position, int width, ErrorCode code)
		{
			this.AddError(this.MakeError(position, width, code));
		}

		protected void AddError(int position, int width, ErrorCode code, params object[] args)
		{
			this.AddError(this.MakeError(position, width, code, args));
		}

		protected void AddError(ErrorCode code)
		{
			this.AddError(MakeError(code));
		}

		protected void AddError(ErrorCode code, params object[] args)
		{
			this.AddError(MakeError(code, args));
		}

		protected void AddError(SyntaxDiagnosticInfo error)
		{
			if (error != null)
			{
				if (_errors == null)
				{
					_errors = new List<SyntaxDiagnosticInfo>(8);
				}

				_errors.Add(error);
			}
		}

		protected SyntaxDiagnosticInfo MakeError(int position, int width, ErrorCode code)
		{
			int offset = GetLexemeOffsetFromPosition(position);
			return new SyntaxDiagnosticInfo(offset, width, code);
		}

		protected SyntaxDiagnosticInfo MakeError(int position, int width, ErrorCode code, params object[] args)
		{
			int offset = GetLexemeOffsetFromPosition(position);
			return new SyntaxDiagnosticInfo(offset, width, code, args);
		}

		private int GetLexemeOffsetFromPosition(int position)
		{
			return position >= TextWindow.LexemeStartPosition ? position - TextWindow.LexemeStartPosition : position;
		}

		protected static SyntaxDiagnosticInfo MakeError(ErrorCode code)
		{
			return new SyntaxDiagnosticInfo(code);
		}

		protected static SyntaxDiagnosticInfo MakeError(ErrorCode code, params object[] args)
		{
			return new SyntaxDiagnosticInfo(code, args);
		}
	}
}