﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace leec
{
	public abstract class PortableExecutableReference : MetadataReference
	{
		private readonly string _filePath;

		protected PortableExecutableReference(
			MetadataReferenceProperties properties,
			string fullPath = null)
			: base(properties)
		{
			_filePath = fullPath;
		}

		/// <summary>
		/// Display string used in error messages to identity the reference.
		/// </summary>
		public override string Display
		{
			get { return FilePath; }
		}

		/// <summary>
		/// Path describing the location of the metadata, or null if the metadata have no location.
		/// </summary>
		public string FilePath
		{
			get { return _filePath; }
		}

		/// <summary>
		/// Returns an instance of the reference with specified aliases.
		/// </summary>
		/// <param name="aliases">The new aliases for the reference.</param>
		/// <exception cref="ArgumentException">Alias is invalid for the metadata kind.</exception> 
		public new PortableExecutableReference WithAliases(IEnumerable<string> aliases)
		{
			return this.WithAliases(aliases.ToArray());
		}

		/// <summary>
		/// Returns an instance of the reference with specified aliases.
		/// </summary>
		/// <param name="aliases">The new aliases for the reference.</param>
		/// <exception cref="ArgumentException">Alias is invalid for the metadata kind.</exception> 
		public new PortableExecutableReference WithAliases(string[] aliases)
		{
			return WithProperties(Properties.WithAliases(aliases));
		}

		/// <summary>
		/// Returns an instance of the reference with specified interop types embedding.
		/// </summary>
		/// <param name="value">The new value for <see cref="MetadataReferenceProperties.EmbedInteropTypes"/>.</param>
		/// <exception cref="ArgumentException">Interop types can't be embedded from modules.</exception> 
		public new PortableExecutableReference WithEmbedInteropTypes(bool value)
		{
			return WithProperties(Properties.WithEmbedInteropTypes(value));
		}

		/// <summary>
		/// Returns an instance of the reference with specified properties, or this instance if properties haven't changed.
		/// </summary>
		/// <param name="properties">The new properties for the reference.</param>
		/// <exception cref="ArgumentException">Specified values not valid for this reference.</exception> 
		public new PortableExecutableReference WithProperties(MetadataReferenceProperties properties)
		{
			if (properties == this.Properties)
			{
				return this;
			}

			return WithPropertiesImpl(properties);
		}

		public sealed override MetadataReference WithPropertiesImplReturningMetadataReference(MetadataReferenceProperties properties)
		{
			return WithPropertiesImpl(properties);
		}

		/// <summary>
		/// Returns an instance of the reference with specified properties.
		/// </summary>
		/// <param name="properties">The new properties for the reference.</param>
		/// <exception cref="NotSupportedException">Specified values not supported.</exception> 
		/// <remarks>Only invoked if the properties changed.</remarks>
		protected abstract PortableExecutableReference WithPropertiesImpl(MetadataReferenceProperties properties);

		/// <summary>
		/// Get metadata representation for the PE file.
		/// </summary>
		/// <exception cref="BadImageFormatException">If the PE image format is invalid.</exception>
		/// <exception cref="IOException">The metadata image content can't be read.</exception>
		/// <exception cref="FileNotFoundException">The metadata image is stored in a file that can't be found.</exception>
		/// <remarks>
		/// Called when the <see cref="Compilation"/> needs to read the reference metadata.
		/// 
		/// The listed exceptions are caught and converted to compilation diagnostics.
		/// Any other exception is considered an unexpected error in the implementation and is not caught.
		///
		/// <see cref="Metadata"/> objects may cache information decoded from the PE image.
		/// Reusing <see cref="Metadata"/> instances across metadata references will result in better performance.
		/// 
		/// The calling <see cref="Compilation"/> doesn't take ownership of the <see cref="Metadata"/> objects returned by this method.
		/// The implementation needs to retrieve the object from a provider that manages their lifetime (such as metadata cache).
		/// The <see cref="Metadata"/> object is kept alive by the <see cref="Compilation"/> that called <see cref="GetMetadataNoCopy"/>
		/// and by all compilations created from it via calls to With- factory methods on <see cref="Compilation"/>, 
		/// other than <see cref="Compilation.WithReferences(MetadataReference[])"/> overloads. A compilation created using 
		/// <see cref="Compilation.WithReferences(MetadataReference[])"/> will call to <see cref="GetMetadataNoCopy"/> again.
		/// </remarks>
		protected abstract Metadata GetMetadataImpl();

		public Metadata GetMetadataNoCopy()
		{
			return GetMetadataImpl();
		}

		/// <summary>
		/// Returns a copy of the <see cref="Metadata"/> object this <see cref="PortableExecutableReference"/>
		/// contains.  This copy does not need to be <see cref="IDisposable.Dispose"/>d.
		/// </summary>
		/// <exception cref="BadImageFormatException">If the PE image format is invalid.</exception>
		/// <exception cref="IOException">The metadata image content can't be read.</exception>
		/// <exception cref="FileNotFoundException">The metadata image is stored in a file that can't be found.</exception>
		public Metadata GetMetadata()
		{
			return GetMetadataNoCopy().Copy();
		}

		/// <summary>
		/// Returns the <see cref="MetadataId"/> for this reference's <see cref="Metadata"/>.
		/// This will be equivalent to calling <see cref="GetMetadata()"/>.<see cref="Metadata.Id"/>,
		/// but can be done more efficiently.
		/// </summary>
		/// <exception cref="BadImageFormatException">If the PE image format is invalid.</exception>
		/// <exception cref="IOException">The metadata image content can't be read.</exception>
		/// <exception cref="FileNotFoundException">The metadata image is stored in a file that can't be found.</exception>
		public MetadataId GetMetadataId()
		{
			return GetMetadataNoCopy().Id;
		}

		public static Diagnostic ExceptionToDiagnostic(Exception e, CommonMessageProvider messageProvider, Location location, string display, MetadataImageKind kind)
		{
			if (e is BadImageFormatException)
			{
				int errorCode = (kind == MetadataImageKind.Assembly) ? messageProvider.ERR_InvalidAssemblyMetadata : messageProvider.ERR_InvalidModuleMetadata;
				return messageProvider.CreateDiagnostic(errorCode, location, display, e.Message);
			}

			var fileNotFound = e as FileNotFoundException;
			if (fileNotFound != null)
			{
				return messageProvider.CreateDiagnostic(messageProvider.ERR_MetadataFileNotFound, location, fileNotFound.FileName ?? string.Empty);
			}
			else
			{
				int errorCode = (kind == MetadataImageKind.Assembly) ? messageProvider.ERR_ErrorOpeningAssemblyFile : messageProvider.ERR_ErrorOpeningModuleFile;
				return messageProvider.CreateDiagnostic(errorCode, location, display, e.Message);
			}
		}
	}
}
