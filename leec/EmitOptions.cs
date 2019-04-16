using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class EmitOptions
	{
		public static readonly EmitOptions Default = new EmitOptions();

		/// <summary>
		/// True to emit an assembly excluding executable code such as method bodies.
		/// </summary>
		public bool EmitMetadataOnly { get; set; }

		/// <summary>
		/// Tolerate errors, producing a PE stream and a success result even in the presence of (some) errors. 
		/// </summary>
		public bool TolerateErrors { get; set; }

		/// <summary>
		/// Unless set (private) members that don't affect the language semantics of the resulting assembly will be excluded
		/// when emitting metadata-only assemblies as primary output (with <see cref="EmitMetadataOnly"/> on).
		/// If emitting a secondary output, this flag is required to be false.
		/// </summary>
		public bool IncludePrivateMembers { get; set; }

		public SubsystemVersion SubsystemVersion { get; set; }
		/// <summary>
		/// Specifies the size of sections in the output file. 
		/// </summary>
		/// <remarks>
		/// Valid values are 0, 512, 1024, 2048, 4096 and 8192.
		/// If the value is 0 the file alignment is determined based upon the value of <see cref="Platform"/>.
		/// </remarks>
		public int FileAlignment { get; set; }

		/// <summary>
		/// True to enable high entropy virtual address space for the output binary.
		/// </summary>
		public bool HighEntropyVirtualAddressSpace { get; set; }

		/// <summary>
		/// Specifies the preferred base address at which to load the output DLL.
		/// </summary>
		public ulong BaseAddress { get; set; }

		/// <summary>
		/// Assembly name override - file name and extension. If not specified the compilation name is used.
		/// </summary>
		/// <remarks>
		/// By default the name of the output assembly is <see cref="Compilation.AssemblyName"/>. Only in rare cases it is necessary
		/// to override the name.
		/// 
		/// CAUTION: If this is set to a (non-null) value other than the existing compilation output name, then internals-visible-to
		/// and assembly references may not work as expected.  In particular, things that were visible at bind time, based on the 
		/// name of the compilation, may not be visible at runtime and vice-versa.
		/// </remarks>
		public string OutputNameOverride { get; set; }

		/// <summary>
		/// The name of the PDB file to be embedded in the PE image, or null to use the default.
		/// </summary>
		/// <remarks>
		/// If not specified the file name of the source module with an extension changed to "pdb" is used.
		/// </remarks>
		public string PdbFilePath { get; set; }

		/// <summary>
		/// Runtime metadata version. 
		/// </summary>
		public string RuntimeMetadataVersion { get; set; }

		public EmitOptions(
			bool metadataOnly = false,
			string pdbFilePath = null,
			string outputNameOverride = null,
			int fileAlignment = 0,
			ulong baseAddress = 0,
			bool highEntropyVirtualAddressSpace = false,
			SubsystemVersion subsystemVersion = default(SubsystemVersion),
			string runtimeMetadataVersion = null,
			bool tolerateErrors = false,
			bool includePrivateMembers = true)
		{
			EmitMetadataOnly = metadataOnly;
			PdbFilePath = pdbFilePath;
			OutputNameOverride = outputNameOverride;
			FileAlignment = fileAlignment;
			BaseAddress = baseAddress;
			HighEntropyVirtualAddressSpace = highEntropyVirtualAddressSpace;
			SubsystemVersion = subsystemVersion;
			RuntimeMetadataVersion = runtimeMetadataVersion;
			TolerateErrors = tolerateErrors;
			IncludePrivateMembers = includePrivateMembers;
		}

		public void ValidateOptions(DiagnosticBag diagnostics, CommonMessageProvider messageProvider, bool isDeterministic)
		{
			if (OutputNameOverride != null)
			{
				MetadataHelpers.CheckAssemblyOrModuleName(OutputNameOverride, messageProvider, messageProvider.ERR_InvalidOutputName, diagnostics);
			}

			if (FileAlignment != 0 && !IsValidFileAlignment(FileAlignment))
			{
				diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_InvalidFileAlignment, Location.None, FileAlignment));
			}

			if (!SubsystemVersion.Equals(SubsystemVersion.None) && !SubsystemVersion.IsValid)
			{
				diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_InvalidSubsystemVersion, Location.None, SubsystemVersion.ToString()));
			}
		}

		public static bool IsValidFileAlignment(int value)
		{
			switch (value)
			{
				case 512:
				case 1024:
				case 2048:
				case 4096:
				case 8192:
					return true;

				default:
					return false;
			}
		}
	}
}
