using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	[System.FlagsAttribute]
	public enum AssemblyFlags
	{
		ContentTypeMask = 3584,
		DisableJitCompileOptimizer = 16384,
		EnableJitCompileTracking = 32768,
		PublicKey = 1,
		Retargetable = 256,
		WindowsRuntime = 512,
	}
	public enum AssemblyHashAlgorithm
	{
		MD5 = 32771,
		None = 0,
		Sha1 = 32772,
		Sha256 = 32780,
		Sha384 = 32781,
		Sha512 = 32782,
	}
	public enum DeclarativeSecurityAction : short
	{
		Assert = (short)3,
		Demand = (short)2,
		Deny = (short)4,
		InheritanceDemand = (short)7,
		LinkDemand = (short)6,
		None = (short)0,
		PermitOnly = (short)5,
		RequestMinimum = (short)8,
		RequestOptional = (short)9,
		RequestRefuse = (short)10,
	}
	[System.FlagsAttribute]
	public enum ManifestResourceAttributes
	{
		Private = 2,
		Public = 1,
		VisibilityMask = 7,
	}
	[System.FlagsAttribute]
	public enum MethodImportAttributes : short
	{
		BestFitMappingDisable = (short)32,
		BestFitMappingEnable = (short)16,
		BestFitMappingMask = (short)48,
		CallingConventionCDecl = (short)512,
		CallingConventionFastCall = (short)1280,
		CallingConventionMask = (short)1792,
		CallingConventionStdCall = (short)768,
		CallingConventionThisCall = (short)1024,
		CallingConventionWinApi = (short)256,
		CharSetAnsi = (short)2,
		CharSetAuto = (short)6,
		CharSetMask = (short)6,
		CharSetUnicode = (short)4,
		ExactSpelling = (short)1,
		None = (short)0,
		SetLastError = (short)64,
		ThrowOnUnmappableCharDisable = (short)8192,
		ThrowOnUnmappableCharEnable = (short)4096,
		ThrowOnUnmappableCharMask = (short)12288,
	}
	[System.FlagsAttribute]
	public enum MethodSemanticsAttributes
	{
		Adder = 8,
		Getter = 2,
		Other = 4,
		Raiser = 32,
		Remover = 16,
		Setter = 1,
	}

	public enum Characteristics : ushort
	{
		AggressiveWSTrim = (ushort)16,
		Bit32Machine = (ushort)256,
		BytesReversedHi = (ushort)32768,
		BytesReversedLo = (ushort)128,
		DebugStripped = (ushort)512,
		Dll = (ushort)8192,
		ExecutableImage = (ushort)2,
		LargeAddressAware = (ushort)32,
		LineNumsStripped = (ushort)4,
		LocalSymsStripped = (ushort)8,
		NetRunFromSwap = (ushort)2048,
		RelocsStripped = (ushort)1,
		RemovableRunFromSwap = (ushort)1024,
		System = (ushort)4096,
		UpSystemOnly = (ushort)16384,
	}

	public enum Machine : ushort
	{
		Alpha = (ushort)388,
		Alpha64 = (ushort)644,
		AM33 = (ushort)467,
		Amd64 = (ushort)34404,
		Arm = (ushort)448,
		Arm64 = (ushort)43620,
		ArmThumb2 = (ushort)452,
		Ebc = (ushort)3772,
		I386 = (ushort)332,
		IA64 = (ushort)512,
		M32R = (ushort)36929,
		MIPS16 = (ushort)614,
		MipsFpu = (ushort)870,
		MipsFpu16 = (ushort)1126,
		PowerPC = (ushort)496,
		PowerPCFP = (ushort)497,
		SH3 = (ushort)418,
		SH3Dsp = (ushort)419,
		SH3E = (ushort)420,
		SH4 = (ushort)422,
		SH5 = (ushort)424,
		Thumb = (ushort)450,
		Tricore = (ushort)1312,
		Unknown = (ushort)0,
		WceMipsV2 = (ushort)361,
	}

	public enum Subsystem : ushort
	{
		EfiApplication = (ushort)10,
		EfiBootServiceDriver = (ushort)11,
		EfiRom = (ushort)13,
		EfiRuntimeDriver = (ushort)12,
		Native = (ushort)1,
		NativeWindows = (ushort)8,
		OS2Cui = (ushort)5,
		PosixCui = (ushort)7,
		Unknown = (ushort)0,
		WindowsBootApplication = (ushort)16,
		WindowsCEGui = (ushort)9,
		WindowsCui = (ushort)3,
		WindowsGui = (ushort)2,
		Xbox = (ushort)14,
	}

	[System.FlagsAttribute]
	public enum CorFlags
	{
		ILLibrary = 4,
		ILOnly = 1,
		NativeEntryPoint = 16,
		Prefers32Bit = 131072,
		Requires32Bit = 2,
		StrongNameSigned = 8,
		TrackDebugData = 65536,
	}

	[System.FlagsAttribute]
	public enum DllCharacteristics : ushort
	{
		AppContainer = (ushort)4096,
		DynamicBase = (ushort)64,
		HighEntropyVirtualAddressSpace = (ushort)32,
		NoBind = (ushort)2048,
		NoIsolation = (ushort)512,
		NoSeh = (ushort)1024,
		NxCompatible = (ushort)256,
		ProcessInit = (ushort)1,
		ProcessTerm = (ushort)2,
		TerminalServerAware = (ushort)32768,
		ThreadInit = (ushort)4,
		ThreadTerm = (ushort)8,
		WdmDriver = (ushort)8192,
	}

	public sealed class MetadataId
	{
		private MetadataId()
		{
		}

		public static MetadataId CreateNewId() => new MetadataId();
	}

	/// <summary>
	/// Represents immutable assembly or module CLI metadata.
	/// </summary>
	public abstract class Metadata : IDisposable
	{
		public readonly bool IsImageOwner;

		/// <summary>
		/// The id for this metadata instance.  If two metadata instances have the same id, then 
		/// they have the same content.  If they have different ids they may or may not have the
		/// same content.
		/// </summary>
		public MetadataId Id { get; }

		public Metadata(bool isImageOwner, MetadataId id)
		{
			this.IsImageOwner = isImageOwner;
			this.Id = id;
		}

		/// <summary>
		/// Retrieves the <see cref="MetadataImageKind"/> for this instance.
		/// </summary>
		public abstract MetadataImageKind Kind { get; }

		/// <summary>
		/// Releases any resources associated with this instance.
		/// </summary>
		public abstract void Dispose();

		protected abstract Metadata CommonCopy();

		/// <summary>
		/// Creates a copy of this object.
		/// </summary>
		public Metadata Copy()
		{
			return CommonCopy();
		}
	}
}
