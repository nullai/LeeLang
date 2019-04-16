using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class DllImportData : IPlatformInvokeInformation
	{
		private readonly string _moduleName;
		private readonly string _entryPointName;            // null if unspecified, the name of the target method should be used
		private readonly MethodImportAttributes _flags;

		public DllImportData(string moduleName, string entryPointName, MethodImportAttributes flags)
		{
			_moduleName = moduleName;
			_entryPointName = entryPointName;
			_flags = flags;
		}

		/// <summary>
		/// Module name. Null if value specified in the attribute is not valid.
		/// </summary>
		public string ModuleName
		{
			get { return _moduleName; }
		}

		/// <summary>
		/// Name of the native entry point or null if not specified (the effective name is the same as the name of the target method).
		/// </summary>
		public string EntryPointName
		{
			get { return _entryPointName; }
		}

		MethodImportAttributes IPlatformInvokeInformation.Flags
		{
			get { return _flags; }
		}

		/// <summary>
		/// Indicates whether the callee calls the SetLastError Win32 API function before returning from the attributed method.
		/// </summary>
		public bool SetLastError
		{
			get
			{
				return (_flags & MethodImportAttributes.SetLastError) != 0;
			}
		}

		/// <summary>
		/// Indicates the calling convention of an entry point.
		/// </summary>
		public CallingConvention CallingConvention
		{
			get
			{
				switch (_flags & MethodImportAttributes.CallingConventionMask)
				{
					default:
						return CallingConvention.Standard;

					case MethodImportAttributes.CallingConventionCDecl:
						return CallingConvention.C;

					case MethodImportAttributes.CallingConventionStdCall:
						return CallingConvention.Standard;

					case MethodImportAttributes.CallingConventionThisCall:
						return CallingConvention.ThisCall;

					case MethodImportAttributes.CallingConventionFastCall:
						return CallingConvention.FastCall;
				}
			}
		}

		public static MethodImportAttributes MakeFlags(CallingConvention callingConvention)
		{
			MethodImportAttributes result = 0;

			switch (callingConvention)
			{
				default: // Dev10: uses default without reporting an error
					result |= MethodImportAttributes.CallingConventionStdCall;
					break;

				case CallingConvention.C:
					result |= MethodImportAttributes.CallingConventionCDecl;
					break;

				case CallingConvention.Standard:
					result |= MethodImportAttributes.CallingConventionStdCall;
					break;

				case CallingConvention.ThisCall:
					result |= MethodImportAttributes.CallingConventionThisCall;
					break;

				case CallingConvention.FastCall:
					result |= MethodImportAttributes.CallingConventionFastCall;
					break;
			}
			return result;
		}
	}
}
