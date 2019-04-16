using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace leec
{
	public abstract class SourceText
	{
		private static readonly Encoding s_utf8EncodingWithNoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);

		public abstract Encoding Encoding { get; }
		public abstract int Length { get; }

		public abstract char this[int position] { get; }

		public static SourceText From(string text, Encoding encoding = null)
		{
			if (text == null)
			{
				throw new ArgumentNullException(nameof(text));
			}

			return new StringText(text, encoding);
		}

		public static SourceText From(
			Stream stream,
			Encoding encoding = null,
			bool throwIfBinaryDetected = false)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			if (!stream.CanRead || !stream.CanSeek)
			{
				throw new ArgumentException("@StreamMustSupportReadAndSeek", nameof(stream));
			}

			encoding = encoding ?? s_utf8EncodingWithNoBOM;

			string text = Decode(stream, encoding, out encoding);
			if (throwIfBinaryDetected && IsBinary(text))
			{
				throw new InvalidDataException();
			}

			return new StringText(text, encoding);
		}

		private static string Decode(Stream stream, Encoding encoding, out Encoding actualEncoding)
		{
			Debug.Assert(stream != null);
			Debug.Assert(encoding != null);

			stream.Seek(0, SeekOrigin.Begin);

			int length = (int)stream.Length;
			if (length == 0)
			{
				actualEncoding = encoding;
				return string.Empty;
			}

			// Note: We are setting the buffer size to 4KB instead of the default 1KB. That's
			// because we can reach this code path for FileStreams and, to avoid FileStream
			// buffer allocations for small files, we may intentionally be using a FileStream
			// with a very small (1 byte) buffer. Using 4KB here matches the default buffer
			// size for FileStream and means we'll still be doing file I/O in 4KB chunks.
			using (var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: Math.Min(4096, length)))
			{
				string text = reader.ReadToEnd();
				actualEncoding = reader.CurrentEncoding;
				return text;
			}
		}

		public static bool IsBinary(string text)
		{
			// PERF: We can advance two chars at a time unless we find a NUL.
			for (int i = 1; i < text.Length;)
			{
				if (text[i] == '\0')
				{
					if (text[i - 1] == '\0')
					{
						return true;
					}

					i += 1;
				}
				else
				{
					i += 2;
				}
			}

			return false;
		}

		public override string ToString()
		{
			return ToString(new TextSpan(0, this.Length));
		}

		public virtual string ToString(TextSpan span)
		{
			CheckSubSpan(span);

			// default implementation constructs text using CopyTo
			var builder = new StringBuilder();
			var buffer = new char[Math.Min(span.Length, 1024)];

			int position = Math.Max(Math.Min(span.Start, this.Length), 0);
			int length = Math.Min(span.End, this.Length) - position;

			while (position < this.Length && length > 0)
			{
				int copyLength = Math.Min(buffer.Length, length);
				this.CopyTo(position, buffer, 0, copyLength);
				builder.Append(buffer, 0, copyLength);
				length -= copyLength;
				position += copyLength;
			}

			return builder.ToString();
		}

		public void CheckSubSpan(TextSpan span)
		{
			if (span.Start < 0 || span.Start > this.Length || span.End > this.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(span));
			}
		}

		public abstract void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count);
	}
}
