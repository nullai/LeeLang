using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace leec
{
	public static class EncodedStringText
	{
		private const int LargeObjectHeapLimitInChars = 40 * 1024; // 40KB

		/// <summary>
		/// Encoding to use when there is no byte order mark (BOM) on the stream. This encoder may throw a <see cref="DecoderFallbackException"/>
		/// if the stream contains invalid UTF-8 bytes.
		/// </summary>
		private static readonly Encoding s_utf8Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

		/// <summary>
		/// Encoding to use when UTF-8 fails. We try to find the following, in order, if available:
		///     1. The default ANSI codepage
		///     2. CodePage 1252.
		///     3. Latin1.
		/// </summary>
		private static readonly Lazy<Encoding> s_fallbackEncoding = new Lazy<Encoding>(GetFallbackEncoding);

		private static Encoding GetFallbackEncoding()
		{
			try
			{
				// Try to get the default ANSI code page in the operating system's
				// regional and language settings, and fall back to 1252 otherwise
				return Encoding.GetEncoding(0)
					?? Encoding.GetEncoding(1252);
			}
			catch (NotSupportedException)
			{
				return Encoding.GetEncoding(name: "Latin1");
			}
		}

		/// <summary>
		/// Initializes an instance of <see cref="SourceText"/> from the provided stream. This version differs
		/// from <see cref="SourceText.From(Stream, Encoding, SourceHashAlgorithm, bool)"/> in two ways:
		/// 1. It attempts to minimize allocations by trying to read the stream into a byte array.
		/// 2. If <paramref name="defaultEncoding"/> is null, it will first try UTF8 and, if that fails, it will
		///    try CodePage 1252. If CodePage 1252 is not available on the system, then it will try Latin1.
		/// </summary>
		/// <param name="stream">The stream containing encoded text.</param>
		/// <param name="defaultEncoding">
		/// Specifies an encoding to be used if the actual encoding can't be determined from the stream content (the stream doesn't start with Byte Order Mark).
		/// If not specified auto-detect heuristics are used to determine the encoding. If these heuristics fail the decoding is assumed to be Encoding.Default.
		/// Note that if the stream starts with Byte Order Mark the value of <paramref name="defaultEncoding"/> is ignored.
		/// </param>
		/// <param name="canBeEmbedded">Indicates if the file can be embedded in the PDB.</param>
		/// <param name="checksumAlgorithm">Hash algorithm used to calculate document checksum.</param>
		/// <exception cref="InvalidDataException">
		/// The stream content can't be decoded using the specified <paramref name="defaultEncoding"/>, or
		/// <paramref name="defaultEncoding"/> is null and the stream appears to be a binary file.
		/// </exception>
		/// <exception cref="IOException">An IO error occurred while reading from the stream.</exception>
		public static SourceText Create(Stream stream, Encoding defaultEncoding = null)
		{
			return Create(stream,
				s_fallbackEncoding,
				defaultEncoding: defaultEncoding);
		}

		// public for testing
		public static SourceText Create(Stream stream, Lazy<Encoding> getEncoding, Encoding defaultEncoding = null)
		{
			Debug.Assert(stream != null);
			Debug.Assert(stream.CanRead && stream.CanSeek);

			bool detectEncoding = defaultEncoding == null;
			if (detectEncoding)
			{
				try
				{
					return Decode(stream, s_utf8Encoding, throwIfBinaryDetected: false);
				}
				catch (DecoderFallbackException)
				{
					// Fall back to Encoding.ASCII
				}
			}

			try
			{
				return Decode(stream, defaultEncoding ?? getEncoding.Value, throwIfBinaryDetected: detectEncoding);
			}
			catch (DecoderFallbackException e)
			{
				throw new InvalidDataException(e.Message);
			}
		}

		/// <summary>
		/// Try to create a <see cref="SourceText"/> from the given stream using the given encoding.
		/// </summary>
		/// <param name="data">The input stream containing the encoded text. The stream will not be closed.</param>
		/// <param name="encoding">The expected encoding of the stream. The actual encoding used may be different if byte order marks are detected.</param>
		/// <param name="checksumAlgorithm">The checksum algorithm to use.</param>
		/// <param name="throwIfBinaryDetected">Throw <see cref="InvalidDataException"/> if binary (non-text) data is detected.</param>
		/// <returns>The <see cref="SourceText"/> decoded from the stream.</returns>
		/// <exception cref="DecoderFallbackException">The decoder was unable to decode the stream with the given encoding.</exception>
		/// <exception cref="IOException">Error reading from stream.</exception> 
		/// <remarks>
		/// public for unit testing
		/// </remarks>
		public static SourceText Decode(
			Stream data,
			Encoding encoding,
			bool throwIfBinaryDetected = false)
		{
			Debug.Assert(data != null);
			Debug.Assert(encoding != null);

			data.Seek(0, SeekOrigin.Begin);

			return SourceText.From(data, encoding, throwIfBinaryDetected);
		}
	}
}
