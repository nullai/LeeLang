using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace leec
{
	internal struct CodeAnalysisResourcesLocalizableErrorArgument : IFormattable
	{
		private readonly string _targetResourceId;

		internal CodeAnalysisResourcesLocalizableErrorArgument(string targetResourceId)
		{
			Debug.Assert(targetResourceId != null);
			_targetResourceId = targetResourceId;
		}

		public override string ToString()
		{
			return ToString(null, null);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (_targetResourceId != null)
			{
				throw new Exception();
				//return CodeAnalysisResources.ResourceManager.GetString(_targetResourceId, formatProvider as System.Globalization.CultureInfo);
			}

			return string.Empty;
		}
	}
}
