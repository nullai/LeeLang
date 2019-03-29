using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public enum DiagnosticSeverity
	{
		/// <summary>
		/// Something that is an issue, as determined by some authority,
		/// but is not surfaced through normal means.
		/// There may be different mechanisms that act on these issues.
		/// </summary>
		Hidden = 0,

		/// <summary>
		/// Information that does not indicate a problem (i.e. not prescriptive).
		/// </summary>
		Info = 1,

		/// <summary>
		/// Something suspicious but allowed.
		/// </summary>
		Warning = 2,

		/// <summary>
		/// Something not allowed by the rules of the language or other authority.
		/// </summary>
		Error = 3,
	}

	/// <summary>
	/// Values for severity that are used internally by the compiler but are not exposed.
	/// </summary>
	public static class InternalDiagnosticSeverity
	{
		/// <summary>
		/// An unknown severity diagnostic is something whose severity has not yet been determined.
		/// </summary>
		public const DiagnosticSeverity Unknown = (DiagnosticSeverity)InternalErrorCode.Unknown;

		/// <summary>
		/// If an unknown diagnostic is resolved and found to be unnecessary then it is 
		/// treated as a "Void" diagnostic
		/// </summary>
		public const DiagnosticSeverity Void = (DiagnosticSeverity)InternalErrorCode.Void;
	}

	/// <summary>
	/// Values for ErrorCode/ERRID that are used internally by the compiler but are not exposed.
	/// </summary>
	public static class InternalErrorCode
	{
		/// <summary>
		/// The code has yet to be determined.
		/// </summary>
		public const int Unknown = -1;

		/// <summary>
		/// The code was lazily determined and does not need to be reported.
		/// </summary>
		public const int Void = -2;
	}

	public abstract partial class Diagnostic : IEquatable<Diagnostic>
	{
		public const string CompilerDiagnosticCategory = "Compiler";

		/// <summary>
		/// Highest valid warning level for non-error diagnostics.
		/// </summary>
		public const int HighestValidWarningLevel = 4;

		/// <summary>
		/// Creates a <see cref="Diagnostic"/> instance.
		/// </summary>
		/// <param name="descriptor">A <see cref="DiagnosticDescriptor"/> describing the diagnostic</param>
		/// <param name="location">An optional primary location of the diagnostic. If null, <see cref="Location"/> will return <see cref="Location.None"/>.</param>
		/// <param name="messageArgs">Arguments to the message of the diagnostic</param>
		/// <returns>The <see cref="Diagnostic"/> instance.</returns>
		public static Diagnostic Create(
			DiagnosticDescriptor descriptor,
			Location location,
			params object[] messageArgs)
		{
			return Create(descriptor, location, null, null, messageArgs);
		}

		/// <summary>
		/// Creates a <see cref="Diagnostic"/> instance.
		/// </summary>
		/// <param name="descriptor">A <see cref="DiagnosticDescriptor"/> describing the diagnostic.</param>
		/// <param name="location">An optional primary location of the diagnostic. If null, <see cref="Location"/> will return <see cref="Location.None"/>.</param>
		/// <param name="properties">
		/// An optional set of name-value pairs by means of which the analyzer that creates the diagnostic
		/// can convey more detailed information to the fixer. If null, <see cref="Properties"/> will return
		/// <see cref="Dictionary{TKey, TValue}.Empty"/>.
		/// </param>
		/// <param name="messageArgs">Arguments to the message of the diagnostic.</param>
		/// <returns>The <see cref="Diagnostic"/> instance.</returns>
		public static Diagnostic Create(
			DiagnosticDescriptor descriptor,
			Location location,
			Dictionary<string, string> properties,
			params object[] messageArgs)
		{
			return Create(descriptor, location, null, properties, messageArgs);
		}

		/// <summary>
		/// Creates a <see cref="Diagnostic"/> instance.
		/// </summary>
		/// <param name="descriptor">A <see cref="DiagnosticDescriptor"/> describing the diagnostic.</param>
		/// <param name="location">An optional primary location of the diagnostic. If null, <see cref="Location"/> will return <see cref="Location.None"/>.</param>
		/// <param name="additionalLocations">
		/// An optional set of additional locations related to the diagnostic.
		/// Typically, these are locations of other items referenced in the message.
		/// If null, <see cref="AdditionalLocations"/> will return an empty list.
		/// </param>
		/// <param name="messageArgs">Arguments to the message of the diagnostic.</param>
		/// <returns>The <see cref="Diagnostic"/> instance.</returns>
		public static Diagnostic Create(
			DiagnosticDescriptor descriptor,
			Location location,
			IEnumerable<Location> additionalLocations,
			params object[] messageArgs)
		{
			return Create(descriptor, location, additionalLocations, properties: null, messageArgs: messageArgs);
		}

		/// <summary>
		/// Creates a <see cref="Diagnostic"/> instance.
		/// </summary>
		/// <param name="descriptor">A <see cref="DiagnosticDescriptor"/> describing the diagnostic.</param>
		/// <param name="location">An optional primary location of the diagnostic. If null, <see cref="Location"/> will return <see cref="Location.None"/>.</param>
		/// <param name="additionalLocations">
		/// An optional set of additional locations related to the diagnostic.
		/// Typically, these are locations of other items referenced in the message.
		/// If null, <see cref="AdditionalLocations"/> will return an empty list.
		/// </param>
		/// <param name="properties">
		/// An optional set of name-value pairs by means of which the analyzer that creates the diagnostic
		/// can convey more detailed information to the fixer. If null, <see cref="Properties"/> will return
		/// <see cref="Dictionary{TKey, TValue}.Empty"/>.
		/// </param>
		/// <param name="messageArgs">Arguments to the message of the diagnostic.</param>
		/// <returns>The <see cref="Diagnostic"/> instance.</returns>
		public static Diagnostic Create(
			DiagnosticDescriptor descriptor,
			Location location,
			IEnumerable<Location> additionalLocations,
			Dictionary<string, string> properties,
			params object[] messageArgs)
		{
			return Create(descriptor, location, descriptor.DefaultSeverity, additionalLocations, properties, messageArgs);
		}

		/// <summary>
		/// Creates a <see cref="Diagnostic"/> instance.
		/// </summary>
		/// <param name="descriptor">A <see cref="DiagnosticDescriptor"/> describing the diagnostic.</param>
		/// <param name="location">An optional primary location of the diagnostic. If null, <see cref="Location"/> will return <see cref="Location.None"/>.</param>
		/// <param name="effectiveSeverity">Effective severity of the diagnostic.</param>
		/// <param name="additionalLocations">
		/// An optional set of additional locations related to the diagnostic.
		/// Typically, these are locations of other items referenced in the message.
		/// If null, <see cref="AdditionalLocations"/> will return an empty list.
		/// </param>
		/// <param name="properties">
		/// An optional set of name-value pairs by means of which the analyzer that creates the diagnostic
		/// can convey more detailed information to the fixer. If null, <see cref="Properties"/> will return
		/// <see cref="Dictionary{TKey, TValue}.Empty"/>.
		/// </param>
		/// <param name="messageArgs">Arguments to the message of the diagnostic.</param>
		/// <returns>The <see cref="Diagnostic"/> instance.</returns>
		public static Diagnostic Create(
			DiagnosticDescriptor descriptor,
			Location location,
			DiagnosticSeverity effectiveSeverity,
			IEnumerable<Location> additionalLocations,
			Dictionary<string, string> properties,
			params object[] messageArgs)
		{
			if (descriptor == null)
			{
				throw new ArgumentNullException(nameof(descriptor));
			}

			var warningLevel = GetDefaultWarningLevel(effectiveSeverity);
			return SimpleDiagnostic.Create(
				descriptor,
				severity: effectiveSeverity,
				warningLevel: warningLevel,
				location: location ?? Location.None,
				additionalLocations: additionalLocations,
				messageArgs: messageArgs,
				properties: properties);
		}

		/// <summary>
		/// Creates a <see cref="Diagnostic"/> instance which is localizable.
		/// </summary>
		/// <param name="id">An identifier for the diagnostic. For diagnostics generated by the compiler, this will be a numeric code with a prefix such as "CS1001".</param>
		/// <param name="category">The category of the diagnostic. For diagnostics generated by the compiler, the category will be "Compiler".</param>
		/// <param name="message">The diagnostic message text.</param>
		/// <param name="severity">The diagnostic's effective severity.</param>
		/// <param name="defaultSeverity">The diagnostic's default severity.</param>
		/// <param name="isEnabledByDefault">True if the diagnostic is enabled by default</param>
		/// <param name="warningLevel">The warning level, between 1 and 4 if severity is <see cref="DiagnosticSeverity.Warning"/>; otherwise 0.</param>
		/// <param name="title">An optional short localizable title describing the diagnostic.</param>
		/// <param name="description">An optional longer localizable description for the diagnostic.</param>
		/// <param name="helpLink">An optional hyperlink that provides more detailed information regarding the diagnostic.</param>
		/// <param name="location">An optional primary location of the diagnostic. If null, <see cref="Location"/> will return <see cref="Location.None"/>.</param>
		/// <param name="additionalLocations">
		/// An optional set of additional locations related to the diagnostic.
		/// Typically, these are locations of other items referenced in the message.
		/// If null, <see cref="AdditionalLocations"/> will return an empty list.
		/// </param>
		/// <param name="customTags">
		/// An optional set of custom tags for the diagnostic. See <see cref="WellKnownDiagnosticTags"/> for some well known tags.
		/// If null, <see cref="CustomTags"/> will return an empty list.
		/// </param>
		/// <param name="properties">
		/// An optional set of name-value pairs by means of which the analyzer that creates the diagnostic
		/// can convey more detailed information to the fixer. If null, <see cref="Properties"/> will return
		/// <see cref="Dictionary{TKey, TValue}.Empty"/>.
		/// </param>
		/// <returns>The <see cref="Diagnostic"/> instance.</returns>
		public static Diagnostic Create(
			string id,
			string category,
			string message,
			DiagnosticSeverity severity,
			DiagnosticSeverity defaultSeverity,
			bool isEnabledByDefault,
			int warningLevel,
			string title = null,
			string description = null,
			string helpLink = null,
			Location location = null,
			IEnumerable<Location> additionalLocations = null,
			IEnumerable<string> customTags = null,
			Dictionary<string, string> properties = null)
		{
			return Create(id, category, message, severity, defaultSeverity, isEnabledByDefault, warningLevel, false,
				title, description, helpLink, location, additionalLocations, customTags, properties);
		}

		/// <summary>
		/// Creates a <see cref="Diagnostic"/> instance which is localizable.
		/// </summary>
		/// <param name="id">An identifier for the diagnostic. For diagnostics generated by the compiler, this will be a numeric code with a prefix such as "CS1001".</param>
		/// <param name="category">The category of the diagnostic. For diagnostics generated by the compiler, the category will be "Compiler".</param>
		/// <param name="message">The diagnostic message text.</param>
		/// <param name="severity">The diagnostic's effective severity.</param>
		/// <param name="defaultSeverity">The diagnostic's default severity.</param>
		/// <param name="isEnabledByDefault">True if the diagnostic is enabled by default</param>
		/// <param name="warningLevel">The warning level, between 1 and 4 if severity is <see cref="DiagnosticSeverity.Warning"/>; otherwise 0.</param>
		/// <param name="isSuppressed">Flag indicating whether the diagnostic is suppressed by a source suppression.</param>
		/// <param name="title">An optional short localizable title describing the diagnostic.</param>
		/// <param name="description">An optional longer localizable description for the diagnostic.</param>
		/// <param name="helpLink">An optional hyperlink that provides more detailed information regarding the diagnostic.</param>
		/// <param name="location">An optional primary location of the diagnostic. If null, <see cref="Location"/> will return <see cref="Location.None"/>.</param>
		/// <param name="additionalLocations">
		/// An optional set of additional locations related to the diagnostic.
		/// Typically, these are locations of other items referenced in the message.
		/// If null, <see cref="AdditionalLocations"/> will return an empty list.
		/// </param>
		/// <param name="customTags">
		/// An optional set of custom tags for the diagnostic. See <see cref="WellKnownDiagnosticTags"/> for some well known tags.
		/// If null, <see cref="CustomTags"/> will return an empty list.
		/// </param>
		/// <param name="properties">
		/// An optional set of name-value pairs by means of which the analyzer that creates the diagnostic
		/// can convey more detailed information to the fixer. If null, <see cref="Properties"/> will return
		/// <see cref="Dictionary{TKey, TValue}.Empty"/>.
		/// </param>
		/// <returns>The <see cref="Diagnostic"/> instance.</returns>
		public static Diagnostic Create(
			string id,
			string category,
			string message,
			DiagnosticSeverity severity,
			DiagnosticSeverity defaultSeverity,
			bool isEnabledByDefault,
			int warningLevel,
			bool isSuppressed,
			string title = null,
			string description = null,
			string helpLink = null,
			Location location = null,
			IEnumerable<Location> additionalLocations = null,
			IEnumerable<string> customTags = null,
			Dictionary<string, string> properties = null)
		{
			if (id == null)
			{
				throw new ArgumentNullException(nameof(id));
			}

			if (category == null)
			{
				throw new ArgumentNullException(nameof(category));
			}

			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			return SimpleDiagnostic.Create(id, title ?? string.Empty, category, message, description ?? string.Empty, helpLink ?? string.Empty,
				severity, defaultSeverity, isEnabledByDefault, warningLevel, location ?? Location.None, additionalLocations, customTags, properties, isSuppressed);
		}

		public static Diagnostic Create(CommonMessageProvider messageProvider, int errorCode)
		{
			return Create(new DiagnosticInfo(messageProvider, errorCode));
		}

		public static Diagnostic Create(CommonMessageProvider messageProvider, int errorCode, params object[] arguments)
		{
			return Create(new DiagnosticInfo(messageProvider, errorCode, arguments));
		}

		public static Diagnostic Create(DiagnosticInfo info)
		{
			return new DiagnosticWithInfo(info, Location.None);
		}

		/// <summary>
		/// Gets the diagnostic descriptor, which provides a description about a <see cref="Diagnostic"/>.
		/// </summary>
		public abstract DiagnosticDescriptor Descriptor { get; }

		/// <summary>
		/// Gets the diagnostic identifier. For diagnostics generated by the compiler, this will be a numeric code with a prefix such as "CS1001".
		/// </summary>
		public abstract string Id { get; }

		/// <summary>
		/// Gets the category of diagnostic. For diagnostics generated by the compiler, the category will be "Compiler".
		/// </summary>
		public virtual string Category { get { return this.Descriptor.Category; } }

		/// <summary>
		/// Get the culture specific text of the message.
		/// </summary>
		public abstract string GetMessage();

		/// <summary>
		/// Gets the default <see cref="DiagnosticSeverity"/> of the diagnostic's <see cref="DiagnosticDescriptor"/>.
		/// </summary>
		/// <remarks>
		/// To get the effective severity of the diagnostic, use <see cref="Severity"/>.
		/// </remarks>
		public virtual DiagnosticSeverity DefaultSeverity { get { return this.Descriptor.DefaultSeverity; } }

		/// <summary>
		/// Gets the effective <see cref="DiagnosticSeverity"/> of the diagnostic.
		/// </summary>
		/// <remarks>
		/// To get the default severity of diagnostic's <see cref="DiagnosticDescriptor"/>, use <see cref="DefaultSeverity"/>.
		/// To determine if this is a warning treated as an error, use <see cref="IsWarningAsError"/>.
		/// </remarks>
		public abstract DiagnosticSeverity Severity { get; }

		/// <summary>
		/// Gets the warning level. This is 0 for diagnostics with severity <see cref="DiagnosticSeverity.Error"/>,
		/// otherwise an integer between 1 and 4.
		/// </summary>
		public abstract int WarningLevel { get; }

		/// <summary>
		/// Returns true if the diagnostic has a source suppression, i.e. an attribute or a pragma suppression.
		/// </summary>
		public abstract bool IsSuppressed { get; }

		/// <summary>
		/// Returns true if this diagnostic is enabled by default by the author of the diagnostic.
		/// </summary>
		public virtual bool IsEnabledByDefault { get { return this.Descriptor.IsEnabledByDefault; } }

		/// <summary>
		/// Returns true if this is a warning treated as an error; otherwise false.
		/// </summary>
		/// <remarks>
		/// True implies <see cref="DefaultSeverity"/> = <see cref="DiagnosticSeverity.Warning"/>
		/// and <see cref="Severity"/> = <see cref="DiagnosticSeverity.Error"/>.
		/// </remarks>
		public bool IsWarningAsError
		{
			get
			{
				return this.DefaultSeverity == DiagnosticSeverity.Warning &&
					this.Severity == DiagnosticSeverity.Error;
			}
		}

		/// <summary>
		/// Gets the primary location of the diagnostic, or <see cref="Location.None"/> if no primary location.
		/// </summary>
		public abstract Location Location { get; }

		/// <summary>
		/// Gets an array of additional locations related to the diagnostic.
		/// Typically these are the locations of other items referenced in the message.
		/// </summary>
		public abstract IList<Location> AdditionalLocations { get; }

		/// <summary>
		/// Gets custom tags for the diagnostic.
		/// </summary>
		public virtual IList<string> CustomTags { get { return (IList<string>)this.Descriptor.CustomTags; } }

		/// <summary>
		/// Gets property bag for the diagnostic. it will return <see cref="Dictionary{TKey, TValue}.Empty"/> 
		/// if there is no entry. This can be used to put diagnostic specific information you want 
		/// to pass around. for example, to corresponding fixer.
		/// </summary>
		public virtual Dictionary<string, string> Properties
			=> new Dictionary<string, string>();

		public override string ToString()
		{
			return DiagnosticFormatter.Instance.Format(this);
		}

		public abstract override bool Equals(object obj);

		public abstract override int GetHashCode();

		public abstract bool Equals(Diagnostic obj);

		private string GetDebuggerDisplay()
		{
			switch (this.Severity)
			{
				case InternalDiagnosticSeverity.Unknown:
					// If we called ToString before the diagnostic was resolved,
					// we would risk infinite recursion (e.g. if we were still computing
					// member lists).
					return "Unresolved diagnostic at " + this.Location;

				case InternalDiagnosticSeverity.Void:
					// If we called ToString on a void diagnostic, the MessageProvider
					// would complain about the code.
					return "Void diagnostic at " + this.Location;

				default:
					return ToString();
			}
		}

		/// <summary>
		/// Create a new instance of this diagnostic with the Location property changed.
		/// </summary>
		public abstract Diagnostic WithLocation(Location location);

		/// <summary>
		/// Create a new instance of this diagnostic with the Severity property changed.
		/// </summary>
		public abstract Diagnostic WithSeverity(DiagnosticSeverity severity);

		/// <summary>
		/// Create a new instance of this diagnostic with the suppression info changed.
		/// </summary>
		public abstract Diagnostic WithIsSuppressed(bool isSuppressed);

		// compatibility
		public virtual int Code { get { return 0; } }

		public virtual IList<object> Arguments
		{
			get { return new List<object>(); }
		}

		/// <summary>
		/// Returns true if the diagnostic location (or any additional location) is within the given tree and intersects with the filterSpanWithinTree, if non-null.
		/// </summary>
		public bool HasIntersectingLocation(SyntaxTree tree, TextSpan? filterSpanWithinTree = null)
		{
			var locations = this.GetDiagnosticLocationsWithinTree(tree);

			foreach (var location in locations)
			{
				if (!filterSpanWithinTree.HasValue || filterSpanWithinTree.Value.IntersectsWith(location.SourceSpan))
				{
					return true;
				}
			}

			return false;
		}

		private IEnumerable<Location> GetDiagnosticLocationsWithinTree(SyntaxTree tree)
		{
			if (this.Location.SourceTree == tree)
			{
				yield return this.Location;
			}

			if (this.AdditionalLocations != null)
			{
				foreach (var additionalLocation in this.AdditionalLocations)
				{
					if (additionalLocation.SourceTree == tree)
					{
						yield return additionalLocation;
					}
				}
			}
		}

		public Diagnostic WithReportDiagnostic(ReportDiagnostic reportAction)
		{
			switch (reportAction)
			{
				case ReportDiagnostic.Suppress:
					// Suppressed diagnostic.
					return null;
				case ReportDiagnostic.Error:
					return this.WithSeverity(DiagnosticSeverity.Error);
				case ReportDiagnostic.Default:
					return this;
				case ReportDiagnostic.Warn:
					return this.WithSeverity(DiagnosticSeverity.Warning);
				case ReportDiagnostic.Info:
					return this.WithSeverity(DiagnosticSeverity.Info);
				case ReportDiagnostic.Hidden:
					return this.WithSeverity(DiagnosticSeverity.Hidden);
				default:
					throw ExceptionUtilities.UnexpectedValue(reportAction);
			}
		}

		/// <summary>
		/// Gets the default warning level for a diagnostic severity. Warning levels are used with the <c>/warn:N</c>
		/// command line option to suppress diagnostics over a severity of interest. When N is 0, only error severity
		/// messages are produced by the compiler. Values greater than 0 indicated that warnings up to and including
		/// level N should also be included.
		/// </summary>
		/// <remarks>
		/// <see cref="DiagnosticSeverity.Info"/> and <see cref="DiagnosticSeverity.Hidden"/> are treated as warning
		/// level 1. In other words, these diagnostics which typically interact with editor features are enabled unless
		/// the special <c>/warn:0</c> option is set.
		/// </remarks>
		/// <param name="severity">A <see cref="DiagnosticSeverity"/> value.</param>
		/// <returns>The default compiler warning level for <paramref name="severity"/>.</returns>
		public static int GetDefaultWarningLevel(DiagnosticSeverity severity)
		{
			switch (severity)
			{
				case DiagnosticSeverity.Error:
					return 0;

				case DiagnosticSeverity.Warning:
				default:
					return 1;
			}
		}

		public virtual bool IsNotConfigurable()
		{
			return false;
		}
	}

	public sealed class SimpleDiagnostic : Diagnostic
	{
		private readonly DiagnosticDescriptor _descriptor;
		private readonly DiagnosticSeverity _severity;
		private readonly int _warningLevel;
		private readonly Location _location;
		private readonly IList<Location> _additionalLocations;
		private readonly object[] _messageArgs;
		private readonly Dictionary<string, string> _properties;
		private readonly bool _isSuppressed;

		private SimpleDiagnostic(
			DiagnosticDescriptor descriptor,
			DiagnosticSeverity severity,
			int warningLevel,
			Location location,
			IEnumerable<Location> additionalLocations,
			object[] messageArgs,
			Dictionary<string, string> properties,
			bool isSuppressed)
		{
			if ((warningLevel == 0 && severity != DiagnosticSeverity.Error) ||
				(warningLevel != 0 && severity == DiagnosticSeverity.Error))
			{
				throw new ArgumentException($"{nameof(warningLevel)} ({warningLevel}) and {nameof(severity)} ({severity}) are not compatible.", nameof(warningLevel));
			}

			_descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
			_severity = severity;
			_warningLevel = warningLevel;
			_location = location ?? Location.None;
			_additionalLocations = additionalLocations?.ToArray() ?? new Location[0];
			_messageArgs = messageArgs ?? new object[0];
			_properties = properties ?? new Dictionary<string, string>();
			_isSuppressed = isSuppressed;
		}

		public static SimpleDiagnostic Create(
			DiagnosticDescriptor descriptor,
			DiagnosticSeverity severity,
			int warningLevel,
			Location location,
			IEnumerable<Location> additionalLocations,
			object[] messageArgs,
			Dictionary<string, string> properties,
			bool isSuppressed = false)
		{
			return new SimpleDiagnostic(descriptor, severity, warningLevel, location, additionalLocations, messageArgs, properties, isSuppressed);
		}

		public static SimpleDiagnostic Create(string id, string title, string category, string message, string description, string helpLink,
								  DiagnosticSeverity severity, DiagnosticSeverity defaultSeverity,
								  bool isEnabledByDefault, int warningLevel, Location location,
								  IEnumerable<Location> additionalLocations, IEnumerable<string> customTags,
								  Dictionary<string, string> properties, bool isSuppressed = false)
		{
			var descriptor = new DiagnosticDescriptor(id, title, message,
				 category, defaultSeverity, isEnabledByDefault, description, helpLink, customTags.ToArray());
			return new SimpleDiagnostic(descriptor, severity, warningLevel, location, additionalLocations, messageArgs: null, properties: properties, isSuppressed: isSuppressed);
		}

		public override DiagnosticDescriptor Descriptor
		{
			get { return _descriptor; }
		}

		public override string Id
		{
			get { return _descriptor.Id; }
		}

		public override string GetMessage()
		{
			if (_messageArgs.Length == 0)
			{
				return _descriptor.MessageFormat.ToString();
			}

			var localizedMessageFormat = _descriptor.MessageFormat.ToString();

			try
			{
				return string.Format(localizedMessageFormat, _messageArgs);
			}
			catch (Exception)
			{
				// Analyzer reported diagnostic with invalid format arguments, so just return the unformatted message.
				return localizedMessageFormat;
			}
		}

		public override IList<object> Arguments
		{
			get { return _messageArgs; }
		}

		public override DiagnosticSeverity Severity
		{
			get { return _severity; }
		}

		public override bool IsSuppressed
		{
			get { return _isSuppressed; }
		}

		public override int WarningLevel
		{
			get { return _warningLevel; }
		}

		public override Location Location
		{
			get { return _location; }
		}

		public override IList<Location> AdditionalLocations
		{
			get { return _additionalLocations; }
		}

		public override Dictionary<string, string> Properties
		{
			get { return _properties; }
		}

		public override bool Equals(Diagnostic obj)
		{
			var other = obj as SimpleDiagnostic;
			if (other == null)
			{
				return false;
			}

			return _descriptor.Equals(other._descriptor)
				&& _messageArgs.SequenceEqual(other._messageArgs, (a, b) => a == b)
				&& _location == other._location
				&& _severity == other._severity
				&& _warningLevel == other._warningLevel;
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as Diagnostic);
		}

		public override int GetHashCode()
		{
			return Hash.Combine(_descriptor,
				Hash.CombineValues(_messageArgs,
				Hash.Combine(_warningLevel,
				Hash.Combine(_location, (int)_severity))));
		}

		public override Diagnostic WithLocation(Location location)
		{
			if (location == null)
			{
				throw new ArgumentNullException(nameof(location));
			}

			if (location != _location)
			{
				return new SimpleDiagnostic(_descriptor, _severity, _warningLevel, location, _additionalLocations, _messageArgs, _properties, _isSuppressed);
			}

			return this;
		}

		public override Diagnostic WithSeverity(DiagnosticSeverity severity)
		{
			if (this.Severity != severity)
			{
				var warningLevel = GetDefaultWarningLevel(severity);
				return new SimpleDiagnostic(_descriptor, severity, warningLevel, _location, _additionalLocations, _messageArgs, _properties, _isSuppressed);
			}

			return this;
		}

		public override Diagnostic WithIsSuppressed(bool isSuppressed)
		{
			if (this.IsSuppressed != isSuppressed)
			{
				return new SimpleDiagnostic(_descriptor, _severity, _warningLevel, _location, _additionalLocations, _messageArgs, _properties, isSuppressed);
			}

			return this;
		}
	}

	public class DiagnosticWithInfo : Diagnostic
	{
		private readonly DiagnosticInfo _info;
		private readonly Location _location;
		private readonly bool _isSuppressed;

		public DiagnosticWithInfo(DiagnosticInfo info, Location location, bool isSuppressed = false)
		{
			Debug.Assert(info != null);
			Debug.Assert(location != null);
			_info = info;
			_location = location;
			_isSuppressed = isSuppressed;
		}

		public override Location Location
		{
			get { return _location; }
		}

		public override IList<Location> AdditionalLocations
		{
			get { return this.Info.AdditionalLocations; }
		}

		public override IList<string> CustomTags
		{
			get
			{
				return this.Info.CustomTags;
			}
		}

		public override DiagnosticDescriptor Descriptor
		{
			get
			{
				return this.Info.Descriptor;
			}
		}

		public override string Id
		{
			get { return this.Info.MessageIdentifier; }
		}

		public override string Category
		{
			get { return this.Info.Category; }
		}


		public sealed override int Code
		{
			get { return this.Info.Code; }
		}

		public sealed override DiagnosticSeverity Severity
		{
			get { return this.Info.Severity; }
		}

		public sealed override DiagnosticSeverity DefaultSeverity
		{
			get { return this.Info.DefaultSeverity; }
		}

		public sealed override bool IsEnabledByDefault
		{
			// All compiler errors and warnings are enabled by default.
			get { return true; }
		}

		public override bool IsSuppressed
		{
			get { return _isSuppressed; }
		}

		public sealed override int WarningLevel
		{
			get { return this.Info.WarningLevel; }
		}

		public override string GetMessage()
		{
			return this.Info.GetMessage();
		}

		public override IList<object> Arguments
		{
			get { return this.Info.Arguments; }
		}

		/// <summary>
		/// Get the information about the diagnostic: the code, severity, message, etc.
		/// </summary>
		public DiagnosticInfo Info
		{
			get
			{
				if (_info.Severity == InternalDiagnosticSeverity.Unknown)
				{
					return _info.GetResolvedInfo();
				}

				return _info;
			}
		}

		/// <summary>
		/// True if the DiagnosticInfo for this diagnostic requires (or required - this property
		/// is immutable) resolution.
		/// </summary>
		public bool HasLazyInfo
		{
			get
			{
				return _info.Severity == InternalDiagnosticSeverity.Unknown ||
					_info.Severity == InternalDiagnosticSeverity.Void;
			}
		}

		public override int GetHashCode()
		{
			return Hash.Combine(this.Location.GetHashCode(), this.Info.GetHashCode());
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Diagnostic);
		}

		public override bool Equals(Diagnostic obj)
		{
			if (this == obj)
			{
				return true;
			}

			var other = obj as DiagnosticWithInfo;

			if (other == null || this.GetType() != other.GetType())
			{
				return false;
			}

			return
				this.Location.Equals(other._location) &&
				this.Info.Equals(other.Info) &&
				this.AdditionalLocations.SequenceEqual(other.AdditionalLocations);
		}

		private string GetDebuggerDisplay()
		{
			switch (_info.Severity)
			{
				case InternalDiagnosticSeverity.Unknown:
					// If we called ToString before the diagnostic was resolved,
					// we would risk infinite recursion (e.g. if we were still computing
					// member lists).
					return "Unresolved diagnostic at " + this.Location;

				case InternalDiagnosticSeverity.Void:
					// If we called ToString on a void diagnostic, the MessageProvider
					// would complain about the code.
					return "Void diagnostic at " + this.Location;

				default:
					return ToString();
			}
		}

		public override Diagnostic WithLocation(Location location)
		{
			if (location == null)
			{
				throw new ArgumentNullException(nameof(location));
			}

			if (location != _location)
			{
				return new DiagnosticWithInfo(_info, location, _isSuppressed);
			}

			return this;
		}

		public override Diagnostic WithSeverity(DiagnosticSeverity severity)
		{
			if (this.Severity != severity)
			{
				return new DiagnosticWithInfo(this.Info.GetInstanceWithSeverity(severity), _location, _isSuppressed);
			}

			return this;
		}

		public override Diagnostic WithIsSuppressed(bool isSuppressed)
		{
			if (this.IsSuppressed != isSuppressed)
			{
				return new DiagnosticWithInfo(this.Info, _location, isSuppressed);
			}

			return this;
		}

		public sealed override bool IsNotConfigurable()
		{
			return this.Info.IsNotConfigurable();
		}
	}
}
