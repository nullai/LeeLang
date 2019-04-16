using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class ResourceDescription : IFileReference
	{
		public readonly string ResourceName;
		public readonly string FileName; // null if embedded
		public readonly bool IsPublic;
		public readonly Func<Stream> DataProvider;

		/// <summary>
		/// Creates a representation of a resource whose contents are to be embedded in the output assembly.
		/// </summary>
		/// <param name="resourceName">Resource name.</param>
		/// <param name="dataProvider">The callers will dispose the result after use.
		/// This allows the resources to be opened and read one at a time.
		/// </param>
		/// <param name="isPublic">True if the resource is public.</param>
		/// <remarks>
		/// Returns a stream of the data to embed.
		/// </remarks> 
		public ResourceDescription(string resourceName, Func<Stream> dataProvider, bool isPublic)
			: this(resourceName, null, dataProvider, isPublic, isEmbedded: true, checkArgs: true)
		{
		}

		/// <summary>
		/// Creates a representation of a resource whose file name will be recorded in the assembly.
		/// </summary>
		/// <param name="resourceName">Resource name.</param>
		/// <param name="fileName">File name with an extension to be stored in metadata.</param>
		/// <param name="dataProvider">The callers will dispose the result after use.
		/// This allows the resources to be opened and read one at a time.
		/// </param>
		/// <param name="isPublic">True if the resource is public.</param>
		/// <remarks>
		/// Function returning a stream of the resource content (used to calculate hash).
		/// </remarks>
		public ResourceDescription(string resourceName, string fileName, Func<Stream> dataProvider, bool isPublic)
			: this(resourceName, fileName, dataProvider, isPublic, isEmbedded: false, checkArgs: true)
		{
		}

		public ResourceDescription(string resourceName, string fileName, Func<Stream> dataProvider, bool isPublic, bool isEmbedded, bool checkArgs)
		{
			if (checkArgs)
			{
				if (dataProvider == null)
				{
					throw new ArgumentNullException(nameof(dataProvider));
				}

				if (resourceName == null)
				{
					throw new ArgumentNullException(nameof(resourceName));
				}

				if (!MetadataHelpers.IsValidMetadataIdentifier(resourceName))
				{
					throw new ArgumentException("@@EmptyOrInvalidResourceName", nameof(resourceName));
				}

				if (!isEmbedded)
				{
					if (fileName == null)
					{
						throw new ArgumentNullException(nameof(fileName));
					}

					if (!MetadataHelpers.IsValidMetadataFileName(fileName))
					{
						throw new ArgumentException("@@EmptyOrInvalidFileName", nameof(fileName));
					}
				}
			}

			this.ResourceName = resourceName;
			this.DataProvider = dataProvider;
			this.FileName = isEmbedded ? null : fileName;
			this.IsPublic = isPublic;
		}

		public bool IsEmbedded
		{
			get { return FileName == null; }
		}

		//public ManagedResource ToManagedResource(CommonPEModuleBuilder moduleBeingBuilt)
		//{
		//	return new ManagedResource(ResourceName, IsPublic, IsEmbedded ? DataProvider : null, IsEmbedded ? null : this, offset: 0);
		//}

		byte[] IFileReference.GetHashValue()
		{
			try
			{
				using (var stream = DataProvider())
				{
					if (stream == null)
					{
						throw new InvalidOperationException("@@ResourceDataProviderShouldReturnNonNullStream");
					}

					return Hash.ComputeHash(stream);
				}
			}
			catch (Exception ex)
			{
				throw new Exception(FileName, ex);
			}
		}

		string IFileReference.FileName
		{
			get { return FileName; }
		}

		bool IFileReference.HasMetadata
		{
			get { return false; }
		}
	}
}
