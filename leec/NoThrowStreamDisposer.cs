using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace leec
{
	public class NoThrowStreamDisposer : IDisposable
	{
		private bool? _failed; // Nullable to assert that this is only checked after dispose
		private readonly string _filePath;
		private readonly DiagnosticBag _diagnostics;
		private readonly CommonMessageProvider _messageProvider;

		/// <summary>
		/// Underlying stream
		/// </summary>
		public Stream Stream { get; }

		/// <summary>
		/// True iff an exception was thrown during a call to <see cref="Dispose"/>
		/// </summary>
		public bool HasFailedToDispose
		{
			get
			{
				Debug.Assert(_failed != null);
				return _failed.GetValueOrDefault();
			}
		}

		public NoThrowStreamDisposer(
			Stream stream,
			string filePath,
			DiagnosticBag diagnostics,
			CommonMessageProvider messageProvider)
		{
			Stream = stream;
			_failed = null;
			_filePath = filePath;
			_diagnostics = diagnostics;
			_messageProvider = messageProvider;
		}

		public void Dispose()
		{
			Debug.Assert(_failed == null);
			try
			{
				Stream.Dispose();
				if (_failed == null)
				{
					_failed = false;
				}
			}
			catch (Exception e)
			{
				_messageProvider.ReportStreamWriteException(e, _filePath, _diagnostics);
				// Record if any exceptions are thrown during dispose
				_failed = true;
			}
		}
	}
}
