﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leec
{
	public sealed class ExternalFileLocation : Location, IEquatable<ExternalFileLocation>
	{
		private readonly TextSpan _sourceSpan;
		private readonly FileLinePositionSpan _lineSpan;

		public ExternalFileLocation(string filePath, TextSpan sourceSpan, LinePositionSpan lineSpan)
		{
			_sourceSpan = sourceSpan;
			_lineSpan = new FileLinePositionSpan(filePath, lineSpan);
		}

		public override TextSpan SourceSpan
		{
			get
			{
				return _sourceSpan;
			}
		}

		public override FileLinePositionSpan GetLineSpan()
		{
			return _lineSpan;
		}

		public override FileLinePositionSpan GetMappedLineSpan()
		{
			return _lineSpan;
		}

		public override LocationKind Kind
		{
			get
			{
				return LocationKind.ExternalFile;
			}
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as ExternalFileLocation);
		}

		public bool Equals(ExternalFileLocation obj)
		{
			if (ReferenceEquals(obj, this))
			{
				return true;
			}

			return obj != null
				&& _sourceSpan == obj._sourceSpan
				&& _lineSpan.Equals(obj._lineSpan);
		}

		public override int GetHashCode()
		{
			return Hash.Combine(_lineSpan.GetHashCode(), _sourceSpan.GetHashCode());
		}
	}
}
