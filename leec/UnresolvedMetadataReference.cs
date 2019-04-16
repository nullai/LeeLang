using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class UnresolvedMetadataReference : MetadataReference
	{
		public string Reference { get; }

		public UnresolvedMetadataReference(string reference, MetadataReferenceProperties properties)
			: base(properties)
		{
			this.Reference = reference;
		}

		public override string Display
		{
			get
			{
				return "@@Unresolved" + Reference;
			}
		}

		public override bool IsUnresolved
		{
			get { return true; }
		}

		public override MetadataReference WithPropertiesImplReturningMetadataReference(MetadataReferenceProperties properties)
		{
			return new UnresolvedMetadataReference(this.Reference, properties);
		}
	}
}
