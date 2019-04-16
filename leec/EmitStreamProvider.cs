using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace leec
{
	public abstract class EmitStreamProvider
	{
		/// <summary>
		/// Returns an existing open stream or null if no stream has been open.
		/// </summary>
		public abstract Stream Stream { get; }

		/// <summary>
		/// This method will be called once during Emit at the time the Compilation needs 
		/// to create a stream for writing. It will not be called in the case of
		/// user errors in code. Shall not be called when <see cref="Stream"/> returns non-null.
		/// </summary>
		public abstract Stream CreateStream(DiagnosticBag diagnostics);

		public Stream GetOrCreateStream(DiagnosticBag diagnostics)
		{
			return Stream ?? CreateStream(diagnostics);
		}
	}

	public sealed class SimpleEmitStreamProvider : EmitStreamProvider
	{
		private readonly Stream _stream;

		public SimpleEmitStreamProvider(Stream stream)
		{
			Debug.Assert(stream != null);
			_stream = stream;
		}

		public override Stream Stream => _stream;

		public override Stream CreateStream(DiagnosticBag diagnostics)
		{
			throw ExceptionUtilities.Unreachable;
		}
	}
}
