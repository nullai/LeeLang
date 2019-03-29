using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public class DiagnosticFormatter
	{
		/// <summary>
		/// Formats the <see cref="Diagnostic"/> message using the optional <see cref="IFormatProvider"/>.
		/// </summary>
		/// <param name="diagnostic">The diagnostic.</param>
		/// <param name="formatter">The formatter; or null to use the default formatter.</param>
		/// <returns>The formatted message.</returns>
		public virtual string Format(Diagnostic diagnostic)
		{
			if (diagnostic == null)
			{
				throw new ArgumentNullException(nameof(diagnostic));
			}

			switch (diagnostic.Location.Kind)
			{
				case LocationKind.SourceFile:
				case LocationKind.XmlFile:
				case LocationKind.ExternalFile:
					var span = diagnostic.Location.GetLineSpan();
					var mappedSpan = diagnostic.Location.GetMappedLineSpan();
					if (!span.IsValid || !mappedSpan.IsValid)
					{
						goto default;
					}

					string path, basePath;
					if (mappedSpan.HasMappedPath)
					{
						path = mappedSpan.Path;
						basePath = span.Path;
					}
					else
					{
						path = span.Path;
						basePath = null;
					}

					return string.Format("{0}{1}: {2}: {3}",
										 FormatSourcePath(path, basePath),
										 FormatSourceSpan(mappedSpan.Span),
										 GetMessagePrefix(diagnostic),
										 diagnostic.GetMessage());

				default:
					return string.Format("{0}: {1}",
										 GetMessagePrefix(diagnostic),
										 diagnostic.GetMessage());
			}
		}

		public virtual string FormatSourcePath(string path, string basePath)
		{
			// ignore base path
			return path;
		}

		public virtual string FormatSourceSpan(LinePositionSpan span)
		{
			return string.Format("({0},{1})", span.Start.Line + 1, span.Start.Character + 1);
		}

		public string GetMessagePrefix(Diagnostic diagnostic)
		{
			string prefix;
			switch (diagnostic.Severity)
			{
				case DiagnosticSeverity.Hidden:
					prefix = "hidden";
					break;
				case DiagnosticSeverity.Info:
					prefix = "info";
					break;
				case DiagnosticSeverity.Warning:
					prefix = "warning";
					break;
				case DiagnosticSeverity.Error:
					prefix = "error";
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(diagnostic.Severity);
			}

			return string.Format("{0} {1}", prefix, diagnostic.Id);
		}

		public static readonly DiagnosticFormatter Instance = new DiagnosticFormatter();
	}
}
