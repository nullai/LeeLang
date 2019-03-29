using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	public static class WellKnownDiagnosticTags
	{
		/// <summary>
		/// Indicates that the diagnostic is related to some unnecessary source code.
		/// </summary>
		public const string Unnecessary = nameof(Unnecessary);

		/// <summary>
		/// Indicates that the diagnostic is related to edit and continue.
		/// </summary>
		public const string EditAndContinue = nameof(EditAndContinue);

		/// <summary>
		/// Indicates that the diagnostic is related to build.
		/// </summary>
		public const string Build = nameof(Build);

		/// <summary>
		/// Indicates that the diagnostic is reported by the compiler.
		/// </summary>
		public const string Compiler = nameof(Compiler);

		/// <summary>
		/// Indicates that the diagnostic can be used for telemetry
		/// </summary>
		public const string Telemetry = nameof(Telemetry);

		/// <summary>
		/// Indicates that the diagnostic is not configurable, i.e. it cannot be suppressed or filtered or have its severity changed.
		/// </summary>
		public const string NotConfigurable = nameof(NotConfigurable);

		/// <summary>
		/// Indicates that the diagnostic is related to an exception thrown by a <see cref="DiagnosticAnalyzer"/>.
		/// </summary>
		public const string AnalyzerException = nameof(AnalyzerException);
	}

	public class DiagnosticInfo
	{
		private readonly CommonMessageProvider _messageProvider;
		private readonly int _errorCode;
		private readonly DiagnosticSeverity _defaultSeverity;
		private readonly DiagnosticSeverity _effectiveSeverity;
		private readonly object[] _arguments;
		private static Dictionary<int, DiagnosticDescriptor> s_errorCodeToDescriptorMap = new Dictionary<int, DiagnosticDescriptor>();
		private static readonly string[] s_compilerErrorCustomTags = { WellKnownDiagnosticTags.Compiler, WellKnownDiagnosticTags.Telemetry , WellKnownDiagnosticTags.NotConfigurable };
		private static readonly string[] s_compilerNonErrorCustomTags = { WellKnownDiagnosticTags.Compiler, WellKnownDiagnosticTags.Telemetry };

		public DiagnosticInfo(CommonMessageProvider messageProvider, int errorCode)
		{
			_messageProvider = messageProvider;
			_errorCode = errorCode;
			_defaultSeverity = messageProvider.GetSeverity(errorCode);
			_effectiveSeverity = _defaultSeverity;
		}

		public DiagnosticInfo(CommonMessageProvider messageProvider, int errorCode, params object[] arguments)
			: this(messageProvider, errorCode)
		{
			_arguments = arguments;
		}

		private DiagnosticInfo(DiagnosticInfo original, DiagnosticSeverity overriddenSeverity)
		{
			_messageProvider = original.MessageProvider;
			_errorCode = original._errorCode;
			_defaultSeverity = original.DefaultSeverity;
			_arguments = original._arguments;

			_effectiveSeverity = overriddenSeverity;
		}

		public static DiagnosticDescriptor GetDescriptor(int errorCode, CommonMessageProvider messageProvider)
		{
			var defaultSeverity = messageProvider.GetSeverity(errorCode);
			return GetOrCreateDescriptor(errorCode, defaultSeverity, messageProvider);
		}

		private static DiagnosticDescriptor GetOrCreateDescriptor(int errorCode, DiagnosticSeverity defaultSeverity, CommonMessageProvider messageProvider)
		{
			DiagnosticDescriptor result;
			if (!s_errorCodeToDescriptorMap.TryGetValue(errorCode, out result))
			{
				result = CreateDescriptor(errorCode, defaultSeverity, messageProvider);
				s_errorCodeToDescriptorMap.Add(errorCode, result);
			}
			return result;
		}

		private static DiagnosticDescriptor CreateDescriptor(int errorCode, DiagnosticSeverity defaultSeverity, CommonMessageProvider messageProvider)
		{
			var id = messageProvider.GetIdForErrorCode(errorCode);
			var title = messageProvider.GetTitle(errorCode);
			var description = messageProvider.GetDescription(errorCode);
			var messageFormat = messageProvider.GetMessageFormat(errorCode);
			var helpLink = messageProvider.GetHelpLink(errorCode);
			var category = messageProvider.GetCategory(errorCode);
			var customTags = GetCustomTags(defaultSeverity);
			return new DiagnosticDescriptor(id, title, messageFormat, category, defaultSeverity,
				isEnabledByDefault: true, description: description, helpLinkUri: helpLink, customTags: customTags);
		}

		public DiagnosticInfo(CommonMessageProvider messageProvider, bool isWarningAsError, int errorCode, params object[] arguments)
			: this(messageProvider, errorCode, arguments)
		{
			Debug.Assert(!isWarningAsError || _defaultSeverity == DiagnosticSeverity.Warning);

			if (isWarningAsError)
			{
				_effectiveSeverity = DiagnosticSeverity.Error;
			}
		}

		public DiagnosticInfo GetInstanceWithSeverity(DiagnosticSeverity severity)
		{
			return new DiagnosticInfo(this, severity);
		}
        public int Code { get { return _errorCode; } }

		public DiagnosticDescriptor Descriptor
		{
			get
			{
				return GetOrCreateDescriptor(_errorCode, _defaultSeverity, _messageProvider);
			}
		}

		/// <summary>
		/// Returns the effective severity of the diagnostic: whether this diagnostic is informational, warning, or error.
		/// If IsWarningsAsError is true, then this returns <see cref="DiagnosticSeverity.Error"/>, while <see cref="DefaultSeverity"/> returns <see cref="DiagnosticSeverity.Warning"/>.
		/// </summary>
		public DiagnosticSeverity Severity
		{
			get
			{
				return _effectiveSeverity;
			}
		}

		/// <summary>
		/// Returns whether this diagnostic is informational, warning, or error by default, based on the error code.
		/// To get diagnostic's effective severity, use <see cref="Severity"/>.
		/// </summary>
		public DiagnosticSeverity DefaultSeverity
		{
			get
			{
				return _defaultSeverity;
			}
		}

		/// <summary>
		/// Gets the warning level. This is 0 for diagnostics with severity <see cref="DiagnosticSeverity.Error"/>,
		/// otherwise an integer between 1 and 4.
		/// </summary>
		public int WarningLevel
		{
			get
			{
				if (_effectiveSeverity != _defaultSeverity)
				{
					return Diagnostic.GetDefaultWarningLevel(_effectiveSeverity);
				}

				return _messageProvider.GetWarningLevel(_errorCode);
			}
		}

		public bool IsWarningAsError
		{
			get
			{
				return this.DefaultSeverity == DiagnosticSeverity.Warning &&
					this.Severity == DiagnosticSeverity.Error;
			}
		}

		/// <summary>
		/// Get the diagnostic category for the given diagnostic code.
		/// Default category is <see cref="Diagnostic.CompilerDiagnosticCategory"/>.
		/// </summary>
		public string Category
		{
			get
			{
				return _messageProvider.GetCategory(_errorCode);
			}
		}

		public string[] CustomTags
		{
			get
			{
				return GetCustomTags(_defaultSeverity);
			}
		}

		private static string[] GetCustomTags(DiagnosticSeverity defaultSeverity)
		{
			return defaultSeverity == DiagnosticSeverity.Error ?
				s_compilerErrorCustomTags :
				s_compilerNonErrorCustomTags;
		}

		public bool IsNotConfigurable()
		{
			// Only compiler errors are non-configurable.
			return _defaultSeverity == DiagnosticSeverity.Error;
		}

		/// <summary>
		/// If a derived class has additional information about other referenced symbols, it can
		/// expose the locations of those symbols in a general way, so they can be reported along
		/// with the error.
		/// </summary>
		public virtual IList<Location> AdditionalLocations
		{
			get
			{
				return new Location[0];
			}
		}

		/// <summary>
		/// Get the message id (for example "CS1001") for the message. This includes both the error number
		/// and a prefix identifying the source.
		/// </summary>
		public string MessageIdentifier
		{
			get
			{
				return _messageProvider.GetIdForErrorCode(_errorCode);
			}
		}

		/// <summary>
		/// Get the text of the message in the given language.
		/// </summary>
		public virtual string GetMessage()
		{
			// Get the message and fill in arguments.
			string message = _messageProvider.LoadMessage(_errorCode);
			if (string.IsNullOrEmpty(message))
			{
				return string.Empty;
			}

			if (_arguments == null || _arguments.Length == 0)
			{
				return message;
			}

			return String.Format(message, GetArgumentsToUse());
		}

		protected object[] GetArgumentsToUse()
		{
			object[] argumentsToUse = null;
			for (int i = 0; i < _arguments.Length; i++)
			{
				var embedded = _arguments[i] as DiagnosticInfo;
				if (embedded != null)
				{
					argumentsToUse = InitializeArgumentListIfNeeded(argumentsToUse);
					argumentsToUse[i] = embedded.GetMessage();
					continue;
				}
			}

			return argumentsToUse ?? _arguments;
		}

		private object[] InitializeArgumentListIfNeeded(object[] argumentsToUse)
		{
			if (argumentsToUse != null)
			{
				return argumentsToUse;
			}

			var newArguments = new object[_arguments.Length];
			Array.Copy(_arguments, newArguments, newArguments.Length);

			return newArguments;
		}

		public object[] Arguments
		{
			get { return _arguments; }
		}

		public CommonMessageProvider MessageProvider
		{
			get { return _messageProvider; }
		}

		// TODO (tomat): remove
		public override string ToString()
		{
			return String.Format("{0}: {1}",
				_messageProvider.GetMessagePrefix(this.MessageIdentifier, this.Severity, this.IsWarningAsError),
				this.GetMessage());
		}

		public sealed override int GetHashCode()
		{
			int hashCode = _errorCode;
			if (_arguments != null)
			{
				for (int i = 0; i < _arguments.Length; i++)
				{
					hashCode = Hash.Combine(_arguments[i], hashCode);
				}
			}

			return hashCode;
		}

		public sealed override bool Equals(object obj)
		{
			DiagnosticInfo other = obj as DiagnosticInfo;

			bool result = false;

			if (other != null &&
				other._errorCode == _errorCode &&
				this.GetType() == obj.GetType())
			{
				if (_arguments == null && other._arguments == null)
				{
					result = true;
				}
				else if (_arguments != null && other._arguments != null && _arguments.Length == other._arguments.Length)
				{
					result = true;
					for (int i = 0; i < _arguments.Length; i++)
					{
						if (!object.Equals(_arguments[i], other._arguments[i]))
						{
							result = false;
							break;
						}
					}
				}
			}

			return result;
		}

		private string GetDebuggerDisplay()
		{
			// There aren't message resources for our public error codes, so make
			// sure we don't call ToString for those.
			switch (Code)
			{
				case InternalErrorCode.Unknown:
					return "Unresolved DiagnosticInfo";

				case InternalErrorCode.Void:
					return "Void DiagnosticInfo";

				default:
					return ToString();
			}
		}

		/// <summary>
		/// For a DiagnosticInfo that is lazily evaluated, this method evaluates it
		/// and returns a non-lazy DiagnosticInfo.
		/// </summary>
		public virtual DiagnosticInfo GetResolvedInfo()
		{
			// We should never call GetResolvedInfo on a non-lazy DiagnosticInfo
			throw ExceptionUtilities.Unreachable;
		}
	}
}
