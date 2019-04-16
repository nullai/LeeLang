using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace leec
{
	/// <summary>
	/// This type abstracts away the legacy COM based signing implementation for PE streams. Under the hood
	/// a temporary file must be created on disk (at the last possible moment), emitted to, signed on disk
	/// and then copied back to the original <see cref="Stream"/>. Only when legacy signing is enabled though.
	/// </summary>
	public sealed class EmitStream
	{
		private readonly EmitStreamProvider _emitStreamProvider;
		private Stream _tempStream;
		private string _tempFilePath;

		/// <summary>
		/// The <see cref="Stream"/> that is being emitted into. This value should _never_ be 
		/// disposed. It is either returned from the <see cref="EmitStreamProvider"/> instance in
		/// which case it is owned by that. Or it is just an alias for the value that is stored 
		/// in <see cref="_tempInfo"/> in which case it will be disposed from there.
		/// </summary>
		private Stream _stream;

		public EmitStream(
			EmitStreamProvider emitStreamProvider)
		{
			Debug.Assert(emitStreamProvider != null);
			_emitStreamProvider = emitStreamProvider;
		}

		public Func<Stream> GetCreateStreamFunc(DiagnosticBag diagnostics)
		{
			return () => CreateStream(diagnostics);
		}

		public void Close()
		{
			// The _stream value is deliberately excluded from being disposed here. That value is not 
			// owned by this type.
			_stream = null;

			if (_tempStream != null)
			{
				var tempStream = _tempStream;
				var tempFilePath = _tempFilePath;
				_tempStream = null;
				_tempFilePath = null;

				try
				{
					tempStream.Dispose();
				}
				finally
				{
					try
					{
						File.Delete(tempFilePath);
					}
					catch
					{
						// Not much to do if we can't delete from the temp directory
					}
				}
			}
		}

		/// <summary>
		/// Create the stream which should be used for Emit. This should only be called one time.
		/// </summary>
		private Stream CreateStream(DiagnosticBag diagnostics)
		{
			Debug.Assert(_stream == null);
			Debug.Assert(diagnostics != null);

			if (diagnostics.HasAnyErrors())
			{
				return null;
			}

			_stream = _emitStreamProvider.GetOrCreateStream(diagnostics);
			if (_stream == null)
			{
				return null;
			}

			return _stream;
		}

		public bool Complete(CommonMessageProvider messageProvider, DiagnosticBag diagnostics)
		{
			Debug.Assert(_stream != null);
			
			Close();

			return true;
		}
	}
}
